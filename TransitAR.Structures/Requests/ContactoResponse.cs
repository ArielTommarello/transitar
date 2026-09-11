using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransitAR.Structures
{
    /// <summary>
    /// REd social del postulante, respeusta de la api
    /// </summary>
    public class ContactoResponse
    {
        /// <summary>
        /// Tipo de red que es
        /// </summary>
        public TipoContacto Tipo { get; set; }

        /// <summary>
        /// Direccion del perfil 
        /// </summary>
        public string Url { get; set; } = string.Empty;
    }
}
