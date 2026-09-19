using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransitAR.Structures
{
    /// <summary>
    /// Tenecia como  la ve el refugio: animal, quien lo tiene , fechas desde y hasta cuando y si era transito como termino
    /// </summary>
    public class TenenciaResponse
    {
        /// <summary>
        /// Identificador de la tenencia
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Postulacion de la que suege la tenencia
        /// </summary>
        public Guid PostulacionId { get; set; }

        /// <summary>
        /// Si es transito o adopcion ,cambia a adopcion si un transito decide adoptar
        /// </summary>
        public TipoPublicacion Modalidad { get; set; }

        /// <summary>
        /// Mascota entregada y de la cual era la postulacuion
        /// </summary>
        public Guid MascotaId { get; set; }

        /// <summary>
        /// Nombre de la mascota
        /// </summary>
        public string MascotaNombre { get; set; } = string.Empty;

        /// <summary>
        /// Peostulante que tiene al animal, para consultar su historial
        /// </summary>
        public Guid UsuarioId { get; set; }

        /// <summary>
        /// Nombre de la persona que tiene a la mascota
        /// </summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Apellido de la persona que tiene a la mascota
        /// </summary>
        public string Apellido { get; set; } = string.Empty;

        /// <summary>
        /// Fecha en que se entrego a la mascota
        /// </summary>
        public DateTime FechaInicio { get; set; }

        /// <summary>
        /// Fecha pactada de devolucion. Null en adopciones o si el plazo quedo abierto , solo funciona en transitos
        /// </summary>
        public DateTime? FechaFinEstimada { get; set; }

        /// <summary>
        /// Fecha en que el animal volvio al refugio, null mientras sigue entregado
        /// </summary>
        public DateTime? FechaFinReal { get; set; }

        /// <summary>
        /// Fecha en que un transito paso a ser adopcion , null si no hubo conversion (el transito decidio adoptar a la mascota)
        /// </summary>
        public DateTime? FechaConversion { get; set; }

        /// <summary>
        /// Observacion del refugio al cerrar la tenencia (en caso de transito se cierra porque vuelve, tambien peude ser porque se devolvio antes de teiempo)
        /// </summary>
        public string? ObservacionCierre { get; set; }

        /// <summary>
        /// cierre final del refugio sobre la estadia, Null mientras la tenencia sigue en curso
        /// </summary>
        public bool? FinalizoBien { get; set; }

        /// <summary>
        /// Transito en curso cuya fecha pactada de devolucion ya paso. Siempre es false en caso de adopcion, tenencia cerrada o plazo abierto y no vencido.
        /// </summary>
        public bool EstaVencida { get; set; }
    }
}   
