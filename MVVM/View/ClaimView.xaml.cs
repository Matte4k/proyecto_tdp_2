using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using Microsoft.Win32;
using proyecto_tdp_2.Helpers;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
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
        string connectionString = ConfigurationManager.ConnectionStrings["MiReclamoDB"].ConnectionString;

        private ObservableCollection<BitmapImage> _imagenes;
        public string NombreUsuario { get; set; } = string.Empty;
        public string RolUsuario { get; set; } = string.Empty;

        private DataTable _clientesDT = new DataTable();

        private DataRow? _clienteSeleccionado;

        private GMapMarker? currentMarker;


        public ClaimView()
        {
            InitializeComponent();
            _imagenes = new ObservableCollection<BitmapImage>();
            PreviewPanel.ItemsSource = _imagenes;
            CargarTiposPadre();
            CargarClientes();
            CargarPrioridades();
            CargarReclamosRecientes();
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

        private void CargarPrioridades()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT id_prioridad, nombre FROM Prioridades";
                SqlCommand cmd = new SqlCommand(query, conn);
                DataTable dt = new DataTable();
                dt.Load(cmd.ExecuteReader());
                cbPriority.ItemsSource = dt.DefaultView;
                cbPriority.DisplayMemberPath = "nombre";
                cbPriority.SelectedValuePath = "id_prioridad";
            }
        }

        private void CargarTiposPadre()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT id_tipo, nombre, subtipo_reclamo FROM TipoReclamo WHERE subtipo_reclamo IS NULL";
                    SqlCommand cmd = new SqlCommand(query, conn);

                    DataTable dt = new DataTable();
                    dt.Load(cmd.ExecuteReader());

                    TypeCombo.ItemsSource = dt.DefaultView;
                    TypeCombo.DisplayMemberPath = "nombre";
                    TypeCombo.SelectedValuePath = "id_tipo";

                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar tipos: {ex.Message}");
            }
        }


        private void CargarSubTipo(int tipoReclamo)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT id_tipo, nombre, subtipo_reclamo FROM TipoReclamo WHERE subtipo_reclamo = @tipoReclamo";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@tipoReclamo", tipoReclamo);

                    DataTable dt = new DataTable();
                    dt.Load(cmd.ExecuteReader());

                    cbSubtipo.ItemsSource = dt.DefaultView;
                    cbSubtipo.DisplayMemberPath = "nombre";
                    cbSubtipo.SelectedValuePath = "id_tipo";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar tipos: {ex.Message}");
            }
        }

        private void CargarClientes()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT id_cliente, dni FROM Clientes";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    _clientesDT.Clear();
                    _clientesDT.Load(cmd.ExecuteReader());

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar clientes: {ex.Message}");
            }
        }

        private void txtBuscarCliente_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (lstClientes == null || _clientesDT == null)
                return;

            string filtro = txtBuscarCliente.Text.Trim();

            if (string.IsNullOrEmpty(filtro))
            {
                lstClientes.Visibility = Visibility.Collapsed;
                return;
            }

            if (_clientesDT.Rows.Count == 0)
                return;

            var resultados = _clientesDT.AsEnumerable()
                .Where(row =>
                    row["dni"] != DBNull.Value &&
                    row["dni"].ToString().StartsWith(filtro, StringComparison.OrdinalIgnoreCase))
                .Take(10);

            if (resultados.Any())
            {
                lstClientes.ItemsSource = resultados
                    .Select(row => new
                    {
                        id_cliente = row["id_cliente"] != DBNull.Value ? row["id_cliente"].ToString() : "",
                        dni = row["dni"] != DBNull.Value ? row["dni"].ToString() : ""
                    })
                    .ToList();

                lstClientes.Visibility = Visibility.Visible;
            }
            else
            {
                lstClientes.Visibility = Visibility.Collapsed;
            }
        }


        private void lstResultadosClientes_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lstClientes.SelectedItems != null)
            {
                dynamic item = lstClientes.SelectedItem;
                txtBuscarCliente.Text = item.dni;
                lstClientes.Visibility = Visibility.Collapsed;

                _clienteSeleccionado = _clientesDT.AsEnumerable()
                    .FirstOrDefault(r => r["id_cliente"].ToString() == item.id_cliente.ToString());
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
            mapView.Position = new PointLatLng(-27.469192, -58.831186);

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

        private void TypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TypeCombo.SelectedValue != null)
            {
                int idTipo = Convert.ToInt32(TypeCombo.SelectedValue);
                CargarSubTipo(idTipo);
            }
        }

        private void BtnEnviarReclamo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_clienteSeleccionado == null)
                {
                    MessageBox.Show("Debe seleccionar un cliente.");
                    return;
                }

                if (TypeCombo.SelectedValue == null)
                {
                    MessageBox.Show("Debe seleccionar un tipo de reclamo.");
                    return;
                }

                if (currentMarker == null)
                {
                    MessageBox.Show("Debe seleccionar una ubicación en el mapa.");
                    return;
                }

                string descripcion = txtDescripcion.Text.Trim();
                string direccion = txtDireccion.Text.Trim();

                int idCliente = Convert.ToInt32(_clienteSeleccionado["id_cliente"]);
                int idTipo = Convert.ToInt32(TypeCombo.SelectedValue);
                int? idSubtipo = cbSubtipo.SelectedValue != null ? Convert.ToInt32(cbSubtipo.SelectedValue) : null;
                int idPrioridad = Convert.ToInt32(cbPriority.SelectedValue);

                double lat = currentMarker.Position.Lat;
                double lng = currentMarker.Position.Lng;

                int idProvincia = Convert.ToInt32(Session.Provincia);
                int idUsuarioActual = Convert.ToInt32(Session.UserId);

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlTransaction transaction = conn.BeginTransaction();

                    try
                    {
                        string queryBuscar = "SELECT id_zona FROM Ubicacion WHERE direccion = @direccion AND id_provincia = @provincia";
                        SqlCommand cmdBuscar = new SqlCommand(queryBuscar, conn, transaction);
                        cmdBuscar.Parameters.AddWithValue("@direccion", direccion);
                        cmdBuscar.Parameters.AddWithValue("@provincia", idProvincia);

                        object result = cmdBuscar.ExecuteScalar();
                        int idUbicacion;

                        if (result != null)
                        {
                            idUbicacion = (int)result;
                        }
                        else
                        {
                            string queryInsertUbicacion = @"
                        INSERT INTO Ubicacion (latitud, longitud, direccion, id_provincia)
                        OUTPUT INSERTED.id_zona
                        VALUES (@lat, @lon, @direccion, @provincia)";
                            SqlCommand cmdInsertUbicacion = new SqlCommand(queryInsertUbicacion, conn, transaction);
                            cmdInsertUbicacion.Parameters.AddWithValue("@lat", lat);
                            cmdInsertUbicacion.Parameters.AddWithValue("@lon", lng);
                            cmdInsertUbicacion.Parameters.AddWithValue("@direccion", direccion);
                            cmdInsertUbicacion.Parameters.AddWithValue("@provincia", idProvincia);

                            idUbicacion = (int)cmdInsertUbicacion.ExecuteScalar();
                        }

                        string queryReclamo = @"
                    INSERT INTO Reclamos (descripcion, fecha_creacion, prioridad, tipo_reclamo, id_zona, cliente_reclamo, id_estado)
                    VALUES (@descripcion, GETDATE(), @id_prioridad, @id_tipo, @id_zona, @id_cliente, @id_estado);
                    SELECT SCOPE_IDENTITY();";
                        SqlCommand cmdReclamo = new SqlCommand(queryReclamo, conn, transaction);
                        cmdReclamo.Parameters.AddWithValue("@descripcion", descripcion);
                        cmdReclamo.Parameters.AddWithValue("@id_tipo", idTipo);
                        cmdReclamo.Parameters.AddWithValue("@id_zona", idUbicacion);
                        cmdReclamo.Parameters.AddWithValue("@id_cliente", idCliente);
                        cmdReclamo.Parameters.AddWithValue("@id_estado", 1);
                        cmdReclamo.Parameters.AddWithValue("@id_prioridad", idPrioridad);

                        int idReclamo = Convert.ToInt32(cmdReclamo.ExecuteScalar());

                        string queryAsignacion = @"
                    INSERT INTO AsignacionReclamo (reclamo_asignado, usuario_asignado)
                    VALUES (@idReclamo, @idUsuario)";
                        SqlCommand cmdAsignacion = new SqlCommand(queryAsignacion, conn, transaction);
                        cmdAsignacion.Parameters.AddWithValue("@idReclamo", idReclamo);
                        cmdAsignacion.Parameters.AddWithValue("@idUsuario", idUsuarioActual);
                        cmdAsignacion.ExecuteNonQuery();

                        string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "claims");
                        if (!Directory.Exists(folderPath))
                            Directory.CreateDirectory(folderPath);

                        foreach (var img in _imagenes)
                        {
                            string fileName = $"reclamo_{idReclamo}_{Guid.NewGuid():N}.jpg";
                            string filePath = Path.Combine(folderPath, fileName);

                            using (FileStream stream = new FileStream(filePath, FileMode.Create))
                            {
                                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                                encoder.Frames.Add(BitmapFrame.Create(img));
                                encoder.Save(stream);
                            }

                            string relativePath = $"Images/claims/{fileName}";

                            string queryImagen = @"INSERT INTO Imagenes (ruta_imagen, id_reclamo)
                                           VALUES (@ruta, @id_reclamo)";
                            SqlCommand cmdImagen = new SqlCommand(queryImagen, conn, transaction);
                            cmdImagen.Parameters.AddWithValue("@ruta", relativePath);
                            cmdImagen.Parameters.AddWithValue("@id_reclamo", idReclamo);
                            cmdImagen.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        MessageBox.Show($"✅ Reclamo #{idReclamo} creado y asignado correctamente a {Session.Nombre} {Session.Apellido}.");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        MessageBox.Show($"Error al guardar reclamo: {ex.Message}");
                    }
                }

                LimpiarFormulario();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error general: {ex.Message}");
            }
        }



        private void LimpiarFormulario()
        {
            TypeCombo.SelectedIndex = -1;
            cbSubtipo.ItemsSource = null;
            txtBuscarCliente.Text = "";
            _clienteSeleccionado = null;
            txtDireccion.Text = "";
            lblCoordenadas.Text = "";
            currentMarker = null;
            mapView.Markers.Clear();
            _imagenes.Clear();
            txtDescripcion.Clear();
        }

        private void BtnCrearNuevoCliente_Click(object sender, RoutedEventArgs e)
        {
            RegisterClientView CrearCliente = new RegisterClientView();
            CrearCliente.Show();
        }

        private void CargarReclamosRecientes()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"
                SELECT TOP 5 
                    r.id_reclamo,
                    r.fecha_creacion,
                    tr.nombre AS tipo,
                    u.direccion,
                    e.nombre AS estado
                FROM Reclamos r
                INNER JOIN TipoReclamo tr ON r.tipo_reclamo = tr.id_tipo
                INNER JOIN Ubicacion u ON r.id_zona = u.id_zona
                INNER JOIN Estados e ON r.id_estado = e.id_estado
                ORDER BY r.fecha_creacion DESC";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    var reclamos = dt.AsEnumerable().Select(row => new
                    {
                        IdReclamo = Convert.ToInt32(row["id_reclamo"]),
                        Numero = $"#{row["id_reclamo"]}",
                        Tipo = $"Tipo: {row["tipo"]}",
                        Direccion = $"Ubicación: {row["direccion"]}",
                        Fecha = Convert.ToDateTime(row["fecha_creacion"]).ToString("dd/MM/yyyy"),
                        Estado = row["estado"].ToString(),
                        ColorFondo = ObtenerColorFondo(row["estado"].ToString()),
                        ColorEstado = ObtenerColorEstado(row["estado"].ToString())
                    }).ToList();

                    RecentClaimsPanel.ItemsSource = reclamos;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar reclamos recientes: {ex.Message}");
            }
        }

        private Brush ObtenerColorFondo(string estado)
        {
            return estado.ToLower() switch
            {
                "pendiente" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF5E5")),
                "en curso" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E6F0FF")),
                "resuelto" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E5F9E7")),
                _ => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F8F8F8"))
            };
        }

        private void RecentClaim_Click(object sender, MouseButtonEventArgs e)
        {
            var clickedElement = e.OriginalSource as FrameworkElement;
            if (clickedElement == null) return;

            var reclamo = clickedElement.DataContext;
            if (reclamo == null) return;

            int idReclamo = 0;
            var prop = reclamo.GetType().GetProperty("IdReclamo");
            if (prop != null)
                idReclamo = (int)prop.GetValue(reclamo);

            MostrarDetalleReclamo(idReclamo);
        }

        private void MostrarDetalleReclamo(int idReclamo)
        {
            var detalleView = new DetalleReclamo();
            detalleView.Tag = idReclamo;
            Navigator.NavigateTo(detalleView);
        }


        private Brush ObtenerColorEstado(string estado)
        {
            return estado.ToLower() switch
            {
                "pendiente" => Brushes.Orange,
                "en curso" => Brushes.Blue,
                "resuelto" => Brushes.Green,
                _ => Brushes.Black
            };
        }
    }
}
