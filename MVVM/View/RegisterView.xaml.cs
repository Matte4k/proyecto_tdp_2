using Microsoft.Win32;
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
            // Recolectar valores
            string fullName = tbFullName.Text != null ? tbFullName.Text.Trim() : string.Empty;
            string company = tbCompany.Text != null ? tbCompany.Text.Trim() : string.Empty;
            string dni = tbDNI.Text != null ? tbDNI.Text.Trim() : string.Empty;
            string cuit = tbCUIT.Text != null ? tbCUIT.Text.Trim() : string.Empty;
            string? province = (cbProvince.SelectedItem as ComboBoxItem)?.Content?.ToString();
            string phone = tbPhone.Text != null ? tbPhone.Text.Trim() : string.Empty;
            string? email = tbEmail.Text?.Trim();
            string role = (cbRole.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Operador";
            string? zone = (cbZone.SelectedItem as ComboBoxItem)?.Content?.ToString();
            bool isActive = chkActive.IsChecked == true;
            bool sendWelcome = chkSendWelcome.IsChecked == true;
            string password = pbPassword.Password;
            string confirm = pbConfirm.Password;

            // Validaciones básicas
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

            // TODO: añadir validación adicional (formato email, longitudes, CUIT/DNI, etc.)

            // Construir DTO / modelo para enviar al servicio (ejemplo)
            var nuevoOperador = new
            {
                Nombre = fullName,
                Empresa = company,
                DNI = dni,
                CUIT = cuit,
                Provincia = province,
                Telefono = phone,
                Email = email,
                Rol = role,
                Zona = zone,
                Activo = isActive,
                AvatarPath = _avatarFilePath,
                // nunca almacenar password en texto plano: hashear en servidor o usar protocolo seguro
            };

            // Aquí llamás a tu servicio / ViewModel para persistir. Ejemplo (placeholder):
            bool creado = CrearUsuarioEnServicio(nuevoOperador, password);

            if (creado)
            {
                if (sendWelcome)
                {
                    // Llamá a tu servicio de correo para enviar email de bienvenida.
                }

                MessageBox.Show("Usuario creado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("Ocurrió un error al crear el usuario.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Placeholder: implementá la llamada real a tu API/servicio o ViewModel
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
    }
}
