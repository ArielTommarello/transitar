using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransitAR.Api.Services;
using TransitAR.Structures;
using TransitAR.Tests.Soporte;

namespace TransitAR.Tests.Services
{
    public class PerfilServiceTests : BaseEnMemoria
    {

        //guids
        private readonly Guid _anaId = Guid.NewGuid();
        private readonly Guid _carlosId = Guid.NewGuid();
        private readonly Guid _perfilAnaId = Guid.NewGuid();
        private readonly Guid _instagramAnaId = Guid.NewGuid();
        private readonly DateTime _fechaCompletado = new DateTime(2026, 5, 20);

        //Creo el contexto nuevo
        public PerfilServiceTests()
        {

            //Ana con perfil (Tiene patio cerrado, otra masctoa y dos contactos).
            //Carlos nada
            using var contexto = CrearContexto();

            contexto.Usuarios.Add(NuevoUsuario(_anaId, "Ana"));
            contexto.Usuarios.Add(NuevoUsuario(_carlosId, "Carlos"));
            contexto.PerfilPostulantes.Add(new PerfilPostulante
            {
                Id = _perfilAnaId,
                UsuarioId = _anaId,
                Seleccion = SeleccionUsuario.Adoptante,
                TipoVivienda = "Casa",
                TienePatio = true,
                PatioCerrado = true,
                TieneOtrasMascotas = true,
                DetalleOtrasMascotas = "Un gato",
                MotivoPostulacion = "Quiero adoptar",
                FechaCompletado = _fechaCompletado,
                Contactos = new List<ContactoPostulante>
                {
                    new() { Id = _instagramAnaId, PerfilPostulanteId = _perfilAnaId, Tipo = TipoContacto.Instagram, Url = "instagram.com/ana" },
                    new() { Id = Guid.NewGuid(), PerfilPostulanteId = _perfilAnaId, Tipo = TipoContacto.Facebook, Url = "facebook.com/ana" }
                }
            });
            contexto.SaveChanges();
        }

        //Datos

        private static Usuario NuevoUsuario(Guid id, string nombre) => new()
        {
            Id = id,
            Email = $"{id}@test.org",
            PasswordHash = "hash-de-prueba",
            Nombre = nombre,
            Apellido = "Perez",
            Rol = RolUsuario.Usuario,
            Activo = true
        };

        private static PerfilRequest NuevoRequest() => new()
        {
            Seleccion = SeleccionUsuario.Ambos,
            TipoVivienda = "  Departamento ",
            TienePatio = true,
            PatioCerrado = true,
            TieneOtrasMascotas = true,
            DetalleOtrasMascotas = " Dos perros ",
            HorasSoloPorDia = 4,
            DisponibilidadHorario = DisponibilidadHorario.Tarde,
            CercaniaVeterinaria = " A 5 cuadras ",
            ExperienciaPrevia = " Tuve perros ",
            MotivoPostulacion = "  Quiero ayudar  ",
            Contactos = new List<ContactoRequest>
            {
                new() { Tipo = TipoContacto.WhatsApp, Url = " 1155556666 " }
            }
        };

        //Usuario diferente con foto
        private Guid CargarUsuarioConFoto()
        {
            using var contexto = CrearContexto();

            var id = Guid.NewGuid();
            contexto.Usuarios.Add(NuevoUsuario(id, "Sofia"));
            contexto.PerfilPostulantes.Add(new PerfilPostulante { Id = Guid.NewGuid(), UsuarioId = id, Seleccion = SeleccionUsuario.Transito, FotoPerfil = new byte[] { 5 }, FotoContentType = "image/png" });
            contexto.SaveChanges();

            return id;
        }

        private static ArchivoAdjunto NuevaFoto(byte contenido) => new()
        {
            NombreArchivo = "foto.png",
            ContentType = "image/png",
            Contenido = new byte[] { contenido }
        };

        //lectura en nuevo contexto , para ver lo que quedo guardado
        private PerfilPostulante? LeerPerfil(Guid usuarioId)
        {
            using var contexto = CrearContexto();

            return contexto.PerfilPostulantes.Include(p => p.Contactos).SingleOrDefault(p => p.UsuarioId == usuarioId);
        }


        //********* PRUEBAS *************


        /// <summary>
        /// Obtiene el perfil del usuario con sus contactos (POSTULANTE)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ObtenerPerfil_DelUsuario_LoTraeConSusContactos()
        {
            using var contexto = CrearContexto();
            var servicio = new PerfilService(contexto);

            var perfil = await servicio.ObtenerPerfilAsync(_anaId);

            Assert.NotNull(perfil);
            Assert.Equal(_perfilAnaId, perfil.Id);
            Assert.Equal("Casa", perfil.TipoVivienda);
            Assert.False(perfil.FotoPerfil);
            Assert.Equal(2, perfil.Contactos.Count);
        }

        /// <summary>
        /// obtener un perfil sin completar da null (en la web nos avisa de compeltar el perfil y redireccion) (POSTULANTE)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ObtenerPerfil_SinPerfil_DaNull()
        {
            using var contexto = CrearContexto();
            var servicio = new PerfilService(contexto);

            Assert.Null(await servicio.ObtenerPerfilAsync(_carlosId));
        }

