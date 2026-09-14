using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransitAR.Structures
{
    /// <summary>
    /// Motivo con el cual el refugio rechaza una postulacion. SI esta vacio e sporque se eligio otro candidato y se muestra la frase de defecto 
    /// </summary>
    public class RechazoRequest
    {

        /// <summary>
        /// Motivo del rechazo (opcional)
        /// </summary>
        [MaxLength(500)]
        public string? Observacion { get; set; }


    }
}
