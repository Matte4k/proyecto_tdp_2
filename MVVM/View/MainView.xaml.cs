using proyecto_tdp_2.Helpers;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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
            if (Session.Rol != "SuperAdmin") rbAgregarOperador.Visibility = Visibility.Collapsed;
            if (Session.Rol != "SuperAdmin" && Session.Rol != "Supervisor") rbAgregarTipoReclamo.Visibility = Visibility.Collapsed;
            if (Session.Rol != "SuperAdmin" && Session.Rol != "Supervisor") rbBackup.Visibility = Visibility.Collapsed;
            if (Session.Rol != "SuperAdmin" && Session.Rol != "Supervisor") rbReportes.Visibility = Visibility.Collapsed;
            if (Session.Rol == "Operador") rbUsuarios.Visibility = Visibility.Collapsed;
            if (Session.Rol != "Operador") rbAgregarReclamo.Visibility = Visibility.Collapsed;
            CargarAvatar();
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

        private void CargarAvatar()
        {
            try
            {
                if (!string.IsNullOrEmpty(Session.ImagenRuta))
                {
                    string fullPath = Session.ImagenRuta;

                    if (!Path.IsPathRooted(fullPath))
                    {
                        string binPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fullPath);
                        string projectPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\", fullPath));

                        if (File.Exists(binPath))
                            fullPath = binPath;
                        else if (File.Exists(projectPath))
                            fullPath = projectPath;
                    }

                    if (File.Exists(fullPath))
                    {
                        AvatarEllipse.Fill = new ImageBrush(new BitmapImage(new Uri(fullPath, UriKind.Absolute)))
                        {
                            Stretch = Stretch.UniformToFill
                        };
                        return;
                    }
                }

                AvatarEllipse.Fill = new ImageBrush(
                    new BitmapImage(new Uri("pack://application:,,,/Images/man.png"))
                )
                {
                    Stretch = Stretch.UniformToFill
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar el avatar: " + ex.Message);
            }
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginView();
            loginWindow.Show();
            this.Close();
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
                        MainContent.Content = new ClientesView();
                        break;

                    case "Dashboard":
                        MainContent.Content = new DashboardView(Session.UserId, Session.Rol);
                        break;

                    case "Reportes":
                        MainContent.Content = new ReportesView();
                        break;

                    case "Backup":
                        MainContent.Content = new BackupView();
                        break;

                    case "Usuarios":
                        MainContent.Content = new UserManagementView(Session.UserId, Session.Rol);
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