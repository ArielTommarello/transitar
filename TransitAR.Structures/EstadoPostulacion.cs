
namespace TransitAR.Structures
{

    /// <summary>
    /// Estado en que se encuentra la postulacion del adoptante o transitante
    /// Pendiente : espera a que el refugio la revisa (ocupa cupo)
    /// EnEspera :El refugio acepto a otro candidato, pero no se concreto la entrega aun (solucion para momentos en que algo pase y no tener que rehacer la publicacion)
    /// Aceptada : el refugio la eligio, se habilita el chat y la publicaicon pasa a pausada
    /// Rechazada : Se descarto por alguna razon, si ObservacionRechazo viene null es porque se selecicono otro candidato
    /// Retirada : el postulante le dio de baja el mismo (libera cupo)
    /// </summary>

   public enum EstadoPostulacion
    {
        Pendiente = 1,
        EnEspera = 2,
        Aceptada = 3,
        Rechazada = 4,
        Retirada = 5
    }
}
