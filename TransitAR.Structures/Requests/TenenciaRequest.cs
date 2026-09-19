using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransitAR.Structures
{
    /// <summary>
    /// Datos que hacen a la confirmacion de la entrega del animal. Se necesita la publicacion y la fecha en que termina en caso de  transito. La de inicio se pone automaticamente
    /// </summary>
    public class TenenciaRequest
    {
        /// <summary>
        /// Postulacion aceptada de la que nace esta tenencia
        /// </summary>
        public Guid PostulacionId { get; set; }

        /// <summary>
        /// Fecha pactada de devolucion. Solo aplica a transito , en una adopcion se ignora. Null si el plazo quedo abierto
        /// </summary>
        public DateTime? FechaFinEstimada { get; set; }


    }
}
