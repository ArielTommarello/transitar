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
    public class RefugioServiceTests : BaseEnMemoria
    {
        //guids
        private readonly Guid _patitasId = Guid.NewGuid();
        private readonly Guid _surId = Guid.NewGuid();
        private readonly Guid _instagramId = Guid.NewGuid();
        private readonly DateTime _fechaAlta = new DateTime(2026, 2, 1);



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
            contexto.SaveChanges();
        }



        //Datos

        private static RefugioRequest NuevoRequest() => new()
        {
            Nombre = "  Patitas al Rescate ",
            Descripcion = " Rescatamos perros y gatos ",
            Email = "  Contacto@PatitasAlRescate.ORG ",
            Telefono = " 1144443333 ",
            Direccion = " Calle 123 ",
            Localidad = " Banfield ",
            MensajeRechazoAutomatico = " Gracias por postularte ",
            Contactos = new List<ContactoRequest>
            {
                new() { Tipo = TipoContacto.SitioWeb, Url = " patitas.org " }
            }
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
            Assert.Equal("Patitas al Rescate", guardado.Nombre);
            Assert.Equal("Rescatamos perros y gatos", guardado.Descripcion);
            Assert.Equal("contacto@patitasalrescate.org", guardado.Email);
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

    }
}
