using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransitAR.Structures.Requests
{

    /// <summary>
    /// Historial de etenencias anteriores del postulante. Para que el refugio lo vea cuando este por decidir, otro refugio tambien puede verlo, por eso pongo nombre del que lo escirbio (refugio)
    /// </summary>
    public class HistorialTenenciaResponse
    {
        /// <summary>
        /// Identificador de la tenencia
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Mascota que tuvo a cargo
        /// </summary>
        public string MascotaNombre { get; set; } = string.Empty;

        /// <summary>
        /// Refugio que entrego el animal y que escribio el cierre. (Puede ser el que no esta consultando o evaluando en el momento)
        /// </summary>
        public string RefugioNombre { get; set; } = string.Empty;

        /// <summary>
        /// Si fue transito o adopcion
        /// </summary>
        public TipoPublicacion Modalidad { get; set; }

        /// <summary>
        /// Fecha en que recibio la mascota
        /// </summary>
        public DateTime FechaInicio { get; set; }

        /// <summary>
        /// Fecha pactada de devolucion, solo en transitos, null en adopciones o si el plazo quedo abierto
        /// </summary>
        public DateTime? FechaFinEstimada { get; set; }

        /// <summary>
        /// Fecha en que el animal volvio al refugio , Null si sigue con el postulante
        /// </summary>
        public DateTime? FechaFinReal { get; set; }

        /// <summary>
        /// Fecha en que un transito paso a adopcion definitiva, null si no hubo conversion
        /// </summary>
        public DateTime? FechaConversion { get; set; }

        /// <summary>
        /// pequeño balance del refugio sobre la estadia, null mientras la tenencia sigue abierta
        /// </summary>
        public bool? FinalizoBien { get; set; }

        /// <summary>
        /// breve comentario del refugio al cerrar
        /// </summary>
        public string? ObservacionCierre { get; set; }

        /// <summary>
        /// Si esta persona tiene un animal a cargo ahora mismo ,dato importante para decidir si entregarle otro
        /// </summary>
        public bool EnCurso { get; set; }

        /// <summary>
        /// Transito en curso cuya fecha pactada ya paso. Si viene en true, esta persona le debe un animal a otro refugio ahora mismo
        /// </summary>
        public bool VencidaEnCurso { get; set; }

        /// <summary>
        /// Dias que duro la tenencia si todavia esta en curso son los dias que pasaron hasta hoy ( mirar EnCurso para saber cual de los dos es)
        /// </summary>
        public int DuracionEnDias { get; set; }
    }
}
