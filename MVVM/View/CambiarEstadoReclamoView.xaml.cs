using System.Windows;
using System.Windows.Controls;

namespace proyecto_tdp_2.MVVM.View
{
    public partial class CambiarEstadoReclamoView : Window
    {
        public string? NuevoEstado { get; private set; }
        public string? Comentario { get; private set; }

        public CambiarEstadoReclamoView()
        {
            InitializeComponent();
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (CmbEstado.SelectedItem is ComboBoxItem item)
                NuevoEstado = item.Content.ToString();

            Comentario = TxtComentario.Text.Trim();
            DialogResult = true;
            Close();
        }

        private void TopBar_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
                DragMove();
        }
    }
}
