using proyecto_tdp_2.MVVM.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Data.SqlClient;
using System.Runtime.CompilerServices;

namespace proyecto_tdp_2.MVVM.ViewModel
{
    public class DashboardViewModel : INotifyPropertyChanged
    {
        private string _connectionString = ConfigurationManager.ConnectionStrings["MiReclamoDB"].ConnectionString;

        // Totales
        private int _totalReclamos;
        public int TotalReclamos { get => _totalReclamos; set { _totalReclamos = value; OnPropertyChanged(); } }

        private int _reclamosAbiertos;
        public int ReclamosAbiertos { get => _reclamosAbiertos; set { _reclamosAbiertos = value; OnPropertyChanged(); } }

        private int _reclamosCerrados;
        public int ReclamosCerrados { get => _reclamosCerrados; set { _reclamosCerrados = value; OnPropertyChanged(); } }

        private int _reclamosPendientes;
        public int ReclamosPendientes { get => _reclamosPendientes; set { _reclamosPendientes = value; OnPropertyChanged(); } }

        public ObservableCollection<KeyValuePair<string, int>> ReclamosPorZona { get; set; } = new();
        public ObservableCollection<KeyValuePair<string, int>> ReclamosPorPrioridad { get; set; } = new();

        public ObservableCollection<Reclamo> ReclamosRecientes { get; set; } = new();

        public DashboardViewModel()
        {
            CargarDatos();
        }

        private void CargarDatos()
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            conn.Open();

            using (SqlCommand cmd = new SqlCommand(@"
                SELECT COUNT(*) FROM Reclamos;
                SELECT COUNT(*) FROM Reclamos r INNER JOIN Estados e ON r.id_estado = e.id_estado WHERE e.nombre = 'Abierto';
                SELECT COUNT(*) FROM Reclamos r INNER JOIN Estados e ON r.id_estado = e.id_estado WHERE e.nombre = 'Cerrado';
                SELECT COUNT(*) FROM Reclamos r INNER JOIN Estados e ON r.id_estado = e.id_estado WHERE e.nombre = 'Pendiente';
            ", conn))
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                if (reader.Read()) TotalReclamos = reader.GetInt32(0);
                if (reader.NextResult() && reader.Read()) ReclamosAbiertos = reader.GetInt32(0);
                if (reader.NextResult() && reader.Read()) ReclamosCerrados = reader.GetInt32(0);
                if (reader.NextResult() && reader.Read()) ReclamosPendientes = reader.GetInt32(0);
            }

            using (SqlCommand cmd = new SqlCommand(@"
                    SELECT P.nombre AS provincia, COUNT(*) AS cantidad
                    FROM Reclamos R
                    INNER JOIN Ubicacion U ON R.id_zona = U.id_zona
                    INNER JOIN Provincia P ON U.id_provincia = P.id_provincia
                    GROUP BY P.nombre
                    ORDER BY cantidad DESC;", conn))
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                ReclamosPorZona.Clear();
                while (reader.Read())
                    ReclamosPorZona.Add(new KeyValuePair<string, int>(
                        reader.GetString(0),
                        reader.GetInt32(1)
                    ));
            }


            using (SqlCommand cmd = new SqlCommand(@"
                SELECT P.nombre, COUNT(*) AS cantidad
                FROM Reclamos R
                INNER JOIN Prioridades P ON R.prioridad = P.id_prioridad
                GROUP BY P.nombre
                ORDER BY cantidad DESC;", conn))
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                ReclamosPorPrioridad.Clear();
                while (reader.Read())
                    ReclamosPorPrioridad.Add(new KeyValuePair<string, int>(reader.GetString(0), reader.GetInt32(1)));
            }

            using (SqlCommand cmd = new SqlCommand(@"
                    SELECT TOP 5 
                        r.id_reclamo,
                        r.descripcion,
                        P2.nombre AS provincia,
                        P.nombre AS prioridad,
                        E.nombre AS estado,
                        r.fecha_creacion
                    FROM Reclamos r
                    INNER JOIN Prioridades P ON r.prioridad = P.id_prioridad
                    INNER JOIN Estados E ON r.id_estado = E.id_estado
                    INNER JOIN Ubicacion U ON r.id_zona = U.id_zona
                    INNER JOIN Provincia P2 ON U.id_provincia = P2.id_provincia
                    ORDER BY r.fecha_creacion DESC;", conn))
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                ReclamosRecientes.Clear();
                while (reader.Read())
                {
                    ReclamosRecientes.Add(new Reclamo
                    {
                        IdReclamo = reader.GetInt32(0),
                        Descripcion = reader.GetString(1),
                        Direccion = reader.GetString(2),
                        Prioridad = reader.GetString(3),
                        Estado = reader.GetString(4),
                        FechaCreacion = reader.GetDateTime(5)
                    });
                }
            }

        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
