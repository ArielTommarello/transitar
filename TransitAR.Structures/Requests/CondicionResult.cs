using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransitAR.Structures
{
    /// <summary>
    /// El resultado de crear o editar una condicion. COpia de Postulacion y Tenenca result con error
    /// </summary>
    public class CondicionResult
    {
        /// <summary>
        /// La econdicon que se creo o edito puede ser null si ffallo algo
        /// </summary>
        public CondicionResponse? Condicion { get; set; }

        /// <summary>
        /// Motivo del error
        /// </summary>
        public string? Error { get; set; }

    }
}
