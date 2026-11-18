using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using TransitAR.Api.Services;
using TransitAR.Structures;
using TransitAR.Tests.Soporte;

namespace TransitAR.Tests.Services
{
    public class RefugioServiceTests : BaseEnMemoria
    {
        //guids
        private readonly Guid _patitasId = Guid.NewGuid();
        private readonly Guid _surId = Guid.NewGuid();
        private readonly Guid _instagramId = Guid.NewGuid();
        private readonly DateTime _fechaAlta = new DateTime(2026, 2, 1);

        //guids equipo
        private readonly Guid _fundadorId = Guid.NewGuid();
        private readonly Guid _colaboradorId = Guid.NewGuid();
        private readonly Guid _colaboradorBloqueadoId = Guid.NewGuid();
        private readonly Guid _fundadorSurId = Guid.NewGuid();
        private readonly Guid _colaboradorSurId = Guid.NewGuid();
        private const string Clave = "claveprueba2026";


        //Creo el nuevo contexto
        public RefugioServiceTests()
        {
                       
            using var contexto = CrearContexto();

            contexto.Refugios.Add(new Refugio
            {
                Id = _patitasId,
                Nombre = "Patitas",
                Email = "patitas@test.org",
                Localidad = "Lomas",
                Activo = true,
                FechaAlta = _fechaAlta,
                Contactos = new List<ContactoRefugio>
                {
                    new() { Id = _instagramId, RefugioId = _patitasId, Tipo = TipoContacto.Instagram, Url = "instagram.com/patitas" },
                    new() { Id = Guid.NewGuid(), RefugioId = _patitasId, Tipo = TipoContacto.Facebook, Url = "facebook.com/patitas" }
                }
            });
            contexto.Refugios.Add(new Refugio { Id = _surId, Nombre = "Refugio del Sur", Email = "sur@test.org", Localidad = "Adrogue", Activo = true, FechaAlta = _fechaAlta, FotoPerfil = new byte[] { 4 } });

            //Equipo 1 fundador, un colaborador activo y uno bloqueado
            contexto.Usuarios.Add(Miembro(_fundadorId, "Laura", "laura@test.org", _patitasId, RolRefugio.Fundador));
            contexto.Usuarios.Add(Miembro(_colaboradorId, "Martin", "martin@test.org", _patitasId, RolRefugio.Colaborador));
            contexto.Usuarios.Add(Miembro(_colaboradorBloqueadoId, "Bruno", "bruno@test.org", _patitasId, RolRefugio.Colaborador, activo: false));
            //Equipo 2 para probar que no se mezclan refugios
            contexto.Usuarios.Add(Miembro(_fundadorSurId, "Carla", "carla@test.org", _surId, RolRefugio.Fundador));
            contexto.Usuarios.Add(Miembro(_colaboradorSurId, "Pedro", "pedro@test.org", _surId, RolRefugio.Colaborador));


            contexto.SaveChanges();
        }



        //Datos

        private static RefugioRequest NuevoRequest() => new()
        {
            Nombre = "  Huellitas Rescatadas ",
            Descripcion = " Rescatamos perros y gatos ",
            Email = "  HuellitasRescatadas@Example.COM ",
            Telefono = " 1144443333 ",
            Direccion = " Calle 123 ",
            Localidad = " Banfield ",
            MensajeRechazoAutomatico = " Gracias por postularte ",
            Contactos = new List<ContactoRequest>
            {
                new() { Tipo = TipoContacto.SitioWeb, Url = " patitas.org " }
            }
        };

        //nuevo colaborador
        private static ColaboradorRequest NuevoColaborador(string email) => new()
        {
            Email = email,
            Password = "inicial2026",
            Nombre = " Sofia ",
            Apellido = " Gomez "
        };

        //Miembro del refugio
        private static Usuario Miembro(Guid id, string nombre, string email, Guid refugioId, RolRefugio rolRefugio, bool activo = true) => new()
        {
            Id = id,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(Clave),
            Nombre = nombre,
            Apellido = "Test",
            Rol = RolUsuario.Refugio,
            RolRefugio = rolRefugio,
            RefugioId = refugioId,
            Activo = activo,
            FechaAlta = new DateTime(2026, 2, 1)
        };

        private static ArchivoAdjunto NuevaFoto(byte contenido) => new()
        {
            NombreArchivo = "logo.png",
            ContentType = "image/png",
            Contenido = new byte[] { contenido }
        };

