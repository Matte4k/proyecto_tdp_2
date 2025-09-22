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


            ProfileView perfil = new ProfileView();
            perfil.Show();
            this.Close();
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
    }
}
