

namespace TransitAR.Structures
{
    /// <summary>
    /// Nivel de una persona dentro del refugio. Solo se usa en rol = Refugio.
    /// es un nivel dentro de rolrefugio , usado para que el refugio puedan trbaajar mas de una persona
    /// Fundador: registra el refugio y puede hacer todo (alta y baja del personal)
    /// Colaborador : puede cargar mascotas, revisar rechazar y hacer seguimiento. No puede dar de alta y baja cuentas del refugio
    /// </summary>
    public enum  RolRefugio
    {

        Fundador = 1,
        Colaborador = 2,


    }
}
