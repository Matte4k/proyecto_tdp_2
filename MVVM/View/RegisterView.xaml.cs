using Microsoft.Win32;
using System.Configuration;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace proyecto_tdp_2.MVVM.View
{
    public partial class RegisterView : Window
    {
        // Solución: Inicializar el campo _avatarFilePath para evitar el error CS8618.
        private string _avatarFilePath = string.Empty;
        string connectionString = ConfigurationManager.ConnectionStrings["MiReclamoDB"].ConnectionString;

        public RegisterView()
        {
            InitializeComponent();
        }

        // Permitir arrastrar ventana desde la barra superior
        private void TopBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                try { this.DragMove(); }
                catch (InvalidOperationException) { }
            }
        }


        private void BtnMinimize_Click(object sender, RoutedEventArgs e) => this.WindowState = WindowState.Minimized;
        private void BtnExit_Click(object sender, RoutedEventArgs e) => this.Close();
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DashboardView dashboard = new DashboardView();
            dashboard.Show();
            this.Close();
        }

        // Subir avatar
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

        // Guardar / validar
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Recolectar valores
                string fullName = tbFullName.Text.Trim();
                string company = tbCompany.Text.Trim();
                string dni = tbDNI.Text.Trim();
                string cuit = tbCUIT.Text.Trim();
                string province = (cbProvince.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? string.Empty;
                string phone = tbPhone.Text.Trim();
                string email = tbEmail.Text.Trim();
                string role = (cbRole.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Operador";
                string zone = (cbZone.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? string.Empty;
                bool isActive = chkActive.IsChecked == true;
                bool sendWelcome = chkSendWelcome.IsChecked == true;
                string password = pbPassword.Password;
                string confirm = pbConfirm.Password;

                // Validaciones
                if (string.IsNullOrWhiteSpace(fullName))
                {
                    MessageBox.Show("El nombre completo es obligatorio.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    tbFullName.Focus();
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

                // Abrir conexión desde App.config
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = @"INSERT INTO Usuario (nombre, email, password, telefono, rol, servicio) 
                             VALUES (@nombre, @correo, @pass, @telefono, @rol, @servicio)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@nombre", fullName);
                        command.Parameters.AddWithValue("@correo", email);
                        command.Parameters.AddWithValue("@pass", password); // ⚠️ OJO: en producción se debe hashear
                        command.Parameters.AddWithValue("@telefono", phone);

                        // Mapear rol a ID real (asegurate que exista en la tabla Roles)
                        if (role == "Operador")
                            command.Parameters.AddWithValue("@rol", 1);
                        else
                            command.Parameters.AddWithValue("@rol", 2);

                        // Mapear servicio a ID real (asegurate que exista en la tabla Servicios)
                        switch (company)
                        {
                            case "IOSCOR":
                                command.Parameters.AddWithValue("@servicio", 1);
                                break;
                            case "AguasCorrientes":
                                command.Parameters.AddWithValue("@servicio", 2);
                                break;
                            default:
                                command.Parameters.AddWithValue("@servicio", 3);
                                break;
                        }

                        int rows = command.ExecuteNonQuery();
                        if (rows > 0)
                        {
                            if (sendWelcome)
                            {
                                // Acá podrías llamar a un servicio de correo para enviar la bienvenida
                            }

                            MessageBox.Show("Usuario creado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("No se pudo crear el usuario.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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


        private bool CrearUsuarioEnServicio(object usuarioDto, string password)
        {
            // Ejemplo: aquí deberías:
            //  - Validar más (email único)
            //  - Enviar password de forma segura (hash en servidor o uso de Identity)
            //  - Guardar avatar (subir a storage o guardar ruta)
            // Retornar true si se creó correctamente
            try
            {
                // Simulación rápida
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            DashboardView home = new DashboardView();
            home.Show();
            this.Close();
        }

        private void RadioButton_Checked_1(object sender, RoutedEventArgs e)
        {
            MisReclamosView reclamos = new MisReclamosView();
            reclamos.Show();
            this.Close();
        }

        private void RadioButton_Checked_2(object sender, RoutedEventArgs e)
        {
            ProfileView perfil = new ProfileView();
            perfil.Show();
            this.Close();
        }

        private void tbDNI_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;

            // Regex: solo dígitos
            Regex regex = new Regex("[^0-9]+");

            // Bloquea si no es número
            if (regex.IsMatch(e.Text))
            {
                e.Handled = true;
                return;
            }

            // Bloquea si ya hay 8 dígitos
            if (textBox != null && textBox.Text.Length >= 8)
            {
                e.Handled = true;
            }
        }

        private void tbCUIT_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;

            // Regex: solo dígitos
            Regex regex = new Regex("[^0-9]+");

            // Bloquea si no es número
            if (regex.IsMatch(e.Text))
            {
                e.Handled = true;
                return;
            }

            // Bloquea si ya hay 11 dígitos
            if (textBox != null && textBox.Text.Length >= 11)
            {
                e.Handled = true;
            }
        }

        private void tbPhone_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;

            // Regex: solo dígitos
            Regex regex = new Regex("[^0-9]+");

            // Bloquea si no es número
            if (regex.IsMatch(e.Text))
            {
                e.Handled = true;
                return;
            }
        }

        private void tbFullName_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Regex: solo letras y espacios (NO números)
            Regex regex = new Regex("[^a-zA-ZáéíóúÁÉÍÓÚüÜñÑ ]+");

            // Si el texto NO cumple, se bloquea
            e.Handled = regex.IsMatch(e.Text);
        }

        private void tbCompany_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Regex: solo letras y espacios (NO números)
            Regex regex = new Regex("[^a-zA-ZáéíóúÁÉÍÓÚüÜñÑ ]+");

            // Si el texto NO cumple, se bloquea
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
