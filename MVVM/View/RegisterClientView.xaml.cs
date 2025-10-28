using System.Configuration;
using System.Data.SqlClient;
using System.Windows;

namespace proyecto_tdp_2.MVVM.View
{
    public partial class RegisterClientView : Window
    {
        string connectionString = ConfigurationManager.ConnectionStrings["MiReclamoDB"].ConnectionString;

        public RegisterClientView()
        {
            InitializeComponent();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string nombre = tbFullName.Text.Trim();
                string apellido = tbSurName.Text.Trim();
                string email = tbEmail.Text.Trim();
                string telefono = tbPhone.Text.Trim();
                string dni = tbDNI.Text.Trim();
                string cuit = tbCUIT.Text.Trim();


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

                if (string.IsNullOrWhiteSpace(email))
                {
                    MessageBox.Show("El correo electrónico es obligatorio.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    tbEmail.Focus();
                    return;
                }


                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = @"INSERT INTO Clientes
                                (nombre, apellido, email, telefono, dni, cuit)
                                VALUES 
                                (@nombre, @apellido, @correo, @telefono, @dni, @cuit)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@nombre", nombre);
                        command.Parameters.AddWithValue("@apellido", apellido);
                        command.Parameters.AddWithValue("@correo", email);
                        command.Parameters.AddWithValue("@telefono", telefono);
                        command.Parameters.AddWithValue("@dni", dni);
                        command.Parameters.AddWithValue("@cuit", cuit);

                        int rows = command.ExecuteNonQuery();

                        if (rows > 0)
                        {
                            MessageBox.Show("Cliente registrado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("No se pudo registrar el cliente.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
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

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            ProfileView perfil = new ProfileView();
            this.Close();
        }

        private void TopBar_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
                this.DragMove();
        }


        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            tbFullName.Clear();
            tbSurName.Clear();
            tbEmail.Clear();
            tbPhone.Clear();
            tbDNI.Clear();
            tbCUIT.Clear();
        }
    }
}