        //Lectura con el contexto nuevo, para ver lo que quedo guardado
        private Refugio LeerRefugio(Guid id)
        {
            using var contexto = CrearContexto();

            return contexto.Refugios.Include(r => r.Contactos).Single(r => r.Id == id);
        }

        private Usuario LeerUsuario(Guid id)
        {
            using var contexto = CrearContexto();
            return contexto.Usuarios.Single(u => u.Id == id);
        }

        //********* PRUEBAS *************


        /// <summary>
        /// Obtiene el perfil del refugio con sus contactos (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ObtenerPerfil_DelRefugio_LoTraeConSusContactos()
        {
            using var contexto = CrearContexto();
            var servicio = new RefugioService(contexto);

            var perfil = await servicio.ObtenerPerfilAsync(_patitasId);

            Assert.NotNull(perfil);
            Assert.Equal("Patitas", perfil.Nombre);
            Assert.False(perfil.FotoPerfil);
            Assert.Equal(_fechaAlta, perfil.FechaAlta);
            Assert.Equal(2, perfil.Contactos.Count);
        }


        /// <summary>
        /// Obtener un perfil que no existe da null (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ObtenerPerfil_Inexistente_DaNull()
        {
            using var contexto = CrearContexto();
            var servicio = new RefugioService(contexto);

            Assert.Null(await servicio.ObtenerPerfilAsync(Guid.NewGuid()));
        }


        /// <summary>
        /// Actualizar un perfil con datos validos, los guarda con trim , email en minusculas y no cambia la fecha de alta ni estado (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ActualizarPerfil_ConDatosValidos_LoGuardaSinEspaciosYEmailEnMinusculas()
        {
            using var contexto = CrearContexto();
            var servicio = new RefugioService(contexto);

            var respuesta = await servicio.ActualizarPerfilAsync(NuevoRequest(), _patitasId);

            Assert.NotNull(respuesta);
            var guardado = LeerRefugio(_patitasId);
            Assert.Equal("Huellitas Rescatadas", guardado.Nombre);
            Assert.Equal("Rescatamos perros y gatos", guardado.Descripcion);
            Assert.Equal("huellitasrescatadas@example.com", guardado.Email);
            Assert.Equal("1144443333", guardado.Telefono);
            Assert.Equal("Calle 123", guardado.Direccion);
            Assert.Equal("Banfield", guardado.Localidad);
            Assert.Equal("Gracias por postularte", guardado.MensajeRechazoAutomatico);
            Assert.Equal(_fechaAlta, guardado.FechaAlta);
            Assert.True(guardado.Activo);
        }

        /// <summary>
        /// Actualizar o editar  contactos de un perfil de refugio, los sincronizan el que sigue se modifica (id igual), si no esta se borra si hay nuevo agrego (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ActualizarPerfil_Contactos_ModificaBorraYAgrega()
        {
            var request = NuevoRequest();
            request.Contactos.Add(new ContactoRequest { Tipo = TipoContacto.Instagram, Url = " instagram.com/patitas.nueva " });

            using var contexto = CrearContexto();
            var servicio = new RefugioService(contexto);

            await servicio.ActualizarPerfilAsync(request, _patitasId);

            var contactos = LeerRefugio(_patitasId).Contactos.OrderBy(c => c.Tipo).ToList();
            Assert.Equal(new[] { TipoContacto.SitioWeb, TipoContacto.Instagram }, contactos.Select(c => c.Tipo));
            Assert.Equal("patitas.org", contactos[0].Url);
            Assert.Equal(_instagramId, contactos[1].Id);
            Assert.Equal("instagram.com/patitas.nueva", contactos[1].Url);
        }



        /// <summary>
        /// Si se edita los contactos de un perfil y llegan en null  se borran todos (borrar contactos) (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ActualizarPerfil_SinContactos_LosBorraTodos()
        {
            var request = NuevoRequest();
            request.Contactos = null!;

            using var contexto = CrearContexto();
            var servicio = new RefugioService(contexto);

            Assert.NotNull(await servicio.ActualizarPerfilAsync(request, _patitasId));
            Assert.Empty(LeerRefugio(_patitasId).Contactos);
        }


        /// <summary>
        /// Actualziar o editar un perfil que no existe da null (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ActualizarPerfil_Inexistente_DaNull()
        {
            using var contexto = CrearContexto();
            var servicio = new RefugioService(contexto);

            Assert.Null(await servicio.ActualizarPerfilAsync(NuevoRequest(), Guid.NewGuid()));
        }


