using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.SKCharts;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Runtime.CompilerServices;

namespace proyecto_tdp_2.MVVM.ViewModel
{
    public class ReportesViewModel : BaseViewModel
    {
        string connectionString = ConfigurationManager.ConnectionStrings["MiReclamoDB"].ConnectionString;

        private int _reclamosResueltos;
        public int ReclamosResueltos
        {
            get => _reclamosResueltos;
            set { _reclamosResueltos = value; OnPropertyChanged(); }
        }

        private double _tiempoPromedioResolucion;
        public double TiempoPromedioResolucion
        {
            get => _tiempoPromedioResolucion;
            set { _tiempoPromedioResolucion = value; OnPropertyChanged(); }
        }

        private int _reclamosAltaPrioridad;
        public int ReclamosAltaPrioridad
        {
            get => _reclamosAltaPrioridad;
            set { _reclamosAltaPrioridad = value; OnPropertyChanged(); }
        }

        private ISeries[] _seriesEstados;
        public ISeries[] SeriesEstados
        {
            get => _seriesEstados;
            set { _seriesEstados = value; OnPropertyChanged(); }
        }

        private Axis[] _xAxisEstados;
        public Axis[] XAxisEstados
        {
            get => _xAxisEstados;
            set { _xAxisEstados = value; OnPropertyChanged(); }
        }

        private ISeries[] _seriesServicios;
        public ISeries[] SeriesServicios
        {
            get => _seriesServicios;
            set { _seriesServicios = value; OnPropertyChanged(); }
        }

        private ObservableCollection<OperadorRendimiento> _rendimientoOperadores;
        public ObservableCollection<OperadorRendimiento> RendimientoOperadores
        {
            get => _rendimientoOperadores;
            set { _rendimientoOperadores = value; OnPropertyChanged(); }
        }

        public ReportesViewModel()
        {
            CargarIndicadores();
            CargarGraficoEstados();
            CargarGraficoServicios();
            CargarRendimientoOperadores();
        }

        private void CargarIndicadores()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Reclamos WHERE id_estado = (SELECT id_estado FROM Estados WHERE nombre='Resuelto')", conn))
                    ReclamosResueltos = (int)cmd.ExecuteScalar();

                using (SqlCommand cmd = new SqlCommand("SELECT AVG(DATEDIFF(DAY, fecha_creacion, fecha_cierre)) FROM Reclamos WHERE fecha_cierre IS NOT NULL", conn))
                {
                    var result = cmd.ExecuteScalar();
                    TiempoPromedioResolucion = result != DBNull.Value ? Convert.ToDouble(result) : 0;
                }

