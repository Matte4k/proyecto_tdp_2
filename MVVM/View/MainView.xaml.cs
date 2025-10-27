using proyecto_tdp_2.Helpers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
namespace proyecto_tdp_2.MVVM.View
{
    public partial class MainView : Window
    {
        public MainView()
        {
            InitializeComponent();

            Navigator.OnNavigate += view =>
            {
                MainContent.Content = view;
            };

            this.Loaded += (s, e) =>
            {
                MainContent.Content = new ClaimView();
            };

            txtNombre.Text = Session.Nombre;
            txtRol.Text = Session.Rol;
            if (Session.Rol != "SuperAdmin" && Session.Rol != "Supervisor") rbAgregarOperador.Visibility = Visibility.Collapsed;
            if (Session.Rol != "SuperAdmin" && Session.Rol != "Supervisor") rbAgregarTipoReclamo.Visibility = Visibility.Collapsed;
            if (Session.Rol != "SuperAdmin" && Session.Rol != "Supervisor") rbBackup.Visibility = Visibility.Collapsed;
            if (Session.Rol != "SuperAdmin" && Session.Rol != "Supervisor") rbReportes.Visibility = Visibility.Collapsed;

        }
        private void Nav_CrearReclamo(object sender, RoutedEventArgs e) { MainContent.Content = new ClaimView(); }
        private void Nav_MisReclamos(object sender, RoutedEventArgs e) { MainContent.Content = new MisReclamosView(); }
        private void Nav_Perfil(object sender, RoutedEventArgs e) { MainContent.Content = new ProfileView(); }
        private void Nav_AgregarOperador(object sender, RoutedEventArgs e) { MainContent.Content = new RegisterView(); }
        private void BtnExit_Click(object sender, RoutedEventArgs e) { this.Close(); }
        private void BtnMinimize_Click(object sender, RoutedEventArgs e) { this.WindowState = WindowState.Minimized; }


        private void TopBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                try { this.DragMove(); }
                catch (InvalidOperationException)
                {
                }
            }
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (!this.IsLoaded || MainContent == null)
                return;

            if (sender is RadioButton rb)
            {
                string viewName = rb.Content?.ToString() ?? "";

                switch (viewName)
                {
                    case "Crear Reclamo":
                        MainContent.Content = new ClaimView();
                        break;

                    case "Mis Reclamos":
                        MainContent.Content = new MisReclamosView();
                        break;

                    case "Perfil":
                        MainContent.Content = new ProfileView();
                        break;

                    case "Agregar Operador":
                        MainContent.Content = new RegisterView();
                        break;

                    case "Agregar Tipo de Reclamo":
                        MainContent.Content = new NewType();
                        break;

                    case "Clientes":
                        MainContent.Content = new RegisterClientView();
                        break;

                    case "Cerrar Sesión":
                        MainContent.Content = null;
                        LoginView loginView = new LoginView();
                        loginView.Show();
                        this.Close();
                        break;

                    default:
                        MainContent.Content = null;
                        break;
                }
            }
        }


    }
}