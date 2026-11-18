using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransitAR.Structures
{
    /// <summary>
    /// Resultado del alta del colaborador, rtrae colaborador creado o el error
    /// </summary>
    public class ColaboradorResult
    {
        /// <summary>
        /// Resultado colaborador del alta 
        /// </summary>
        public MiembroEquipoResponse? Colaborador {  get; set; }


        /// <summary>
        /// Error
        /// </summary>
        public string? Error { get; set; }


    }
}
