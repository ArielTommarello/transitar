using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransitAR.Structures
{
    /// <summary>
    /// El resultado de crear o editar una especie. COpia de Postulacion y Tenenca result con error
    /// </summary>
    public class EspecieResult
    {
        /// <summary>
        /// La espceie que se creo o edito puede ser null si ffallo algo
        /// </summary>
        public EspecieResponse? Especie {  get; set; }

        /// <summary>
        /// Motivo del error
        /// </summary>
        public string? Error { get; set; }
    }
}
