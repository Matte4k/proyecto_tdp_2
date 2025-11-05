using System.Configuration;
using System.Data.SqlClient;
using System.Windows;

namespace proyecto_tdp_2.MVVM.View
{
    public partial class CambiarEstadoReclamoView : Window
    {
        private int idReclamo;
        private int idOperador;
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["MiReclamoDB"].ConnectionString;

        public CambiarEstadoReclamoView(int idReclamo, int idOperador)
        {
            InitializeComponent();
            this.idReclamo = idReclamo;
            this.idOperador = idOperador;
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string nuevoEstado = ((System.Windows.Controls.ComboBoxItem)CmbEstado.SelectedItem).Content.ToString();
                string comentario = TxtComentario.Text.Trim();

                if (string.IsNullOrWhiteSpace(nuevoEstado))
                {
                    MessageBox.Show("Debe seleccionar un estado.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(comentario))
                {
                    MessageBox.Show("Debe ingresar un comentario.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    int idEstado = 0;
                    using (SqlCommand cmdEstado = new SqlCommand("SELECT id_estado FROM Estados WHERE nombre = @nombre", conn))
                    {
                        cmdEstado.Parameters.AddWithValue("@nombre", nuevoEstado);
                        object result = cmdEstado.ExecuteScalar();
                        if (result != null)
                            idEstado = Convert.ToInt32(result);
                        else
                            throw new Exception("No se encontró el estado seleccionado.");
                    }

                    string updateQuery = @"
                        UPDATE Reclamos
                        SET id_estado = @id_estado,
                            fecha_cierre = CASE WHEN @nuevo_estado = 'Resuelto' THEN GETDATE() ELSE fecha_cierre END
                        WHERE id_reclamo = @id_reclamo";

                    using (SqlCommand cmdUpdate = new SqlCommand(updateQuery, conn))
                    {
                        cmdUpdate.Parameters.AddWithValue("@id_estado", idEstado);
                        cmdUpdate.Parameters.AddWithValue("@nuevo_estado", nuevoEstado);
                        cmdUpdate.Parameters.AddWithValue("@id_reclamo", idReclamo);
                        cmdUpdate.ExecuteNonQuery();
                    }

                    string insertQuery = @"
                        INSERT INTO HistorialEstado (id_reclamo, id_estado, id_operador, comentario, fecha_cambio)
                        VALUES (@id_reclamo, @id_estado, @id_operador, @comentario, GETDATE())";

                    using (SqlCommand cmdInsert = new SqlCommand(insertQuery, conn))
                    {
                        cmdInsert.Parameters.AddWithValue("@id_reclamo", idReclamo);
                        cmdInsert.Parameters.AddWithValue("@id_estado", idEstado);
                        cmdInsert.Parameters.AddWithValue("@id_operador", idOperador);
                        cmdInsert.Parameters.AddWithValue("@comentario", comentario);
                        cmdInsert.ExecuteNonQuery();
                    }

                    conn.Close();
                }

                MessageBox.Show("Estado actualizado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cambiar el estado del reclamo:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void TopBar_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
                DragMove();
        }
    }
}
