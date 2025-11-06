using Microsoft.Win32;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace proyecto_tdp_2.MVVM.View
{
    public partial class RegisterView : UserControl
    {
        private string _avatarFilePath = string.Empty;
        string connectionString = ConfigurationManager.ConnectionStrings["MiReclamoDB"].ConnectionString;

        public RegisterView()
        {
            InitializeComponent();
            LoadCompanies();
            CargarProvincias();

            if (Session.Rol == "SuperAdmin")
            {
                SuperAdmin.Visibility = Visibility.Visible;
                Supervisor.Visibility = Visibility.Visible;
            }
        }

        private void LoadCompanies()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT id_servicio, nombre FROM Servicios";

                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        var companies = new List<dynamic>();
                        while (reader.Read())
                        {
                            companies.Add(new
                            {
                                id_servicio = reader.GetInt32(0),
                                nombre = reader.GetString(1)
                            });
                        }

                        cbCompany.ItemsSource = companies;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar las empresas: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CargarProvincias()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT id_provincia, nombre FROM Provincia ORDER BY nombre";

                    SqlCommand cmd = new SqlCommand(query, connection);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    cbProvince.ItemsSource = dt.DefaultView;
                    cbProvince.DisplayMemberPath = "nombre";
                    cbProvince.SelectedValuePath = "id_provincia";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar provincias: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnUploadPhoto_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog()
            {
                Filter = "Imágenes|*.png;*.jpg;*.jpeg;*.bmp;*.gif",
                Title = "Seleccionar foto de perfil"
            };

            if (dlg.ShowDialog() == true)
            {
                _avatarFilePath = dlg.FileName;
                try
                {
                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.UriSource = new Uri(_avatarFilePath);
                    bmp.EndInit();
                    AvatarEllipse.Fill = new ImageBrush(bmp) { Stretch = Stretch.UniformToFill };
                }
                catch (Exception ex)
                {
                    MessageBox.Show("No se pudo cargar la imagen: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnRemovePhoto_Click(object sender, RoutedEventArgs e)
        {
            _avatarFilePath = string.Empty;
            AvatarEllipse.Fill = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Images/man.png"))) { Stretch = Stretch.UniformToFill };
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string nombre = tbFullName.Text.Trim();
                string apellido = tbSurName.Text.Trim();
                string email = tbEmail.Text.Trim();
                string password = pbPassword.Password;
                string confirm = pbConfirm.Password;
                string telefono = tbPhone.Text.Trim();
                string dni = tbDNI.Text.Trim();
                string cuit = tbCUIT.Text.Trim();
                int idProvincia = (int)(cbProvince.SelectedValue ?? 0);
                int idServicio = (int)(cbCompany.SelectedValue ?? 0);

                string rolTexto = (cbRole.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Operador";
                int idRol = rolTexto switch
                {
                    "SuperAdmin" => 1,
                    "Supervisor" => 2,
                    "Operador" => 3,
                    _ => 3
                };

                if (string.IsNullOrWhiteSpace(nombre))
                {
                    MessageBox.Show("El nombre es obligatorio.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    tbFullName.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(apellido))
                {
                    MessageBox.Show("El apellido es obligatorio.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    tbSurName.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(dni) || dni.Length != 8)
                {
                    MessageBox.Show("El DNI debe tener 8 dígitos.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    tbDNI.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(cuit) || cuit.Length != 11)
                {
                    MessageBox.Show("El CUIT debe tener 11 dígitos.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    tbCUIT.Focus();
                    return;
                }

                if (idServicio == 0)
                {
                    MessageBox.Show("Debes seleccionar una empresa.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    cbCompany.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(email))
                {
                    MessageBox.Show("El correo electrónico es obligatorio.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    tbEmail.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(password))
                {
                    MessageBox.Show("La contraseña es obligatoria.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    pbPassword.Focus();
                    return;
                }

                if (password != confirm)
                {
                    MessageBox.Show("Las contraseñas no coinciden.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    pbConfirm.Focus();
                    return;
                }



                int nuevoIdUsuario = 0;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = @"INSERT INTO Usuario 
                                (nombre, apellido, email, password, telefono, dni, cuit, id_rol, id_servicio, id_provincia)
                                OUTPUT INSERTED.id_usuario
                                VALUES 
                                (@nombre, @apellido, @correo, @pass, @telefono, @dni, @cuit, @rol, @servicio, @provincia)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@nombre", nombre);
                        command.Parameters.AddWithValue("@apellido", apellido);
                        command.Parameters.AddWithValue("@correo", email);
                        command.Parameters.AddWithValue("@pass", password);
                        command.Parameters.AddWithValue("@telefono", telefono);
                        command.Parameters.AddWithValue("@dni", dni);
                        command.Parameters.AddWithValue("@cuit", cuit);
                        command.Parameters.AddWithValue("@rol", idRol);
                        command.Parameters.AddWithValue("@servicio", idServicio);
                        command.Parameters.AddWithValue("@provincia", idProvincia);

                        nuevoIdUsuario = (int)command.ExecuteScalar();
                    }

                    if (!string.IsNullOrEmpty(_avatarFilePath))
                    {
                        string projectRoot = System.IO.Path.GetFullPath(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\.."));
                        string imagesFolder = System.IO.Path.Combine(projectRoot, "Images", "Users");

                        if (!System.IO.Directory.Exists(imagesFolder))
                            System.IO.Directory.CreateDirectory(imagesFolder);

                        string fileName = $"user_{nuevoIdUsuario}_{System.IO.Path.GetFileName(_avatarFilePath)}";
                        string destPath = System.IO.Path.Combine(imagesFolder, fileName);

                        System.IO.File.Copy(_avatarFilePath, destPath, true);

                        string relativePath = System.IO.Path.Combine("Images", "Users", fileName);

                        using (SqlConnection conn = new SqlConnection(connectionString))
                        {
                            conn.Open();
                            string queryImg = @"INSERT INTO Imagenes (ruta_imagen, id_usuario)
                            VALUES (@ruta, @idUsuario)";
                            using (SqlCommand cmd = new SqlCommand(queryImg, conn))
                            {
                                cmd.Parameters.AddWithValue("@ruta", relativePath); // Guardar ruta relativa
                                cmd.Parameters.AddWithValue("@idUsuario", nuevoIdUsuario);
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }



                    if (idRol == 3)
                    {
                        string querySupervisor = @"
                            SELECT TOP 1 id_usuario 
                            FROM Usuario 
                            WHERE id_servicio = @servicio AND id_rol = 2";

                        int? idSupervisor = null;
                        using (SqlCommand cmd = new SqlCommand(querySupervisor, connection))
                        {
                            cmd.Parameters.AddWithValue("@servicio", idServicio);
                            object result = cmd.ExecuteScalar();
                            if (result != null)
                                idSupervisor = Convert.ToInt32(result);
                        }

                        if (idSupervisor != null)
                        {
                            string queryAsignacion = @"
                                INSERT INTO SupervisorOperador (id_supervisor, id_operador)
                                VALUES (@idSupervisor, @idOperador)";

                            using (SqlCommand cmd2 = new SqlCommand(queryAsignacion, connection))
                            {
                                cmd2.Parameters.AddWithValue("@idSupervisor", idSupervisor);
                                cmd2.Parameters.AddWithValue("@idOperador", nuevoIdUsuario);
                                cmd2.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            MessageBox.Show("No se encontró un supervisor disponible para este servicio. El operador fue creado sin asignación.",
                                "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }

                MessageBox.Show("Usuario creado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Error en la base de datos: " + ex.Message, "SQL Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            tbFullName.Clear();
            tbSurName.Clear();
            tbEmail.Clear();
            pbPassword.Clear();
            pbConfirm.Clear();
            tbPhone.Clear();
            tbDNI.Clear();
            tbCUIT.Clear();
            cbCompany.SelectedIndex = -1;
            cbProvince.SelectedIndex = -1;
            cbRole.SelectedIndex = 0;
        }

        private void tbDNI_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            Regex regex = new Regex("[^0-9]+");
            if (regex.IsMatch(e.Text)) { e.Handled = true; return; }
            if (textBox != null && textBox.Text.Length >= 8) e.Handled = true;
        }

        private void tbCUIT_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            Regex regex = new Regex("[^0-9]+");
            if (regex.IsMatch(e.Text)) { e.Handled = true; return; }
            if (textBox != null && textBox.Text.Length >= 11) e.Handled = true;
        }

        private void tbPhone_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void tbName_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^a-zA-ZáéíóúÁÉÍÓÚüÜñÑ ]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void tbSurName_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^a-zA-ZáéíóúÁÉÍÓÚüÜñÑ ]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
