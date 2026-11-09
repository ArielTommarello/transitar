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
    public class AdminServiceTests : BaseEnMemoria
    {

        //guids
        private readonly Guid _patitasId = Guid.NewGuid();
        private readonly Guid _surId = Guid.NewGuid();
        private readonly Guid _amigosId = Guid.NewGuid();
        private readonly Guid _anaId = Guid.NewGuid();
        private readonly Guid _lauraId = Guid.NewGuid();
        private readonly Guid _carlosId = Guid.NewGuid();
        private readonly Guid _adminId = Guid.NewGuid();
        private readonly DateTime _fechaAlta = new DateTime(2026, 3, 15);


        //Creo el nuevo contexto
        public AdminServiceTests()
        {
            //Usuarios: Ana - activa, Laura (Patitas), Carlos - bloqueado y el admin
            //Refugios: Patitas y Amigos - activos, Refugio del Sur dado de baja
            using var contexto = CrearContexto();

            contexto.Refugios.Add(NuevoRefugio(_patitasId, "Patitas", true));
            contexto.Refugios.Add(NuevoRefugio(_surId, "Refugio del Sur", false));
            contexto.Refugios.Add(NuevoRefugio(_amigosId, "Amigos", true));
            contexto.Usuarios.Add(NuevoUsuario(_anaId, "Ana", "Perez", RolUsuario.Usuario, true));
            contexto.Usuarios.Add(NuevoUsuario(_lauraId, "Laura", "Perez", RolUsuario.Refugio, true, _patitasId));
            contexto.Usuarios.Add(NuevoUsuario(_carlosId, "Carlos", "Diaz", RolUsuario.Usuario, false));
            contexto.Usuarios.Add(NuevoUsuario(_adminId, "Admin", "Sistema", RolUsuario.Admin, true));
            contexto.SaveChanges();
        }

        //Datos

        private Refugio NuevoRefugio(Guid id, string nombre, bool activo) => new()
        {
            Id = id,
            Nombre = nombre,
            Email = $"{id}@test.org",
            Localidad = "Lomas",
            Activo = activo,
            FechaAlta = _fechaAlta
        };

        private Usuario NuevoUsuario(Guid id, string nombre, string apellido, RolUsuario rol, bool activo, Guid? refugioId = null) => new()
        {
            Id = id,
            Email = $"{nombre.ToLower()}@test.org",
            PasswordHash = "hash-de-prueba",
            Nombre = nombre,
            Apellido = apellido,
            Rol = rol,
            RefugioId = refugioId,
            Activo = activo,
            FechaAlta = _fechaAlta
        };

        //Lectura con el contexto nuevo, para ver lo que quedo guardado
        private Usuario LeerUsuario(Guid id)
        {
            using var contexto = CrearContexto();

            return contexto.Usuarios.Single(u => u.Id == id);
        }

        private Refugio LeerRefugio(Guid id)
        {
            using var contexto = CrearContexto();

            return contexto.Refugios.Single(r => r.Id == id);
        }


        //********* PRUBEAS *************
        
        /// <summary>
        /// Listar usuarios sin filtro trae a todos, con orden por ap y nombre (ADMIN)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ListarUsuarios_SinFiltros_TraeTodosOrdenadosPorApellidoYNombre()
        {
            using var contexto = CrearContexto();
            var servicio = new AdminService(contexto);

            var usuarios = await servicio.ListarUsuariosAsync(null, null);

            Assert.Equal(new[] { _carlosId, _anaId, _lauraId, _adminId }, usuarios.Select(u => u.Id));
        }


        /// <summary>
        /// Filtro por rol, trae solo ese rol con el refugio y sus datos (ADMIN)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ListarUsuarios_PorRol_TraeSoloEseRol()
        {
            using var contexto = CrearContexto();
            var servicio = new AdminService(contexto);

            var usuarios = await servicio.ListarUsuariosAsync(RolUsuario.Refugio, null);

            var laura = Assert.Single(usuarios);
            Assert.Equal(_lauraId, laura.Id);
            Assert.Equal("laura@test.org", laura.Email);
            Assert.Equal(_patitasId, laura.RefugioId);
            Assert.True(laura.Activo);
            Assert.Equal(_fechaAlta, laura.FechaAlta);
        }


        /// <summary>
        /// Filtro por inactivos, trae solo bloqueados (ADMIN)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ListarUsuarios_Inactivos_TraeSoloLosBloqueados()
        {
            using var contexto = CrearContexto();
            var servicio = new AdminService(contexto);

            var usuarios = await servicio.ListarUsuariosAsync(null, false);

            Assert.Equal(_carlosId, Assert.Single(usuarios).Id);
        }


        /// <summary>
        /// Filtro por rol y activo, trae los postulantes activos (ADMIN)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ListarUsuarios_PorRolYActivo_CombinaLosFiltros()
        {
            using var contexto = CrearContexto();
            var servicio = new AdminService(contexto);

            var usuarios = await servicio.ListarUsuariosAsync(RolUsuario.Usuario, true);

            Assert.Equal(_anaId, Assert.Single(usuarios).Id);
        }

        /// <summary>
        /// Cambiar de estado a un usuario, bloquarlo lo deja inactivo (ADMIN)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CambiarEstadoUsuario_Bloquear_LoDesactiva()
        {
            using var contexto = CrearContexto();
            var servicio = new AdminService(contexto);

            Assert.Null(await servicio.CambiarEstadoUsuarioAsync(_anaId, false));
            Assert.False(LeerUsuario(_anaId).Activo);
        }



        /// <summary>
        /// Cambiar de estado a un usuario, desbloquarlo lo deja activo (ADMIN)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CambiarEstadoUsuario_Desbloquear_LoActiva()
        {
            using var contexto = CrearContexto();
            var servicio = new AdminService(contexto);

            Assert.Null(await servicio.CambiarEstadoUsuarioAsync(_carlosId, true));
            Assert.True(LeerUsuario(_carlosId).Activo);
        }


        /// <summary>
        /// cambiar de estado a un admin, da error y no cambia. No se puede cambiar de estado a un admin (ADMIN)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CambiarEstadoUsuario_Admin_DaErrorYNoCambia()
        {
            using var contexto = CrearContexto();
            var servicio = new AdminService(contexto);

            Assert.NotNull(await servicio.CambiarEstadoUsuarioAsync(_adminId, false));
            Assert.True(LeerUsuario(_adminId).Activo);
        }


        /// <summary>
        /// cambiar de estado a un usuario que no existe da error (ADMIN)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CambiarEstadoUsuario_Inexistente_DaError()
        {
            using var contexto = CrearContexto();
            var servicio = new AdminService(contexto);

            Assert.NotNull(await servicio.CambiarEstadoUsuarioAsync(Guid.NewGuid(), false));
        }



        /// <summary>
        /// Sin filtro trae todos los refugios ordenados por nombre y ap (ADMIN)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ListarRefugios_SinFiltro_TraeTodosOrdenadosPorNombre()
        {
            using var contexto = CrearContexto();
            var servicio = new AdminService(contexto);

            var refugios = await servicio.ListarRefugiosAsync(null);

            Assert.Equal(new[] { "Amigos", "Patitas", "Refugio del Sur" }, refugios.Select(r => r.Nombre));
            Assert.Equal("Lomas", refugios[0].Localidad);
            Assert.Equal(_fechaAlta, refugios[0].FechaAlta);
        }



        /// <summary>
        /// Filtrar refugios por inactivo, trae solo los dados de baja (ADMIN)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ListarRefugios_Inactivos_TraeSoloLosDadosDeBaja()
        {
            using var contexto = CrearContexto();
            var servicio = new AdminService(contexto);

            var refugios = await servicio.ListarRefugiosAsync(false);

            Assert.Equal(_surId, Assert.Single(refugios).Id);
        }


        /// <summary>
        /// Cambiar de estado a un refugio, darlo de baja, lo desactiva sin tocar a los usuarios que tiene dentro, solo pueden loguear (ADMIN)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CambiarEstadoRefugio_Desactivar_LoDesactivaSinTocarASuGente()
        {
            using var contexto = CrearContexto();
            var servicio = new AdminService(contexto);

            Assert.Null(await servicio.CambiarEstadoRefugioAsync(_patitasId, false));
            Assert.False(LeerRefugio(_patitasId).Activo);
            Assert.True(LeerUsuario(_lauraId).Activo);
        }

        /// <summary>
        /// CAmbiar de estado a un refugio que fue dado de baja a activo, lo vuelve a activar (ADMIN)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CambiarEstadoRefugio_Activar_LoActiva()
        {
            using var contexto = CrearContexto();
            var servicio = new AdminService(contexto);

            Assert.Null(await servicio.CambiarEstadoRefugioAsync(_surId, true));
            Assert.True(LeerRefugio(_surId).Activo);
        }



        /// <summary>
        /// Camibar de estado a un refugio que no existe da error (ADMIN)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CambiarEstadoRefugio_Inexistente_DaError()
        {
            using var contexto = CrearContexto();
            var servicio = new AdminService(contexto);

            Assert.NotNull(await servicio.CambiarEstadoRefugioAsync(Guid.NewGuid(), false));
        }
    }
}