                using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Reclamos WHERE id_estado <> (SELECT id_estado FROM Estados WHERE nombre='Resuelto') AND prioridad = (SELECT id_prioridad FROM Prioridades WHERE nombre='Alta')", conn))
                    ReclamosAltaPrioridad = (int)cmd.ExecuteScalar();
            }
        }

        private void CargarGraficoEstados()
        {
            var labels = new List<string>();
            var values = new List<int>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"
            SELECT E.nombre, COUNT(*) 
            FROM Reclamos R
            INNER JOIN Estados E ON R.id_estado = E.id_estado
            GROUP BY E.nombre", conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        labels.Add(reader.GetString(0));
                        values.Add(reader.GetInt32(1));
                    }
                }
            }

            SeriesEstados = new ISeries[]
            {
        new ColumnSeries<int>
        {
            Name = "Estados de Reclamos",
            Values = values,
            Fill = new SolidColorPaint(SKColors.SteelBlue),
            DataLabelsPaint = new SolidColorPaint(SKColors.Black),
            DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Top,
            DataLabelsSize = 14
        }
            };

            XAxisEstados = new[]
            {
        new Axis
        {
            Labels = labels.ToArray(),
            LabelsRotation = 0
        }
    };
        }


        private void CargarGraficoServicios()
        {
            var series = new List<PieSeries<int>>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"
                    SELECT TOP 3 T.nombre, COUNT(*) 
                    FROM Reclamos R
                    INNER JOIN TipoReclamo T ON R.tipo_reclamo = T.id_tipo
                    GROUP BY T.nombre
                    ORDER BY COUNT(*) DESC", conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        series.Add(new PieSeries<int>
                        {
                            Name = reader.GetString(0),
                            Values = new[] { reader.GetInt32(1) },
                            DataLabelsSize = 12,
                            DataLabelsPaint = new SolidColorPaint(SKColors.Black)
                        });
                    }
                }

                using (SqlCommand cmd = new SqlCommand(@"
                    SELECT COUNT(*) FROM Reclamos 
                    WHERE tipo_reclamo NOT IN (
                        SELECT TOP 3 id_tipo FROM TipoReclamo R
                        INNER JOIN Reclamos RC ON R.id_tipo = RC.tipo_reclamo
                        GROUP BY id_tipo ORDER BY COUNT(*) DESC)", conn))
                {
                    int otros = (int)cmd.ExecuteScalar();
                    if (otros > 0)
                        series.Add(new PieSeries<int> { Name = "Otros", Values = new[] { otros } });
                }
            }

            SeriesServicios = series.ToArray();
        }

        private void CargarRendimientoOperadores()
        {
            RendimientoOperadores = new ObservableCollection<OperadorRendimiento>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"
                        SELECT 
                            CONCAT(U.nombre, ' ', U.apellido) AS operador,
                            COUNT(A.reclamo_asignado) AS asignados,
                            SUM(CASE WHEN E.nombre = 'Resuelto' THEN 1 ELSE 0 END) AS resueltos,
                            AVG(CASE WHEN R.fecha_cierre IS NOT NULL THEN DATEDIFF(DAY, R.fecha_creacion, R.fecha_cierre) END) AS promedio
                        FROM Usuario U
                        INNER JOIN AsignacionReclamo A ON U.id_usuario = A.usuario_asignado
                        INNER JOIN Reclamos R ON R.id_reclamo = A.reclamo_asignado
                        INNER JOIN Estados E ON R.id_estado = E.id_estado
                        GROUP BY U.nombre, U.apellido;", conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    RendimientoOperadores.Clear();
                    while (reader.Read())
                    {
                        RendimientoOperadores.Add(new OperadorRendimiento
                        {
                            Nombre = reader.GetString(0),
                            Asignados = reader.GetInt32(1),
                            Resueltos = reader.GetInt32(2),
                            PromedioDias = reader.IsDBNull(3) ? 0 : reader.GetInt32(3)
                        });
                    }
                }
            }
        }

        private byte[] RenderGraficoEstados()
        {
            var chart = new SKCartesianChart
            {
                Width = 600,
                Height = 400,
                Series = SeriesEstados,
                XAxes = XAxisEstados
            };

            using var image = chart.GetImage();
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            return data.ToArray();
        }

        private byte[] RenderGraficoServicios()
        {
            var chart = new SKPieChart
            {
                Width = 400,
                Height = 400,
                Series = SeriesServicios
            };

            using var image = chart.GetImage();
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            return data.ToArray();
        }

        public void GenerarReportePDF()
        {
            var pdf = new PdfDocument();
            var page = pdf.AddPage();
            var gfx = XGraphics.FromPdfPage(page);

            var fontTitle = new XFont("Arial", 20, XFontStyle.Bold);
            gfx.DrawString("Reporte de Reclamos", fontTitle, XBrushes.Black, new XPoint(40, 40));

            var fontBody = new XFont("Arial", 12);
            gfx.DrawString($"Reclamos Resueltos: {ReclamosResueltos}", fontBody, XBrushes.Black, new XPoint(40, 80));
            gfx.DrawString($"Tiempo Promedio de Resolución: {TiempoPromedioResolucion:N1} días", fontBody, XBrushes.Black, new XPoint(40, 100));
            gfx.DrawString($"Reclamos Alta Prioridad: {ReclamosAltaPrioridad}", fontBody, XBrushes.Black, new XPoint(40, 120));

            byte[] imgEstados = RenderGraficoEstados();
            using (var ms = new MemoryStream(imgEstados))
            {
                var img = XImage.FromStream(() => ms);
                gfx.DrawImage(img, 40, 150, 300, 200);
            }

            byte[] imgServicios = RenderGraficoServicios();
            using (var ms = new MemoryStream(imgServicios))
            {
                var img = XImage.FromStream(() => ms);
                gfx.DrawImage(img, 360, 150, 200, 200);
            }

            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "ReporteReclamos.pdf");
            pdf.Save(path);

            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
            {
                FileName = path,
                UseShellExecute = true
            });
        }
    }

    public class OperadorRendimiento
    {
        public string Nombre { get; set; }
        public int Asignados { get; set; }
        public int Resueltos { get; set; }
        public double PromedioDias { get; set; }
    }

    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
