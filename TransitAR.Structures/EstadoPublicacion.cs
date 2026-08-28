
namespace TransitAR.Structures
{

    /// <summary>
    /// Estados de una publicacion
    /// Activo mientras se esta en la busqueda
    /// Pausada cuando el refugio acepta un postulante pero tiene que verificar requerimientos y hacer el primer acercamiento (deja de recibir postulantes, pero puede volver a Activa si la gestion no avanza).
    /// Cerrada cuando se concreto la adopcion o se dio de baja la publicacion
    /// </summary>
    public enum EstadoPublicacion
    {
        Activa=1,
        Pausada=2,
        Cerrada =3
    }
}
