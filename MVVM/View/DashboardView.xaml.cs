using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace proyecto_tdp_2.MVVM.View
{
    /// <summary>
    /// Interaction logic for DashboardView.xaml
    /// </summary>
    public partial class DashboardView : Window
    {
        private ObservableCollection<BitmapImage> _imagenes;
        public DashboardView()
        {
            InitializeComponent();
            _imagenes = new ObservableCollection<BitmapImage>();
            PreviewPanel.ItemsSource = _imagenes;
        }

        private void BtnCargarFotos_Click(object sender, RoutedEventArgs e)
        {

            if (_imagenes.Count >= 4)
            {
                MessageBox.Show("Solo se pueden cargar hasta 4 imágenes.");
                return;
            }

            OpenFileDialog ofd = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "Imágenes|*.jpg;*.jpeg;*.png;*.bmp;*.gif"
            };

            if (ofd.ShowDialog() == true)
            {
                foreach (string file in ofd.FileNames)
                {
                    if (_imagenes.Count >= 4) break;

                    BitmapImage img = new BitmapImage();
                    img.BeginInit();
                    img.UriSource = new System.Uri(file);
                    img.DecodePixelWidth = 200; // Optimiza memoria
                    img.EndInit();

                    _imagenes.Add(img);
                }
            }
        }

        private void BtnEliminarFoto_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is BitmapImage img)
            {
                _imagenes.Remove(img);
            }
        }
    }
}
