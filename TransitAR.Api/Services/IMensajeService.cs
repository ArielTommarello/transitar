using TransitAR.Structures;

namespace TransitAR.Api.Services
{
    /// <summary>
    /// Chat interno entre refugio y postulante. Conversacion se accede y crea por la postulacion. Por eso es unica entre usuario y refugio
    /// </summary>
    public interface IMensajeService
    {
        /// <summary>
        /// Lista las conversaciones de quien consulta, los filtros son opcionales
        /// </summary>
        /// <param name="usuarioId"></param>
        /// <param name="refugioId"></param>
        /// <param name="publicacionId"></param>
        /// <param name="mascotaId"></param>
        /// <returns></returns>
        Task<List<ChatResumenResponse>> ListarChatsAsync(Guid usuarioId, Guid? refugioId, Guid? publicacionId, Guid? mascotaId);

        /// <summary>
        /// Devuelve la conversacion completa y marca como leidos los mensajes que mando el otro lado
        /// </summary>
        /// <param name="postulacionId"></param>
        /// <param name="usuarioId"></param>
        /// <param name="refugioId"></param>
        /// <returns></returns>
        Task<ChatResponse?> ObtenerChatAsync(Guid postulacionId, Guid usuarioId, Guid? refugioId);

        /// <summary>
        /// Agrega un mensaje a la conversacion, si sigue abierta (los adjuntos ya vienen leidos)
        /// </summary>
        /// <param name="postulacionId"></param>
        /// <param name="request"></param>
        /// <param name="adjuntos"></param>
        /// <param name="usuarioId"></param>
        /// <param name="refugioId"></param>
        /// <returns></returns>
        Task<MensajeResult> EnviarAsync(Guid postulacionId, MensajeRequest request, List<ArchivoAdjunto> adjuntos, Guid usuarioId, Guid? refugioId);

        /// <summary>
        /// Devuelve el contenido de un adjunto, siempre que quien  lo pida sea parte de la conversacion
        /// </summary>
        /// <param name="adjuntoId"></param>
        /// <param name="usuarioId"></param>
        /// <param name="refugioId"></param>
        /// <returns></returns>
        Task<ArchivoAdjunto?> ObtenerAdjuntoAsync(Guid adjuntoId, Guid usuarioId, Guid? refugioId);
    }
}
