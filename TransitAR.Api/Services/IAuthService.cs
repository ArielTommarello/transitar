using TransitAR.Structures;

namespace TransitAR.Api.Services
{
    /// <summary>
    /// Interfaz para el manejo de resgitro y autenticacion de cuentas
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Interfaz. Registra una persona que quiere adoptar o trasnitar
        /// </summary>
        /// <param name="request">datos del formulario del registro en Request Structures</param>
        /// <returns>El usuario creado, null en caso de ya existir</returns>
        Task<Usuario?> RegistrarPostulanteAsync(RegistroPostulanteRequest request);


        /// <summary>
        /// Interfaz. Registra un usuario fundador y el refugio (datos iniciales)
        /// 
        /// </summary>
        /// <param name="request">datos del formulario del registro en Request Structures para el refugio y el usuario fundador</param>
        /// <returns>El usuario creado, null en caso de ya existir</returns>
        Task<Usuario?> RegistrarRefugioAsync(RegistroRefugioRequest request);


    }
}
