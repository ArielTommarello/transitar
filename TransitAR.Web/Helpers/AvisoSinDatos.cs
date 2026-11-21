namespace TransitAR.Web.Helpers
{
    /// <summary>
    /// Datos para el partial del aviso cuando no tiene datos para mostrar las cards (uso de record C# 9)
    /// </summary>
    public record AvisoSinDatos(string Icono, string Texto, string? Detalle = null, string? BotonTexto = null, string? BotonUrl = null);
}
