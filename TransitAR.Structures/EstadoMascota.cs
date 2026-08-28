
namespace TransitAR.Structures
{

    /// <summary>
    /// Tipos de estado de la mascota, 
    /// En refugio es que se encuentra en el refugio y pendiente a realizar una publicacion de adopcion o transito
    /// En transito que se encuentra cursando un transito , se encontraran detalles en el historial de publicaciones
    /// Adoptada fue adoptada con exito, y los datos del adoptante estan en el historial de publicaciones
    /// </summary>

   public enum EstadoMascota
    {
        EnRefugio = 1,
        EnTransito = 2,
        Adoptada = 3
    }
}
