using System.Configuration;
using System.Data.SqlClient;
using System.Windows;

namespace proyecto_tdp_2.MVVM.View
{
    public partial class RegisterClientView : Window
    {
        string connectionString = ConfigurationManager.ConnectionStrings["MiReclamoDB"].ConnectionString;
        private int? _idCliente = null;

        public RegisterClientView()
        {
            InitializeComponent();
        }

        public RegisterClientView(int idCliente, string nombre, string apellido, string telefono, string cuit, string dni, string email)
        {
            InitializeComponent();
            _idCliente = idCliente;
            Title = "Editar Cliente";
            tbFullName.Text = nombre;
            tbSurName.Text = apellido;
            tbPhone.Text = telefono;
            tbCUIT.Text = cuit;
            tbDNI.Text = dni;
            tbEmail.Text = email;

        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (tbEmail.Text == "")
            {
                MessageBox.Show("Debe escribir un correo electronico.");
                return;
            }

            if (tbFullName.Text == "")
            {
                MessageBox.Show("Debe escribir un nombre.");
                return;
            }
            if (tbDNI.Text == "")
            {
                MessageBox.Show("Debe escribir un numero de documento.");
                return;
            }
            if (tbSurName.Text == "")
            {
                MessageBox.Show("Debe escribir un apellido.");
                return;
            }
            if (tbPhone.Text == "")
            {
                MessageBox.Show("Debe escribir un numero de telefono.");
                return;
            }
            if (tbCUIT.Text == "")
            {
                MessageBox.Show("Debe escribir un numero de telefono.");
                return;
            }
            try
            {
                
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    if (_idCliente == null)
                    {
                        string insertQuery = @"INSERT INTO Clientes (nombre, apellido, telefono, cuit, dni, email)
                                               VALUES (@nombre, @apellido, @telefono, @cuit, @dni, @correo)";
                        SqlCommand cmd = new SqlCommand(insertQuery, conn);
                        cmd.Parameters.AddWithValue("@nombre", tbFullName.Text);
                        cmd.Parameters.AddWithValue("@apellido", tbSurName.Text);
                        cmd.Parameters.AddWithValue("@telefono", tbPhone.Text);
                        cmd.Parameters.AddWithValue("@cuit", tbCUIT.Text);
                        cmd.Parameters.AddWithValue("@dni", tbDNI.Text);
                        cmd.Parameters.AddWithValue("@correo", tbEmail.Text);
                        cmd.ExecuteNonQuery();

                        MessageBox.Show("Cliente creado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        string updateQuery = @"UPDATE Clientes
                                               SET nombre=@nombre, apellido=@apellido, telefono=@telefono, cuit=@cuit, dni=@dni, email=@correo
                                               WHERE id_cliente=@id";
                        SqlCommand cmd = new SqlCommand(updateQuery, conn);
                        cmd.Parameters.AddWithValue("@id", _idCliente);
                        cmd.Parameters.AddWithValue("@nombre", tbFullName.Text);
                        cmd.Parameters.AddWithValue("@apellido", tbSurName.Text);
                        cmd.Parameters.AddWithValue("@telefono", tbPhone.Text);
                        cmd.Parameters.AddWithValue("@cuit", tbCUIT.Text);
                        cmd.Parameters.AddWithValue("@dni", tbDNI.Text);
                        cmd.Parameters.AddWithValue("@correo", tbEmail.Text);
                        cmd.ExecuteNonQuery();

                        MessageBox.Show("Cliente actualizado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    }

                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar cliente: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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



