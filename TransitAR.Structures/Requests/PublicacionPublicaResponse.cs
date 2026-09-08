using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransitAR.Structures
{
    /// <summary>
    /// Publicacion tal como la ve el postulante o un visitante sin cuenta. Trae los datos de la mascota y del refugio para armar la tarjeta, no tiene los mismos datos que peude ver el refugio.
    /// </summary>

    public class PublicacionPublicaResponse
    {
        /// <summary>
        /// Identificador de la publicacion
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Titulo de la publicacion
        /// </summary>
        public string Titulo { get; set; } = string.Empty;

        /// <summary>
        /// Descripcion de la publicacion
        /// </summary>
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>
        /// Zona donde esta la mascota
        /// </summary>
        public string Ubicacion { get; set; } = string.Empty;

        /// <summary>
        /// Si la publicacion es de adopcion o de transito
        /// </summary>
        public TipoPublicacion Tipo { get; set; }

        /// <summary>
        /// Fotos adicionales del aviso en JSON , las otriginales estan en la mascota
        /// </summary>
        public string? FotosUrlExtra { get; set; }

        /// <summary>
        /// Duracion estimada del transito en dias (solo para transitos)
        /// </summary>
        public int? PlazoEstimado { get; set; }

        /// <summary>
        /// Fecha en que se publico la publiciacion
        /// </summary>
        public DateTime FechaPublicacion { get; set; }

        /// <summary>
        /// Nombre de la mascota (ya resuelto, para no tener que realizar busqueda)
        /// </summary>
        public string MascotaNombre { get; set; } = string.Empty;

        /// <summary>
        /// Especie de la mascota (ya resuelto, para no tener que realizar busqueda)
        /// </summary>
        public string EspecieNombre { get; set; } = string.Empty;

        /// <summary>
        /// Sexo de la mascota , (ya resuelto, para no tener que realizar busqueda)
        /// </summary>
        public Sexo? Sexo { get; set; }

        /// <summary>
        /// Tamanio de la mascota,(ya resuelto, para no tener que realizar busqueda)
        /// </summary>
        public Tamanio? Tamanio { get; set; }

        /// <summary>
        /// Edad calculada de la mascota en meses
        /// </summary>
        public int? EdadAproximadaMeses { get; set; }

        /// <summary>
        /// Si tiene el plan de vacunacion al dia
        /// </summary>
        public bool Vacunado { get; set; }

        /// <summary>
        /// Fotos principales de la mascota en JSON
        /// </summary>
        public string? FotosUrl { get; set; }

        /// <summary>
        /// Nombre del refugio que publica 
        /// </summary>
        public string RefugioNombre { get; set; } = string.Empty;

        /// <summary>
        /// Localidad del refugio, para filtrar por cercania
        /// </summary>
        public string? RefugioLocalidad { get; set; }




    }
}
