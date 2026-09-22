using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransitAR.Structures.Requests
{
    /// <summary>
    /// Datos de uso para el refugio, soluciona el caso en cque si se adopta y devuelve. Haya una manera de dejar de antecedente, pára futuras ocasiones.
    /// Tambien contempla el caso de devolcuion en un transito y luego trasnformacion a adopcion.
    /// </summary>
    public class DevolucionRequest
    {

        /// <summary>
        /// Observacion del refugio sobre como fue la estadia de la mascota (opcional)
        /// </summary>
        [MaxLength(1000)]
        public string? ObservacionCierre { get; set; }

        /// <summary>
        /// Como termino la adocpion o transito, Un transito devuelto de buena manera va en true
        /// </summary>
        public bool FinalizoBien { get; set; }
    }
}
