using System.Windows;
using System.Windows.Input;

namespace proyecto_tdp_2.MVVM.View
{

    public partial class ProfileView : Window
    {
        public ProfileView()
        {
            InitializeComponent();

            txtNombre.Text = Session.Nombre;
            txtRol.Text = Session.Rol;
            txtCorreo.Text = Session.Correo;
            txtTelefono.Text = Session.Telefono;
            txtServicio.Text = Session.Servicio;
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void TopBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                try
                {
                    this.DragMove();
                }
                catch (InvalidOperationException)
                {
                }
            }
        }

        private void TopBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                ToggleMaximizeRestore();
            }
        }

        private void ToggleMaximizeRestore()
        {
            this.WindowState = this.WindowState == WindowState.Normal
                               ? WindowState.Maximized
                               : WindowState.Normal;
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            MisReclamosView reclamosView = new MisReclamosView();
            reclamosView.Show();
            this.Close();
        }

        private void RadioButton_Checked_1(object sender, RoutedEventArgs e)
        {
            DashboardView home = new DashboardView();
            home.Show();
            this.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            EditProfileView editar = new EditProfileView();
            editar.Show();
            this.Close();
        }

        private void RadioButton_Checked_2(object sender, RoutedEventArgs e)
        {
            RegisterView register = new RegisterView();
            register.Show();
            this.Close();
        }
    }
}
