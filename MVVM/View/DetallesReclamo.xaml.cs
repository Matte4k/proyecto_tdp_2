using System.Windows;

namespace proyecto_tdp_2.MVVM.View
{
    public partial class DetalleReclamo : Window
    {

        public string NombreUsuario { get; set; } = string.Empty;
        public string RolUsuario { get; set; } = string.Empty;
        public DetalleReclamo()
        {
            InitializeComponent();
            if (Session.Rol != "SuperAdmin")
            {
                rbAgregarOperador.Visibility = Visibility.Collapsed;
            }
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void TopBar_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
                this.DragMove();
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            MisReclamosView reclamosView = new MisReclamosView();
            this.Close();
        }

        private void RadioButton_Checked_1(object sender, RoutedEventArgs e)
        {
            ProfileView perfil = new ProfileView();
            this.Close();
        }

        private void RadioButton_Checked_2(object sender, RoutedEventArgs e)
        {

        }

        private void BtnVolver_Click(object sender, RoutedEventArgs e)
        {
            MisReclamosView misReclamos = new MisReclamosView();
            this.Close();
        }

        private void RadioButton_Checked_3(object sender, RoutedEventArgs e)
        {
            DashboardView home = new DashboardView();
            this.Close();
        }
    }
}
