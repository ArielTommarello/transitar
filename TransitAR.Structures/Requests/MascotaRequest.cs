using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransitAR.Structures
{

    /// <summary>
    /// Datos que el refugio caraga al crear o editar una mascota
    /// RefugioId sale del token , estado y fechaAlta se pone en el momento de creacion
    /// </summary>

    public class MascotaRequest
    {

        /// <summary>
        /// Nombre de la mascota de ese refugio
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;


        /// <summary>
        /// Especie elegida del catalogo base (por ahora)
        /// </summary>
        public Guid EspecieId { get; set; }

        /// <summary>
        /// Condicion elegida del catalogo base (por ahora)
        /// </summary>
        public Guid CondicionId { get; set; }

        /// <summary>
        /// Raza del animal si se conoce
        /// </summary>
        [MaxLength(150)]
        public string? Raza { get; set; }

        /// <summary>
        /// Sexo del animal
        /// </summary>
        public Sexo? Sexo { get; set; }

        /// <summary>
        /// Fecha de nacimiento aproximada , Null si no se conoce la edad. Cone sto puedo calcular una actualizacion de edades
        /// </summary>
        public DateTime? FechaNacimientoAproximada { get; set; }

        /// <summary>
        /// Tamaño del animal (grande, mediano, chico)
        /// </summary>
        public Tamanio? Tamanio { get; set; }

        /// <summary>
        /// Indica si tiene el plan de vacunacion al dia o esta vacunado
        /// </summary>
        public bool Vacunado { get; set; }

        /// <summary>
        /// Fotos del animal en formato JSON
        /// </summary>
        public string? FotosUrl { get; set; }



    }
}