        /// <summary>
        /// Crear perfil con datos validos, guarda la info con trim y con los contactos ´la fecha que lo completo (POSTULANTE)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CrearPerfil_ConDatosValidos_LoGuardaSinEspacios()
        {
            using var contexto = CrearContexto();
            var servicio = new PerfilService(contexto);

            var respuesta = await servicio.CrearPerfilAsync(NuevoRequest(), _carlosId);

            Assert.NotNull(respuesta);
            var guardado = LeerPerfil(_carlosId);
            Assert.NotNull(guardado);
            Assert.Equal(SeleccionUsuario.Ambos, guardado.Seleccion);
            Assert.Equal("Departamento", guardado.TipoVivienda);
            Assert.True(guardado.PatioCerrado);
            Assert.Equal("Dos perros", guardado.DetalleOtrasMascotas);
            Assert.Equal("A 5 cuadras", guardado.CercaniaVeterinaria);
            Assert.Equal("Tuve perros", guardado.ExperienciaPrevia);
            Assert.Equal("Quiero ayudar", guardado.MotivoPostulacion);
            Assert.Equal(4, guardado.HorasSoloPorDia);
            Assert.True(guardado.FechaCompletado > DateTime.UtcNow.AddMinutes(-1));
            var contacto = Assert.Single(guardado.Contactos);
            Assert.Equal(TipoContacto.WhatsApp, contacto.Tipo);
            Assert.Equal("1155556666", contacto.Url);
        }


        /// <summary>
        /// Crear perfil sin patio , no guarda otras opciones (patio cerrado) (POSTULANTE)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CrearPerfil_SinPatio_NoGuardaPatioCerrado()
        {
            var request = NuevoRequest();
            request.TienePatio = false;

            using var contexto = CrearContexto();
            var servicio = new PerfilService(contexto);

            await servicio.CrearPerfilAsync(request, _carlosId);

            Assert.False(LeerPerfil(_carlosId)!.PatioCerrado);
        }

        /// <summary>
        /// Crear perfil sin otras mascotas, no guarda ningun detalle , queda null (POSTULANTE)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CrearPerfil_SinOtrasMascotas_NoGuardaElDetalle()
        {
            var request = NuevoRequest();
            request.TieneOtrasMascotas = false;

            using var contexto = CrearContexto();
            var servicio = new PerfilService(contexto);

            await servicio.CrearPerfilAsync(request, _carlosId);

            Assert.Null(LeerPerfil(_carlosId)!.DetalleOtrasMascotas);
        }

        /// <summary>
        /// Crear perfil sin contactos, crea el perfil igualmente, y los pone en null (POSTULANTE)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CrearPerfil_SinContactos_LoCreaSinContactos()
        {
            var request = NuevoRequest();
            request.Contactos = null!;

            using var contexto = CrearContexto();
            var servicio = new PerfilService(contexto);

            Assert.NotNull(await servicio.CrearPerfilAsync(request, _carlosId));
            Assert.Empty(LeerPerfil(_carlosId)!.Contactos);
        }


        /// <summary>
        /// Crear perfil sin datos da null y no se crea (POSTULANTE)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CrearPerfil_SinDatos_DaNull()
        {
            using var contexto = CrearContexto();
            var servicio = new PerfilService(contexto);

            Assert.Null(await servicio.CrearPerfilAsync(null!, _carlosId));
        }


        /// <summary>
        /// Crear perfil sin usuario id, da null y no lo crea (POSTULANTE)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CrearPerfil_SinUsuario_DaNull()
        {
            using var contexto = CrearContexto();
            var servicio = new PerfilService(contexto);

            Assert.Null(await servicio.CrearPerfilAsync(NuevoRequest(), Guid.Empty));
        }

        /// <summary>
        /// Un usuario tiene un solo perfil, si ya tiene solo puede editar no crear otro da null y no lo crea (POSTULANTE)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CrearPerfil_YaTieneUno_DaNullYNoCrea()
        {
            using var contexto = CrearContexto();
            var servicio = new PerfilService(contexto);

            Assert.Null(await servicio.CrearPerfilAsync(NuevoRequest(), _anaId));
            Assert.Equal("Casa", LeerPerfil(_anaId)!.TipoVivienda);
        }


        /// <summary>
        /// Actualizar o editar perfil cambia los datos , pero no la fecha de completado (POSTULANTE)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ActualizarPerfil_ConDatosValidos_CambiaLosDatosYNoLaFecha()
        {
            using var contexto = CrearContexto();
            var servicio = new PerfilService(contexto);

            var respuesta = await servicio.ActualizarPerfilAsync(NuevoRequest(), _anaId);

            Assert.NotNull(respuesta);
            var guardado = LeerPerfil(_anaId)!;
            Assert.Equal(_perfilAnaId, guardado.Id);
            Assert.Equal("Departamento", guardado.TipoVivienda);
            Assert.Equal("Quiero ayudar", guardado.MotivoPostulacion);
            Assert.Equal(DisponibilidadHorario.Tarde, guardado.DisponibilidadHorario);
            Assert.Equal(_fechaCompletado, guardado.FechaCompletado);
        }