        /// <summary>
        /// No se peude editar un perfil de un refugio sin datos, da null y no cambia (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ActualizarPerfil_SinDatos_DaNullYNoCambia()
        {
            using var contexto = CrearContexto();
            var servicio = new RefugioService(contexto);

            Assert.Null(await servicio.ActualizarPerfilAsync(null!, _patitasId));
            Assert.Equal("Patitas", LeerRefugio(_patitasId).Nombre);
        }


        /// <summary>
        /// Cambiar la foto del refugio cambia la anterior (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CambiarFoto_DelRefugio_LaReemplaza()
        {
            using var contexto = CrearContexto();
            var servicio = new RefugioService(contexto);

            Assert.True(await servicio.CambiarFotoAsync(_surId, NuevaFoto(9)));
            var guardado = LeerRefugio(_surId);
            Assert.Equal(new byte[] { 9 }, guardado.FotoPerfil);
            Assert.Equal("image/png", guardado.FotoContentType);
        }



        /// <summary>
        /// No se puede cambiar la foto de un refugio que no existe, da false (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CambiarFoto_Inexistente_DaFalse()
        {
            using var contexto = CrearContexto();
            var servicio = new RefugioService(contexto);

            Assert.False(await servicio.CambiarFotoAsync(Guid.NewGuid(), NuevaFoto(9)));
        }


        /// <summary>
        /// Obtiene el logo, si se guarda sin tipo sale como jpeg/image (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ObtenerFoto_SinTipo_LaDevuelveComoJpeg()
        {
            using var contexto = CrearContexto();
            var servicio = new RefugioService(contexto);

            var foto = await servicio.ObtenerFotoAsync(_surId);

            Assert.NotNull(foto);
            Assert.Equal("image/jpeg", foto.ContentType);
            Assert.Equal(new byte[] { 4 }, foto.Contenido);
        }

        /// <summary>
        /// Obtener una foto que no existe o no tiene logo, da null (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ObtenerFoto_SinFoto_DaNull()
        {
            using var contexto = CrearContexto();
            var servicio = new RefugioService(contexto);

            Assert.Null(await servicio.ObtenerFotoAsync(_patitasId));
        }


        /// <summary>
        /// Un colaborador no puede bloquear a otro colaborador, solo el fundador puede. Da error
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CambiarEstadoColaborador_PorColaborador_DaError()
        {
            using var contexto = CrearContexto();
            var servicio = new RefugioService(contexto);

            var error = await servicio.CambiarEstadoColaboradorAsync(_colaboradorId, _patitasId, _colaboradorBloqueadoId, true);

            Assert.Equal("Solo el fundador puede administrar el equipo.", error);
            Assert.False(LeerUsuario(_colaboradorBloqueadoId).Activo);
        }

        /// <summary>
        /// Listar el equipo solo devuelve los de su propio refugio , con el fundador primero 
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ListarEquipo_DevuelveSoloLosDelRefugioConElFundadorPrimero()
        {
            using var contexto = CrearContexto();
            var servicio = new RefugioService(contexto);


            var lista = await servicio.ListarEquipoAsync(_patitasId);

            Assert.Equal(3, lista.Count);
            Assert.Equal(RolRefugio.Fundador, lista[0].RolRefugio);
            Assert.DoesNotContain(lista, m => m.Id == _colaboradorSurId);
        }


        /// <summary>
        /// Agrego colaborador por medio del fundador y lo creo OK
        /// </summary>
        /// <returns></returns>

        [Fact]
        public async Task AgregarColaborador_PorFundador_LoCrea()
        {
            using var contexto = CrearContexto();
            var servicio = new RefugioService(contexto);

            

            var result = await servicio.AgregarColaboradorAsync(_fundadorId,_patitasId,NuevoColaborador("  NUEVO@test.org "));

            Assert.Null(result.Error);
            Assert.NotNull(result.Colaborador);

            //leer lo que se guardo
            var guardado = LeerUsuario(result.Colaborador.Id);
            Assert.Equal("nuevo@test.org", guardado.Email);
            Assert.Equal(RolRefugio.Colaborador, guardado.RolRefugio);
            Assert.Equal(_patitasId, guardado.RefugioId);
            Assert.True(BCrypt.Net.BCrypt.Verify("inicial2026", guardado.PasswordHash));
        }


        /// <summary>
        /// Un colaborador no puede agregar a otro colaborador, da error
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AgregarColaborador_PorColaborador_DaError()
        {
            using var contexto = CrearContexto();
            var servicio = new RefugioService(contexto);



            var result = await servicio.AgregarColaboradorAsync(_colaboradorId, _patitasId, NuevoColaborador("  NUEVO@test.org "));

            Assert.Equal("Solo el fundador puede administrar el equipo.", result.Error);
            
        }

