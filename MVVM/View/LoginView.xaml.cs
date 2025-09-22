using System.Configuration;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace proyecto_tdp_2.MVVM.View
{
    /// <summary>
    /// Interaction logic for LoginView.xaml
    /// </summary>
    public partial class LoginView : Window
    {
        public LoginView()
        {
            InitializeComponent();
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void TopBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            string email = tbMail.Text.Trim();
            string password = tbPass.Password;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Debes ingresar correo y contraseña.", "Validación",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["MiReclamoDB"].ConnectionString;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "SELECT COUNT(*) FROM Usuario WHERE email = @correo AND password = @pass";

                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@correo", email);
                        cmd.Parameters.AddWithValue("@pass", password); // ⚠️ OJO: en producción se debe usar hash

                        int count = (int)cmd.ExecuteScalar();

                        if (count > 0)
                        {
                            DashboardView home = new DashboardView();
                            home.Show();
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("Correo o contraseña incorrectos.",
                                            "Error",
                                            MessageBoxButton.OK,
                                            MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Error en la base de datos: " + ex.Message, "SQL Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error inesperado: " + ex.Message, "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void tbMail_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

            if (textBox != null && !Regex.IsMatch(textBox.Text, pattern))
            {
                MessageBox.Show("Correo electrónico inválido.",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                textBox.BorderBrush = System.Windows.Media.Brushes.Red;
                textBox.ToolTip = "Formato de correo inválido";
            }
            else
            {
                textBox.ClearValue(Border.BorderBrushProperty);
                textBox.ToolTip = null;
            }

        }
    }
}
