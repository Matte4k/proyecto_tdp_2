using System.Windows;
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
            DashboardView home = new DashboardView();
            home.Show();

            this.Hide();
        }
    }
}
