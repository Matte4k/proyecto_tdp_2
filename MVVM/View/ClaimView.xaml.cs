using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace proyecto_tdp_2.MVVM.View
{
    public partial class ClaimView : UserControl
    {
        private ObservableCollection<BitmapImage> _imagenes;
        public string NombreUsuario { get; set; } = string.Empty;
        public string RolUsuario { get; set; } = string.Empty;

        private GMapMarker? currentMarker;

        public ClaimView()
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
                    img.DecodePixelWidth = 200;
                    img.EndInit();

                    _imagenes.Add(img);
                }
            }
        }

        private void CargarTiposReclamos()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection("your_connection_string"))
                {
                    conn.Open();
                    string query = "SELECT nombre FROM TipoReclamo WHERE subtipo_reclamo";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<string> tipos = new List<string>();
                    while (reader.Read())
                    {
                        tipos.Add(reader.GetString(0));
                    }
                    cbTipoServicio.ItemsSource = tipos;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar tipos de reclamos: {ex.Message}");
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
            GMaps.Instance.Mode = AccessMode.ServerAndCache;
            mapView.MapProvider = GoogleMapProvider.Instance;
            mapView.MinZoom = 2;
            mapView.MaxZoom = 18;
            mapView.Zoom = 5;
            mapView.Position = new PointLatLng(-34.6037, -58.3816);

            mapView.MouseWheelZoomType = MouseWheelZoomType.MousePositionAndCenter;
            mapView.CanDragMap = true;
            mapView.DragButton = MouseButton.Left;
        }

        private void mapView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var point = e.GetPosition(mapView);
            var latLng = mapView.FromLocalToLatLng((int)point.X, (int)point.Y);
            AgregarMarcador(latLng.Lat, latLng.Lng);
        }

        private void AgregarMarcador(double lat, double lng)
        {
            if (currentMarker != null)
                mapView.Markers.Remove(currentMarker);

            var posicion = new PointLatLng(lat, lng);

            currentMarker = new GMapMarker(posicion)
            {
                Shape = new System.Windows.Shapes.Ellipse
                {
                    Width = 12,
                    Height = 12,
                    Stroke = Brushes.Red,
                    StrokeThickness = 2,
                    Fill = Brushes.OrangeRed
                }
            };

            mapView.Markers.Add(currentMarker);
            mapView.Position = posicion;
            mapView.Zoom = 15;

            lblCoordenadas.Text = $"Latitud: {lat:F6}, Longitud: {lng:F6}";
        }

        private async void btnBuscar_Click(object sender, RoutedEventArgs e)
        {
            string direccion = txtDireccion.Text.Trim();
            if (string.IsNullOrEmpty(direccion))
            {
                MessageBox.Show("Ingrese una dirección para buscar.");
                return;
            }

            try
            {
                var (lat, lng) = await ObtenerCoordenadasDesdeDireccion(direccion);

                if (lat != 0 && lng != 0)
                    AgregarMarcador(lat, lng);
                else
                    MessageBox.Show("No se encontró la dirección.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al buscar dirección: {ex.Message}");
            }
        }

        private async Task<(double lat, double lng)> ObtenerCoordenadasDesdeDireccion(string direccion)
        {
            using var client = new HttpClient();
            string url = $"https://api.mapbox.com/geocoding/v5/mapbox.places/{Uri.EscapeDataString(direccion)}.json?access_token=pk.eyJ1Ijoia2FpYmFiZW5qYSIsImEiOiJjbGRsdGQ1bDcwMnAxM3BxdW4zMTlsaHEyIn0.DZbhRsGZ7aLBnd1LdN6M5A";
            var response = await client.GetStringAsync(url);
            var json = JsonSerializer.Deserialize<JsonElement>(response);

            var features = json.GetProperty("features");
            if (features.GetArrayLength() > 0)
            {
                var coords = features[0].GetProperty("center");
                double lng = coords[0].GetDouble();
                double lat = coords[1].GetDouble();
                return (lat, lng);
            }

            return (0, 0);
        }


        private void cbTipoServicio_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbTipoServicio.SelectedItem != null)
            {

            }
        }
    }
}
