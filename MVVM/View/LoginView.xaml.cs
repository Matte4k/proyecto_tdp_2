using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
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

            this.Close();
        }

        private void tbMail_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

            if (textBox != null && !Regex.IsMatch(textBox.Text, pattern))
            {
                MessageBox.Show("Correo electrónico inválido.",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                textBox.BorderBrush = System.Windows.Media.Brushes.Red;
                textBox.ToolTip = "Formato de correo inválido";
            } 
            else
            {
                textBox.ClearValue(Border.BorderBrushProperty);
                textBox.ToolTip = null;
            }

        }
    }
}
