using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransitAR.Structures
{
    /// <summary>
    /// Publicacion tal como la ve el refugio que la creo, con el estado y cantidad de postulaciones (uso para el refugio)

    public class PublicacionResponse
    {


        /// <summary>
        /// Identificador de la publicacion
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Mascota sobre la que se hace la publicaiocn
        /// </summary>
        public Guid MascotaId { get; set; }

        /// <summary>
        /// Nombre de la mascota (ya puesto)
        /// </summary>
        public string MascotaNombre { get; set; } = string.Empty;

        /// <summary>
        /// Titulo de la publicaicon
        /// </summary>
        public string Titulo { get; set; } = string.Empty;

        /// <summary>
        /// Descripcion de la publicacion y requisitos del refugio sobre esto
        /// </summary>
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>
        /// Zona donde esta la mascota
        /// </summary>
        public string Ubicacion { get; set; } = string.Empty;

        /// <summary>
        /// Si la publciacion es de adopcion o de transito
        /// </summary>
        public TipoPublicacion Tipo { get; set; }

        /// <summary>
        /// Estado actual de la publicaicon
        /// </summary>
        public EstadoPublicacion Estado { get; set; }

        /// <summary>
        /// Fotos adicionales del aviso en JSON . Las otiginales estan en mascota
        /// </summary>
        public string? FotosUrlExtra { get; set; }

        /// <summary>
        /// Duracion estimada del transito en dias (solo para transito)
        /// </summary>
        public int? PlazoEstimado { get; set; }

        /// <summary>
        /// Fecha en que se publico el aviso
        /// </summary>
        public DateTime FechaPublicacion { get; set; }

        /// <summary>
        /// Fecha en que se cerro el aviso. Null si sigue abierto
        /// </summary>
        public DateTime? FechaCierre { get; set; }

        /// <summary>
        /// Cuantas personas se postularon , lo primero que peude ver el refugio 
        /// </summary>
        public int CantidadPostulaciones { get; set; }
    }


}

