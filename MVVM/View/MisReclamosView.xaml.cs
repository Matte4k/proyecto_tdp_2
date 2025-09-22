using System.Windows;
using System.Windows.Input;

namespace proyecto_tdp_2.MVVM.View
{
    /// <summary>
    /// Lógica de interacción para MisReclamosView.xaml
    /// </summary>
    public partial class MisReclamosView : Window
    {
        public MisReclamosView()
        {
            InitializeComponent();
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
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            DashboardView home = new DashboardView();
            home.Show();
            this.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DetalleReclamo detalle = new DetalleReclamo();
            detalle.Show();
            this.Close();
        }

        private void RadioButton_Checked_1(object sender, RoutedEventArgs e)
        {
            ProfileView perfil = new ProfileView();
            perfil.Show();
            this.Close();
        }

        private void RadioButton_Checked_2(object sender, RoutedEventArgs e)
        {
            RegisterView nuevoOperador = new RegisterView();
            nuevoOperador.Show();
            this.Close();
        }
    }
}
