using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace proyecto_tdp_2.MVVM.View
{
    public partial class ClaimView : UserControl
    {
        private ObservableCollection<BitmapImage> _imagenes;
        public string NombreUsuario { get; set; } = string.Empty;
        public string RolUsuario { get; set; } = string.Empty;

        private readonly Dictionary<string, List<string>> _subtiposPorTipo;

        public ClaimView()
        {
            InitializeComponent();
            _imagenes = new ObservableCollection<BitmapImage>();
            PreviewPanel.ItemsSource = _imagenes;

            _subtiposPorTipo = new Dictionary<string, List<string>>
            {
                { "Agua", new List<string> { "Pérdida de agua", "Corte de suministro", "Baja presión", "Medidor roto" } },
                { "Luz", new List<string> { "Luminaria descompuesta", "Poste caído", "Corte de energía", "Cable suelto" } },
                { "Calles", new List<string> { "Bache", "Inundación", "Semáforo roto", "Contenedor volcado" } }
            };
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
                    img.DecodePixelWidth = 200;
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
        private void mapView_Loaded(object sender, RoutedEventArgs e)
        {
            GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerAndCache;
            // Seleccion de proveedor
            mapView.MapProvider = GMap.NET.MapProviders.OpenStreetMapProvider.Instance;
            mapView.MinZoom = 2;
            mapView.MaxZoom = 17;
            // Zoom Global
            mapView.Zoom = 2;
            // Permite hacer zoom usando la rueda del mouse
            mapView.MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionAndCenter;
            // Permite arrastrar el mapa
            mapView.CanDragMap = true;
            // Arrastra con BIM
            mapView.DragButton = MouseButton.Left;
        }

        private void cbTipoServicio_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbTipoServicio.SelectedItem is ComboBoxItem selectedItem)
            {
                string tipo = selectedItem.Content.ToString();

                if (_subtiposPorTipo.ContainsKey(tipo))
                {
                    cbSubtipo.ItemsSource = _subtiposPorTipo[tipo];
                    cbSubtipo.IsEnabled = true;
                    cbSubtipo.SelectedIndex = -1;
                }
                else
                {
                    cbSubtipo.ItemsSource = null;
                    cbSubtipo.IsEnabled = false;
                }

            }
        }
    }
}