        /// <summary>
        /// Actualizar o editar perfil sin patio o otras masctoas, limpia el patio cerrado y los detalles (POSTULANTE)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ActualizarPerfil_SinPatioNiMascotas_LimpiaLosDetalles()
        {
            var request = NuevoRequest();
            request.TienePatio = false;
            request.TieneOtrasMascotas = false;

            using var contexto = CrearContexto();
            var servicio = new PerfilService(contexto);

            await servicio.ActualizarPerfilAsync(request, _anaId);

            var guardado = LeerPerfil(_anaId)!;
            Assert.False(guardado.PatioCerrado);
            Assert.Null(guardado.DetalleOtrasMascotas);
        }


        /// <summary>
        ///  Actualizar un perfil y sus contactos , los actualiza y sincroniza, el que esta sigue y el que no vino se borra, si hay nuevo se agrega (POSTULANTE)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ActualizarPerfil_Contactos_ModificaBorraYAgrega()
        {
            var request = NuevoRequest();
            request.Contactos.Add(new ContactoRequest { Tipo = TipoContacto.Instagram, Url = " instagram.com/ana.nueva " });

            using var contexto = CrearContexto();
            var servicio = new PerfilService(contexto);

            await servicio.ActualizarPerfilAsync(request, _anaId);

            var contactos = LeerPerfil(_anaId)!.Contactos.OrderBy(c => c.Tipo).ToList();
            Assert.Equal(new[] { TipoContacto.Instagram, TipoContacto.WhatsApp }, contactos.Select(c => c.Tipo));
            Assert.Equal(_instagramAnaId, contactos[0].Id);
            Assert.Equal("instagram.com/ana.nueva", contactos[0].Url);
            Assert.Equal("1155556666", contactos[1].Url);
        }


        /// <summary>
        /// Si se actualiza el perfil y los contractos llegan en null o borrados, los borra a todos (POSTULANTE)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ActualizarPerfil_SinContactos_LosBorraTodos()
        {
            var request = NuevoRequest();
            request.Contactos = null!;

            using var contexto = CrearContexto();
            var servicio = new PerfilService(contexto);

            Assert.NotNull(await servicio.ActualizarPerfilAsync(request, _anaId));
            Assert.Empty(LeerPerfil(_anaId)!.Contactos);
        }


        /// <summary>
        /// Actualizar perfil que no existe o no esta, da null (POSTULANTE)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ActualizarPerfil_SinPerfil_DaNull()
        {
            using var contexto = CrearContexto();
            var servicio = new PerfilService(contexto);

            Assert.Null(await servicio.ActualizarPerfilAsync(NuevoRequest(), _carlosId));
            Assert.Null(LeerPerfil(_carlosId));
        }

        /// <summary>
        /// Actualizar perfil sin datos, no se edita y da null (POSTULANTE)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ActualizarPerfil_SinDatos_DaNull()
        {
            using var contexto = CrearContexto();
            var servicio = new PerfilService(contexto);

            Assert.Null(await servicio.ActualizarPerfilAsync(null!, _anaId));
        }



        /// <summary>
        /// Actualizar el perfil con una foto nueva, reemplaza la anterior (POSTULANTE)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CambiarFoto_ConPerfil_LaReemplaza()
        {
            using (var primero = CrearContexto())
                await new PerfilService(primero).CambiarFotoAsync(_anaId, NuevaFoto(1));

            using var contexto = CrearContexto();
            var servicio = new PerfilService(contexto);

            Assert.True(await servicio.CambiarFotoAsync(_anaId, NuevaFoto(2)));
            var guardado = LeerPerfil(_anaId)!;
            Assert.Equal(new byte[] { 2 }, guardado.FotoPerfil);
            Assert.Equal("image/png", guardado.FotoContentType);
        }


        /// <summary>
        /// Sin perfil no se puede cambiar la foto, da false (POSTULANTE)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CambiarFoto_SinPerfil_DaFalse()
        {
            using var contexto = CrearContexto();
            var servicio = new PerfilService(contexto);

            Assert.False(await servicio.CambiarFotoAsync(_carlosId, NuevaFoto(1)));
        }


        /// <summary>
        /// Devuelve la foto del usuario con su tipo (POSTULANTE)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ObtenerFoto_ConFoto_LaDevuelve()
        {
            var sofia = CargarUsuarioConFoto();

            using var contexto = CrearContexto();
            var servicio = new PerfilService(contexto);

            var foto = await servicio.ObtenerFotoAsync(sofia);

            Assert.NotNull(foto);
            Assert.Equal("image/png", foto.ContentType);
            Assert.Equal(new byte[] { 5 }, foto.Contenido);
        }


        /// <summary>
        /// Si se quiere obtnere una foto que no existe o no hay foto da null (POSTULANTE)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ObtenerFoto_SinFoto_DaNull()
        {
            CargarUsuarioConFoto();

            using var contexto = CrearContexto();
            var servicio = new PerfilService(contexto);

            Assert.Null(await servicio.ObtenerFotoAsync(_anaId));
        }
    }
}
