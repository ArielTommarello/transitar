using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransitAR.Structures
{
    /// <summary>
    /// Salida de datos de la mascota
    /// </summary>
    public class MascotaResponse
    {
        /// <summary>
        /// Identificador de la mascota
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Nombre de la mascota
        /// </summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Nombre de la especie , ya puesto en el catalogo base
        /// </summary>
        public string EspecieNombre { get; set; } = string.Empty;

        /// <summary>
        /// Nombre de la condicion, ya puesto en el catalogo base
        /// </summary>
        public string CondicionNombre { get; set; } = string.Empty;

        /// <summary>
        /// Raza del animal 
        /// </summary>
        public string? Raza { get; set; }

        /// <summary>
        /// Sexo del animal
        /// </summary>
        public Sexo? Sexo { get; set; }

        /// <summary>
        /// Fecha de nacimiento aproximada , para calculos actualizados
        /// </summary>
        public DateTime? FechaNacimientoAproximada { get; set; }

        /// <summary>
        /// Edad calculada a partir de la fecha de nacimiento , Null si no se conoce o no se puede calcular
        /// </summary>
        public int? EdadAproximadaMeses { get; set; }

        /// <summary>
        /// Tamaño del animal
        /// </summary>
        public Tamanio? Tamanio { get; set; }

        /// <summary>
        /// Indica si tiene el plan de vacunacion al dia
        /// </summary>
        public bool Vacunado { get; set; }

        /// <summary>
        /// Lugar o estado donde esta el animal ahora (inicia en refugio )
        /// </summary>
        public EstadoMascota Estado { get; set; }

        /// <summary>
        /// Fotos del animal en formato JSON
        /// </summary>
        public string? FotosUrl { get; set; }

        /// <summary>
        /// Fecha en que el refugio cargo al animal
        /// </summary>
        public DateTime FechaAlta { get; set; }
    }



}