        /// <summary>
        /// Agregar un colaborador con un mail ya en uso da error
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AgregarColaborador_EmailEnUso_DaError()
        {
            using var contexto = CrearContexto();
            var servicio = new RefugioService(contexto);



            var result = await servicio.AgregarColaboradorAsync(_fundadorId, _patitasId, NuevoColaborador("  MARTIN@test.org "));

            Assert.Equal("Ya existe una cuenta registrada con este mail.", result.Error);

        }


        /// <summary>
        /// Agregar un colaborador siendo fundador de otro refugio  da Error
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AgregarColaborador_FundadorDeOtroRefugio_DaError()
        {
            using var contexto = CrearContexto();
            var servicio = new RefugioService(contexto);



            var result = await servicio.AgregarColaboradorAsync(_fundadorSurId, _patitasId, NuevoColaborador("  NUEVO@test.org "));

            Assert.Equal("Solo el fundador puede administrar el equipo.", result.Error);

        }


        /// <summary>
        /// Bloquear un colaborador lo desactiva
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CambiarEstadoColaborador_Bloquear_LoDesactiva()
        {
            using var contexto = CrearContexto();
            var servicio = new RefugioService(contexto);



            var result = await servicio.CambiarEstadoColaboradorAsync(_fundadorId, _patitasId,_colaboradorId, false);

            Assert.Null(result);


            var guardado = LeerUsuario(_colaboradorId);
            Assert.False(guardado.Activo);
        }

        /// <summary>
        /// Desbloquear un colaborador bloqueado lo activa 
        /// </summary>
        /// <returns></returns>

        [Fact]
        public async Task CambiarEstadoColaborador_Desbloquear_LoActiva()
        {
            using var contexto = CrearContexto();
            var servicio = new RefugioService(contexto);



            var result = await servicio.CambiarEstadoColaboradorAsync(_fundadorId, _patitasId, _colaboradorBloqueadoId, true);

            Assert.Null(result);


            var guardado = LeerUsuario(_colaboradorId);
            Assert.True(guardado.Activo);
        }


        /// <summary>
        /// No se puede bloquear al fundador, da error. Solo puede el Admin
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CambiarEstadoColaborador_AlFundador_DaError()
        {
            using var contexto = CrearContexto();
            var servicio = new RefugioService(contexto);



            var result = await servicio.CambiarEstadoColaboradorAsync(_fundadorId, _patitasId, _fundadorId, false);

            Assert.Equal("No encontramos ese colaborador en tu refugio.", result);


            var guardado = LeerUsuario(_fundadorId);
            Assert.True(guardado.Activo);
        }


        /// <summary>
        /// No se puede cambiar el estado a un colaborador que no pertenezca al refugio, da error
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CambiarEstadoColaborador_DeOtroRefugio_DaError()
        {
            using var contexto = CrearContexto();
            var servicio = new RefugioService(contexto);



            var result = await servicio.CambiarEstadoColaboradorAsync(_fundadorId, _patitasId, _colaboradorSurId, false);

            Assert.Equal("No encontramos ese colaborador en tu refugio.", result);


            var guardado = LeerUsuario(_colaboradorSurId);
            Assert.True(guardado.Activo);
        }

        /// <summary>
        /// Blanquear la contraseña a un colaborador funciona y cambia la clave, solo fundador
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task BlanquearPasswordColaborador_CambiaLaClave()
        {
            using var contexto = CrearContexto();
            var servicio = new RefugioService(contexto);



            var result = await servicio.BlanquearPasswordColaboradorAsync(_fundadorId, _patitasId, _colaboradorId, "temporal2026");

            Assert.Null(result);

            var guardado = LeerUsuario(_colaboradorId);
            Assert.True(BCrypt.Net.BCrypt.Verify("temporal2026", guardado.PasswordHash));            
           
        }


        /// <summary>
        /// Un colaborador no puede blanquear a otro colaborador, solo el fundador 
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task BlanquearPasswordColaborador_PorColaborador_DaError()
        {
            using var contexto = CrearContexto();
            var servicio = new RefugioService(contexto);



            var result = await servicio.BlanquearPasswordColaboradorAsync(_colaboradorId, _patitasId, _colaboradorBloqueadoId, "temporal2026");

            Assert.Equal("Solo el fundador puede administrar el equipo.", result);

            var guardado = LeerUsuario(_colaboradorId);
            Assert.True(BCrypt.Net.BCrypt.Verify("claveprueba2026", guardado.PasswordHash));

        }
    }
}
