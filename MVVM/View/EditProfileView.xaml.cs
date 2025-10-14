using System.Configuration;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;


namespace proyecto_tdp_2.MVVM.View
{
    public partial class EditProfileView : Window
    {
        public EditProfileView()
        {
            InitializeComponent();
            CargarDatosUsuario();
        }

        private void CargarDatosUsuario()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["MiReclamoDB"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    string query = "SELECT nombre, apellido, telefono, password FROM Usuario WHERE id_usuario = @idUsuario";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@idUsuario", Session.UserId);

                        SqlDataReader reader = cmd.ExecuteReader();

                        if (reader.Read())
                        {
                            TxtNombre.Text = reader["nombre"].ToString();
                            TxtApellido.Text = reader["apellido"].ToString();
                            TxtTelefono.Text = reader["telefono"].ToString();
                            TxtPassword.Password = reader["password"].ToString();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al cargar el perfil: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            ProfileView perfil = new ProfileView();
            this.Close();
        }



        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            string nombre = TxtNombre.Text.Trim();
            string apellido = TxtApellido.Text.Trim();
            string telefonoTexto = TxtTelefono.Text.Trim();
            string password = TxtPassword.Password.Trim();

            if (string.IsNullOrWhiteSpace(nombre) ||
                string.IsNullOrWhiteSpace(apellido) ||
                string.IsNullOrWhiteSpace(telefonoTexto) ||
                string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Todos los campos son obligatorios.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!long.TryParse(telefonoTexto, out long telefono))
            {
                MessageBox.Show("El teléfono debe ser numérico.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string connectionString = ConfigurationManager.ConnectionStrings["MiReclamoDB"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    string query = @"
                        UPDATE Usuario 
                        SET nombre = @nombre, 
                            apellido = @apellido,
                            telefono = @telefono, 
                            password = @password
                        WHERE id_usuario = @idUsuario";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@nombre", nombre);
                        cmd.Parameters.AddWithValue("@apellido", apellido);
                        cmd.Parameters.AddWithValue("@telefono", telefono);
                        cmd.Parameters.AddWithValue("@password", password);
                        cmd.Parameters.AddWithValue("@idUsuario", Session.UserId);

                        int filas = cmd.ExecuteNonQuery();

                        if (filas > 0)
                        {
                            MessageBox.Show("Perfil actualizado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                            ProfileView perfil = new ProfileView();
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("No se encontró el usuario a actualizar.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al actualizar el perfil: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }





        private void TopBar_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
                this.DragMove();
        }

        private void TxtNombre_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            // Regex: solo letras y espacios (NO números)
            Regex regex = new Regex("[^a-zA-ZáéíóúÁÉÍÓÚüÜñÑ ]+");

            // Si el texto NO cumple, se bloquea
            e.Handled = regex.IsMatch(e.Text);
        }

        private void TxtTelefono_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;

            // Regex: solo dígitos
            Regex regex = new Regex("[^0-9]+");


            if (regex.IsMatch(e.Text))
            {
                e.Handled = true;
                return;
            }


            if (textBox != null && textBox.Text.Length >= 15)
            {
                e.Handled = true;
            }
        }

        private void TxtApellido_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^a-zA-ZáéíóúÁÉÍÓÚüÜñÑ ]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
