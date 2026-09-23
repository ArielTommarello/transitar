using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransitAR.Structures.Requests
{
    /// <summary>
    /// Perfil del refugio con todos sus datos, tiene el mensaje automatico.
    /// </summary>
    public class RefugioResponse
    {
        /// <summary>
        /// Identificador del refugio
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Nombre con el que se muestra el refugio
        /// </summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Descripcion del refugio
        /// </summary>
        public string? Descripcion { get; set; }

        /// <summary>
        /// Logo del refugio
        /// </summary>
        public string? LogoUrl { get; set; }

        /// <summary>
        /// Mail de contacto del refugio
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Telefono de contacto
        /// </summary>
        public string? Telefono { get; set; }

        /// <summary>
        /// Domicilio del refugio
        /// </summary>
        public string? Direccion { get; set; }

        /// <summary>
        /// Localidad donde funciona
        /// </summary>
        public string? Localidad { get; set; }

        /// <summary>
        /// Mensaje que se usa al rechazar a un postulante sin motivo escrito a mano
        /// </summary>
        public string? MensajeRechazoAutomatico { get; set; }

        /// <summary>
        /// Fecha en que el refugio se dio de alta en la plataforma
        /// </summary>
        public DateTime FechaAlta { get; set; }

        /// <summary>
        /// Redes y links cargados por el refugio
        /// </summary>
        public List<ContactoResponse> Contactos { get; set; } = new();
    }
}
