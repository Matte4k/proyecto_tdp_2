using System.Windows;

namespace proyecto_tdp_2.MVVM.View
{
    public partial class EditProfileView : Window
    {
        public EditProfileView()
        {
            InitializeComponent();
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            ProfileView perfil = new ProfileView();
            perfil.Show();
            this.Close();
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            string nombre = TxtNombre.Text;
            string telefono = TxtTelefono.Text;
            string password = TxtPassword.Password;

            if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(telefono) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Todos los campos son obligatorios.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MessageBox.Show("Perfil actualizado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

            this.Close();
        }

        private void TopBar_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
                this.DragMove();
        }


    }
}
