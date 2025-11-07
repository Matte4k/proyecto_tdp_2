using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace proyecto_tdp_2.MVVM.View
{
    public partial class BackupView : UserControl
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["MiReclamoDB"].ConnectionString;
        private string backupFolder;
        public BackupView()
        {
            InitializeComponent();
            backupFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backups");
            if (!Directory.Exists(backupFolder))
                Directory.CreateDirectory(backupFolder);

            Loaded += BackupView_Loaded;
        }

        public class BackupInfo
        {
            public string Nombre { get; set; }
            public string Ruta { get; set; }
            public string Fecha { get; set; }
        }

        private void BackupView_Loaded(object sender, RoutedEventArgs e)
        {
            CargarListaBackups();
        }

        private void CargarListaBackups()
        {
            try
            {
                if (!Directory.Exists(backupFolder))
                {
                    MessageBox.Show("No se encontró la carpeta de backups.");
                    return;
                }

                var archivos = Directory.GetFiles(backupFolder, "mireclamo_backup_*.bak");
                var lista = new ObservableCollection<BackupInfo>();

                foreach (var archivo in archivos)
                {
                    FileInfo info = new FileInfo(archivo);
                    lista.Add(new BackupInfo
                    {
                        Nombre = info.Name,
                        Ruta = info.FullName,
                        Fecha = info.CreationTime.ToString("dd/MM/yyyy HH:mm")
                    });
                }

                BackupsList.ItemsSource = lista;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar la lista de backups: {ex.Message}");
            }
        }

        private void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (BackupsList.SelectedItem is not BackupInfo selected)
            {
                MessageBox.Show("Selecciona un backup para eliminar.", "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show($"¿Seguro que deseas eliminar el backup:\n\n{selected.Nombre}?",
                                "Confirmar eliminación",
                                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    if (File.Exists(selected.Ruta))
                    {
                        File.Delete(selected.Ruta);
                        MessageBox.Show("Backup eliminado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                        CargarListaBackups();
                    }
                    else
                    {
                        MessageBox.Show("No se encontró el archivo del backup.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al eliminar el backup:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnRestaurar_Click(object sender, RoutedEventArgs e)
        {
            if (BackupsList.SelectedItem is not BackupInfo backupSeleccionado)
            {
                MessageBox.Show("Selecciona un backup para restaurar.", "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirm = MessageBox.Show(
                $"¿Estás seguro de restaurar la base de datos desde:\n\n{backupSeleccionado.Nombre}?\n\n" +
                "Se reemplazará completamente la base de datos actual.",
                "Confirmar restauración",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes)
                return;

            try
            {
                var builder = new SqlConnectionStringBuilder(connectionString)
                {
                    InitialCatalog = "master"
                };

                using (SqlConnection conn = new SqlConnection(builder.ConnectionString))
                {
                    conn.Open();

                    string setSingleUser = @"
                ALTER DATABASE mireclamo SET SINGLE_USER WITH ROLLBACK IMMEDIATE;";

                    string restoreQuery = @"
                RESTORE DATABASE mireclamo
                FROM DISK = @ruta
                WITH REPLACE;";

                    string setMultiUser = @"
                ALTER DATABASE mireclamo SET MULTI_USER;";

                    using (SqlCommand cmd = new SqlCommand(setSingleUser, conn))
                        cmd.ExecuteNonQuery();

                    using (SqlCommand cmd = new SqlCommand(restoreQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@ruta", backupSeleccionado.Ruta);
                        cmd.ExecuteNonQuery();
                    }

                    using (SqlCommand cmd = new SqlCommand(setMultiUser, conn))
                        cmd.ExecuteNonQuery();
                }

                MessageBox.Show("La base de datos fue restaurada correctamente.\n\nLa aplicación se cerrará para aplicar los cambios.",
                                "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al restaurar la base de datos:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private void BtnExaminar_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Title = "Guardar copia de seguridad",
                Filter = "Archivo de respaldo (*.bak)|*.bak",
                InitialDirectory = backupFolder,
                FileName = $"mireclamo_backup_{DateTime.Now:yyyyMMdd_HHmmss}.bak"
            };

            if (dialog.ShowDialog() == true)
                txtRutaBackup.Text = dialog.FileName;
        }

        private void BtnBackup_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtRutaBackup.Text))
            {
                MessageBox.Show("Por favor selecciona una ruta de destino para el backup.", "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string backupPath = txtRutaBackup.Text;

                    string query = $@"
                        BACKUP DATABASE mireclamo
                        TO DISK = @ruta
                        WITH FORMAT, INIT,
                        NAME = 'Backup de mireclamo',
                        SKIP, NOREWIND, NOUNLOAD, STATS = 10;";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ruta", backupPath);
                        cmd.ExecuteNonQuery();
                    }
                }

                txtResultado.Text = "Backup generado correctamente.";
                txtResultado.Visibility = Visibility.Visible;
                CargarListaBackups();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al realizar el backup:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
