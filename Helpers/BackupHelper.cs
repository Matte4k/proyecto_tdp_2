using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Windows;

namespace proyecto_tdp_2.Helpers
{
    public static class BackupHelper
    {
        public static void CrearBackup()
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["MiReclamoDB"].ConnectionString;

                string backupDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backups");
                if (!Directory.Exists(backupDir))
                    Directory.CreateDirectory(backupDir);

                string fileName = $"mireclamo_backup_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.bak";
                string fullPath = Path.Combine(backupDir, fileName);

                string dbName;
                using (SqlConnection tempConn = new SqlConnection(connectionString))
                {
                    dbName = tempConn.Database;
                }

                string query = $@"
                    BACKUP DATABASE [{dbName}]
                    TO DISK = @path
                    WITH FORMAT,
                         MEDIANAME = 'MiReclamoBackup',
                         NAME = 'Backup completo de MiReclamo';";

                using (SqlConnection connection = new SqlConnection(connectionString))
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@path", fullPath);
                    connection.Open();
                    command.ExecuteNonQuery();
                }

                MessageBox.Show($"✅ Backup creado correctamente en:\n{fullPath}",
                                "Backup completado", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Error al crear el backup:\n{ex.Message}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
