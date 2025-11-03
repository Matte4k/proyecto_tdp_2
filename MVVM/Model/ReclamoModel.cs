namespace proyecto_tdp_2.MVVM.Model
{
    public class Reclamo
    {
        public int IdReclamo { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string Prioridad { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public string DniCliente { get; set; } = string.Empty;
        public string Cliente { get; set; } = string.Empty;
    }
}