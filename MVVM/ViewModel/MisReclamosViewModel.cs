using proyecto_tdp_2.Helpers;
using proyecto_tdp_2.MVVM.Model;
using proyecto_tdp_2.MVVM.View;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace proyecto_tdp_2.MVVM.ViewModel
{
    public class MisReclamosViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Reclamo> Reclamos { get; set; } = new();

        private string? _dniBusqueda;
        public string? DniBusqueda
        {
            get => _dniBusqueda;
            set { _dniBusqueda = value; OnPropertyChanged(); }
        }

        private string? _tipoSeleccionado = "Todos";
        public string? TipoSeleccionado
        {
            get => _tipoSeleccionado;
            set { _tipoSeleccionado = value; OnPropertyChanged(); }
        }

        private string? _prioridadSeleccionada = "Todos";
        public string? PrioridadSeleccionada
        {
            get => _prioridadSeleccionada;
            set { _prioridadSeleccionada = value; OnPropertyChanged(); }
        }

        public List<string> EstadosDisponibles { get; } = new() { "Todos", "Pendiente", "En Curso", "Resuelto" };

        private string _estadoSeleccionado = "Todos";
        public string EstadoSeleccionado
        {
            get => _estadoSeleccionado;
            set { _estadoSeleccionado = value; OnPropertyChanged(); }
        }

        public ICommand FiltrarCommand { get; }
        public ICommand AnteriorCommand { get; }
        public ICommand SiguienteCommand { get; }
        public ICommand VerDetallesCommand { get; }

        private int _paginaActual = 1;
        private int _itemsPorPagina = 5;
        public string PaginaActualTexto => $"Página {_paginaActual}";

        public MisReclamosViewModel()
        {
            FiltrarCommand = new RelayCommand(obj => CargarReclamos());
            AnteriorCommand = new RelayCommand(obj => CambiarPagina(-1), obj => _paginaActual > 1);
            SiguienteCommand = new RelayCommand(obj => CambiarPagina(1), obj => Reclamos.Count == _itemsPorPagina);
            VerDetallesCommand = new RelayCommand(obj => VerDetalles(obj));

            CargarReclamos();
        }

        private void CambiarPagina(int cambio)
        {
            _paginaActual += cambio;
            CargarReclamos();
        }

        private void VerDetalles(object? parametro)
        {
            if (parametro is Reclamo reclamo)
            {
                var detalleView = new DetalleReclamo();
                detalleView.Tag = reclamo.IdReclamo;
                Navigator.NavigateTo(detalleView);
            }
        }

        private void CargarReclamos()
        {
            Reclamos.Clear();

            string connectionString = ConfigurationManager.ConnectionStrings["MiReclamoDB"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string query = @"
            SELECT 
                r.id_reclamo,
                r.descripcion,
                U.direccion,
                P.nombre AS prioridad,
                T.nombre AS tipo_reclamo,
                E.nombre AS estado,
                r.fecha_creacion,
                C.dni AS dni_cliente,
                C.nombre + ' ' + C.apellido AS cliente
            FROM Reclamos r
            INNER JOIN Clientes C ON r.cliente_reclamo = C.id_cliente
            INNER JOIN Prioridades P ON r.prioridad = P.id_prioridad
            INNER JOIN TipoReclamo T ON r.tipo_reclamo = T.id_tipo
            INNER JOIN Estados E ON r.id_estado = E.id_estado
            INNER JOIN Ubicacion U ON r.id_zona = U.id_zona
            WHERE 
                (@dni IS NULL OR C.dni = @dni)
                AND (@tipo = 'Todos' OR T.nombre = @tipo)
                AND (@prioridad = 'Todos' OR P.nombre = @prioridad)
                AND (@estado = 'Todos' OR E.nombre = @estado)
            ORDER BY r.fecha_creacion DESC
            OFFSET @offset ROWS FETCH NEXT @limit ROWS ONLY;
        ";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (string.IsNullOrWhiteSpace(DniBusqueda) || !long.TryParse(DniBusqueda, out long dniParsed))
                        cmd.Parameters.Add("@dni", SqlDbType.BigInt).Value = DBNull.Value;
                    else
                        cmd.Parameters.Add("@dni", SqlDbType.BigInt).Value = dniParsed;

                    cmd.Parameters.Add("@tipo", SqlDbType.VarChar).Value = TipoSeleccionado ?? "Todos";
                    cmd.Parameters.Add("@prioridad", SqlDbType.VarChar).Value = PrioridadSeleccionada ?? "Todos";
                    cmd.Parameters.Add("@estado", SqlDbType.VarChar).Value = EstadoSeleccionado ?? "Todos";

                    cmd.Parameters.Add("@offset", SqlDbType.Int).Value = (_paginaActual - 1) * _itemsPorPagina;
                    cmd.Parameters.Add("@limit", SqlDbType.Int).Value = _itemsPorPagina;

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Reclamo reclamo = new Reclamo
                            {
                                IdReclamo = reader.GetInt32(0),
                                Descripcion = reader.IsDBNull(1) ? "" : reader.GetString(1),
                                Direccion = reader.IsDBNull(2) ? "" : reader.GetString(2),
                                Prioridad = reader.IsDBNull(3) ? "" : reader.GetString(3),
                                Tipo = reader.IsDBNull(4) ? "" : reader.GetString(4),
                                Estado = reader.IsDBNull(5) ? "" : reader.GetString(5),
                                FechaCreacion = reader.GetDateTime(6),
                                DniCliente = reader.IsDBNull(7) ? "" : reader.GetInt64(7).ToString(),
                                Cliente = reader.IsDBNull(8) ? "" : reader.GetString(8)
                            };

                            Reclamos.Add(reclamo);
                        }
                    }
                }
            }

            OnPropertyChanged(nameof(Reclamos));
            OnPropertyChanged(nameof(PaginaActualTexto));
            CommandManager.InvalidateRequerySuggested();
        }


        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
