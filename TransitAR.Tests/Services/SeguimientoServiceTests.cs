using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransitAR.Api.Services;
using TransitAR.Structures;
using TransitAR.Structures.Requests;
using TransitAR.Tests.Soporte;
using TransitAR.Api.Extensions;

namespace TransitAR.Tests.Services
{
    public class SeguimientoServiceTests : BaseEnMemoria
    {
        //guids
        private readonly Guid _refugioId = Guid.NewGuid();
        private readonly Guid _especieId = Guid.NewGuid();
        private readonly Guid _condicionId = Guid.NewGuid();
        private readonly Guid _mascotaId = Guid.NewGuid();
        private readonly Guid _anaId = Guid.NewGuid();
        private readonly Guid _tenenciaId;


        //Creo el nuevo contexto
        public SeguimientoServiceTests()
        {
            using (var contexto = CrearContexto())
            {
                contexto.Refugios.Add(new Refugio { Id = _refugioId, Nombre = "Patitas", Email = "patitas@test.org", Localidad = "Lomas", Activo = true });
                contexto.Especies.Add(new Especie { Id = _especieId, Nombre = "Especie de prueba" });
                contexto.Condiciones.Add(new Condicion { Id = _condicionId, Nombre = "Condicion de prueba" });
                contexto.Mascotas.Add(new Mascota { Id = _mascotaId, Nombre = "Michi", RefugioId = _refugioId, EspecieId = _especieId, CondicionId = _condicionId });
                contexto.Usuarios.Add(NuevoUsuario(_anaId, "ana@test.org", "Ana"));
                contexto.SaveChanges();
            }
            //transito en curso de michi
            _tenenciaId = CargarTenencia();
        }

        //Datos
        private static Usuario NuevoUsuario(Guid id, string email, string nombre) => new()
        {
            Id = id,
            Email = email,
            PasswordHash = "passwordprueba",
            Nombre = nombre,
            Apellido = "Perez",
            Rol = RolUsuario.Usuario,
            Activo = true
        };

        private Guid CargarUsuario(string email, string nombre)
        {
            using var contexto = CrearContexto();

            var id = Guid.NewGuid();
            contexto.Usuarios.Add(NuevoUsuario(id, email, nombre));
            contexto.SaveChanges();

            return id;
        }

        private Guid CargarMascota(string nombre)
        {
            using var contexto = CrearContexto();

            var id = Guid.NewGuid();
            contexto.Mascotas.Add(new Mascota { Id = id, Nombre = nombre, RefugioId = _refugioId, EspecieId = _especieId, CondicionId = _condicionId });
            contexto.SaveChanges();

            return id;
        }
        //Cargo la masctoa de otro refugio para que no toque ni pruebe
        private Guid CargarMascotaDeOtroRefugio()
        {
            using var contexto = CrearContexto();

            var refugioId = Guid.NewGuid();
            var mascotaId = Guid.NewGuid();
            contexto.Refugios.Add(new Refugio { Id = refugioId, Nombre = "Refugio del Sur", Email = "sur@test.org", Localidad = "Adrogue", Activo = true });
            contexto.Mascotas.Add(new Mascota { Id = mascotaId, Nombre = "Toto", RefugioId = refugioId, EspecieId = _especieId, CondicionId = _condicionId });
            contexto.SaveChanges();

            return mascotaId;
        }

        private Guid CargarTenencia(Guid? mascotaId = null, Guid? usuarioId = null, DateTime? fechaFinReal = null)
        {
            using var contexto = CrearContexto();

            var publicacionId = Guid.NewGuid();
            var postulacionId = Guid.NewGuid();
            var id = Guid.NewGuid();
            contexto.Publicaciones.Add(new Publicacion
            {
                Id = publicacionId,
                MascotaId = mascotaId ?? _mascotaId,
                Titulo = "Busca hogar",
                Descripcion = "Descripcion de prueba",
                Ubicacion = "Lomas",
                Tipo = TipoPublicacion.Transito,
                Estado = EstadoPublicacion.Cerrada
            });
            contexto.Postulaciones.Add(new Postulacion { Id = postulacionId, PublicacionId = publicacionId, UsuarioId = usuarioId ?? _anaId, Estado = EstadoPostulacion.Aceptada });
            contexto.Tenencias.Add(new Tenencia
            {
                Id = id,
                PostulacionId = postulacionId,
                MascotaId = mascotaId ?? _mascotaId,
                Modalidad = TipoPublicacion.Transito,
                FechaInicio = DateTime.UtcNow,
                FechaFinReal = fechaFinReal
            });
            contexto.SaveChanges();

            return id;
        }

        private Guid CargarSeguimiento(Guid? tenenciaId = null, EstadoSeguimiento estado = EstadoSeguimiento.Pendiente, DateTime? fechaProgramada = null)
        {
            using var contexto = CrearContexto();

            var id = Guid.NewGuid();
            contexto.Seguimientos.Add(new Seguimiento
            {
                Id = id,
                TenenciaId = tenenciaId ?? _tenenciaId,
                FechaProgramada = fechaProgramada ?? DateTime.UtcNow.Date.AddDays(7),
                Estado = estado
            });
            contexto.SaveChanges();

            return id;
        }

        //Lectura con el contexto nuevo, para ver lo que quedo guardado
        private Seguimiento LeerSeguimiento(Guid id)
        {
            using var contexto = CrearContexto();

            return contexto.Seguimientos.Single(s => s.Id == id);
        }

        private List<Seguimiento> LeerSeguimientos()
        {
            using var contexto = CrearContexto();

            return contexto.Seguimientos.ToList();
        }



        //********* PRUEBAS *************
        /// <summary>
        /// La agenda del refugio teien que traer sololso contorles de sus masctoas , con nombre de mascota, persona y refugio (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Listar_ComoRefugio_TraeSoloLosDeSusMascotasConSusDatos()
        {
            var propioId = CargarSeguimiento();
            CargarSeguimiento(CargarTenencia(mascotaId: CargarMascotaDeOtroRefugio()));

            using var contexto = CrearContexto();
            var servicio = new SeguimientoService(contexto);

            var controles = await servicio.ListarAsync(Guid.Empty, _refugioId, null, false, null);

            var control = Assert.Single(controles);
            Assert.Equal(propioId, control.Id);
            Assert.Equal("Michi", control.MascotaNombre);
            Assert.Equal("Ana Perez", control.PersonaNombre);
            Assert.Equal("Patitas", control.RefugioNombre);
        }

        /// <summary>
        /// Sin refugio, la persona solo ve los controles que tiene de los animas que tiene en ella (POSTULANTE)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Listar_ComoPostulante_TraeSoloLosSuyos()
        {
            var propioId = CargarSeguimiento();
            CargarSeguimiento(CargarTenencia(mascotaId: CargarMascota("Luna"), usuarioId: CargarUsuario("carlos@test.org", "Carlos")));

            using var contexto = CrearContexto();
            var servicio = new SeguimientoService(contexto);

            var controles = await servicio.ListarAsync(_anaId, null, null, false, null);

            Assert.Equal(propioId, Assert.Single(controles).Id);
        }

        /// <summary>
        /// Filtro por estado, solo aparecen los contorles en ese estado (REFUGIO=)
        /// </summary>
        /// <returns></returns>

        [Fact]
        public async Task Listar_PorEstado_TraeSoloEseEstado()
        {
            CargarSeguimiento();
            var realizadoId = CargarSeguimiento(estado: EstadoSeguimiento.Realizado);

            using var contexto = CrearContexto();
            var servicio = new SeguimientoService(contexto);

            var controles = await servicio.ListarAsync(Guid.Empty, _refugioId, EstadoSeguimiento.Realizado, false, null);

            Assert.Equal(realizadoId, Assert.Single(controles).Id);
        }

        /// <summary>
        /// Filtros vencidos, solo aparecen los pendientes que tienenm la fecha ya pasada.(REFUGIO)
        /// </summary>
        /// <returns></returns>

        [Fact]
        public async Task Listar_Vencidos_TraeSoloLosPendientesAtrasados()
        {
            //arreglo para usar la fecha de argentina, fallaba despues de las 12, por el tema de UTC 21hs
            var ayer = FechaExtensions.HoyEnArgentina().AddDays(-1);
            var atrasadoId = CargarSeguimiento(fechaProgramada: ayer);
            CargarSeguimiento();
            CargarSeguimiento(estado: EstadoSeguimiento.Realizado, fechaProgramada: ayer);

            using var contexto = CrearContexto();
            var servicio = new SeguimientoService(contexto);

            var controles = await servicio.ListarAsync(Guid.Empty, _refugioId, null, true, null);

            Assert.Equal(atrasadoId, Assert.Single(controles).Id);
        }


        /// <summary>
        /// Filtrar por mascota, solo trae los controles de esa mascota (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Listar_PorMascota_TraeSoloLosDeEsaMascota()
        {
            var michiId = CargarSeguimiento();
            CargarSeguimiento(CargarTenencia(mascotaId: CargarMascota("Luna")));

            using var contexto = CrearContexto();
            var servicio = new SeguimientoService(contexto);

            var controles = await servicio.ListarAsync(Guid.Empty, _refugioId, null, false, _mascotaId);

            Assert.Equal(michiId, Assert.Single(controles).Id);
        }

        /// <summary>
        /// La agenda viene ordenada pro fecha de control, primero el control mas cercano (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Listar_VariosControles_LosOrdenaPorFecha()
        {
            var hoy = DateTime.UtcNow.Date;
            var tercero = CargarSeguimiento(fechaProgramada: hoy.AddDays(30));
            var primero = CargarSeguimiento(fechaProgramada: hoy.AddDays(1));
            var segundo = CargarSeguimiento(fechaProgramada: hoy.AddDays(10));

            using var contexto = CrearContexto();
            var servicio = new SeguimientoService(contexto);

            var controles = await servicio.ListarAsync(Guid.Empty, _refugioId, null, false, null);

            Assert.Equal(new[] { primero, segundo, tercero }, controles.Select(c => c.Id));
        }

        /// <summary>
        ///Control pendiente con la fecha pasada, tiene que venir como vencido, los de fechas futuras tienen que seguir bien (REFUGIO) 
        /// </summary>
        /// <returns></returns>

        [Fact]
        public async Task Listar_PendienteConFechaPasada_MarcaVencido()
        {
            var atrasadoId = CargarSeguimiento(fechaProgramada: DateTime.UtcNow.Date.AddDays(-3));
            var aTiempoId = CargarSeguimiento();

            using var contexto = CrearContexto();
            var servicio = new SeguimientoService(contexto);

            var controles = await servicio.ListarAsync(Guid.Empty, _refugioId, null, false, null);

            Assert.True(controles.Single(c => c.Id == atrasadoId).Vencido);
            Assert.False(controles.Single(c => c.Id == aTiempoId).Vencido);
        }

        /// <summary>
        /// Refugio agenda un control para el dia de hoy, queda pendiente con esa fecha, porque es hasta uqe se cumpla (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Crear_ParaHoy_QuedaPendiente()
        {
            var hoy = DateTime.UtcNow.Date;

            using var contexto = CrearContexto();
            var servicio = new SeguimientoService(contexto);

            var error = await servicio.CrearAsync(new SeguimientoRequest { TenenciaId = _tenenciaId, FechaProgramada = hoy }, _refugioId);

            var guardado = Assert.Single(LeerSeguimientos());
            Assert.Null(error);
            Assert.Equal(EstadoSeguimiento.Pendiente, guardado.Estado);
            Assert.Equal(hoy, guardado.FechaProgramada);
        }


        /// <summary>
        /// Crear sin una fecha un control, da error, tiene que tener fecha (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Crear_SinFecha_DaError()
        {
            using var contexto = CrearContexto();
            var servicio = new SeguimientoService(contexto);

            var error = await servicio.CrearAsync(new SeguimientoRequest { TenenciaId = _tenenciaId }, _refugioId);

            Assert.Equal("No existe la tenencia o la fecha del control.", error);
        }

        /// <summary>
        /// Crear con fecha pasada da error y no lo crea , y todo sigue como antes, se usa aviso (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Crear_ConFechaPasada_DaErrorYNoCrea()
        {
            using var contexto = CrearContexto();
            var servicio = new SeguimientoService(contexto);

            var error = await servicio.CrearAsync(new SeguimientoRequest { TenenciaId = _tenenciaId, FechaProgramada = FechaExtensions.HoyEnArgentina().AddDays(-1) }, _refugioId);

            Assert.Equal("La fecha del control tiene que ser de hoy para adelante.", error);
            Assert.Empty(LeerSeguimientos());
        }

        /// <summary>
        /// No se puede crear o agendar controles en la tenencia id de otro refguio, solo propios (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Crear_DeOtroRefugio_DaErrorYNoCrea()
        {
            using var contexto = CrearContexto();
            var servicio = new SeguimientoService(contexto);

            var error = await servicio.CrearAsync(new SeguimientoRequest { TenenciaId = _tenenciaId, FechaProgramada = DateTime.UtcNow.Date.AddDays(7) }, Guid.NewGuid());

            Assert.Equal("No encontramos esa tenencia en tus mascotas.", error);
            Assert.Empty(LeerSeguimientos());
        }


        /// <summary>
        /// No se peude crear a una tenencia que ya termino (Adopcion devuelta o trasnito terminado) , da error y no se agendarian controles nuevos (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Crear_TenenciaTerminada_DaErrorYNoCrea()
        {
            var terminadaId = CargarTenencia(mascotaId: CargarMascota("Luna"), fechaFinReal: DateTime.UtcNow);

            using var contexto = CrearContexto();
            var servicio = new SeguimientoService(contexto);

            var error = await servicio.CrearAsync(new SeguimientoRequest { TenenciaId = terminadaId, FechaProgramada = DateTime.UtcNow.Date.AddDays(7) }, _refugioId);

            Assert.Equal("La tenencia ya terminó: no se pueden agendar controles nuevos.", error);
            Assert.Empty(LeerSeguimientos());
        }

        /// <summary>
        /// Cambiar de esta a realizado, guarda la fecha en que se cambio y la observacion (REFUGIO)
        /// </summary>
        /// <returns></returns>

        [Fact]
        public async Task CambiarEstado_ARealizado_GuardaLaFechaYLaObservacion()
        {
            var controlId = CargarSeguimiento();

            using var contexto = CrearContexto();
            var servicio = new SeguimientoService(contexto);

            var error = await servicio.CambiarEstadoAsync(controlId, new SeguimientoRequest { Estado = EstadoSeguimiento.Realizado, Observacion = "  Esta muy bien  " }, _refugioId);

            var guardado = LeerSeguimiento(controlId);
            Assert.Null(error);
            Assert.Equal(EstadoSeguimiento.Realizado, guardado.Estado);
            Assert.NotNull(guardado.FechaRealizada);
            Assert.Equal("Esta muy bien", guardado.Observacion);
        }

        /// <summary>
        /// Cancelar un control lo cierra, sin la fecha de realizar, queda en el historial, pero no tiene fecha de finalizacion porque no se hizo (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CambiarEstado_ACancelado_NoGuardaFechaRealizada()
        {
            var controlId = CargarSeguimiento();

            using var contexto = CrearContexto();
            var servicio = new SeguimientoService(contexto);

            var error = await servicio.CambiarEstadoAsync(controlId, new SeguimientoRequest { Estado = EstadoSeguimiento.Cancelado }, _refugioId);

            var guardado = LeerSeguimiento(controlId);
            Assert.Null(error);
            Assert.Equal(EstadoSeguimiento.Cancelado, guardado.Estado);
            Assert.Null(guardado.FechaRealizada);
        }


        /// <summary>
        /// Reprogramar , hace que el control viejo aparezca con estado reprogramado y fecha vieja, para mantener un historial, y se agenda uno nuevo con fecha nueva (Refugio)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CambiarEstado_AReprogramado_DejaElViejoYCreaUnoNuevoPendiente()
        {
            var fechaOriginal = DateTime.UtcNow.Date.AddDays(2);
            var fechaNueva = DateTime.UtcNow.Date.AddDays(14);
            var controlId = CargarSeguimiento(fechaProgramada: fechaOriginal);

            using var contexto = CrearContexto();
            var servicio = new SeguimientoService(contexto);

            var error = await servicio.CambiarEstadoAsync(controlId, new SeguimientoRequest { Estado = EstadoSeguimiento.Reprogramado, FechaProgramada = fechaNueva }, _refugioId);

            var viejo = LeerSeguimiento(controlId);
            var nuevo = LeerSeguimientos().Single(s => s.Id != controlId);
            Assert.Null(error);
            Assert.Equal(EstadoSeguimiento.Reprogramado, viejo.Estado);
            Assert.Equal(fechaOriginal, viejo.FechaProgramada);
            Assert.Equal(EstadoSeguimiento.Pendiente, nuevo.Estado);
            Assert.Equal(fechaNueva, nuevo.FechaProgramada);
            Assert.Equal(_tenenciaId, nuevo.TenenciaId);
        }


        /// <summary>
        /// Reprogramar pero sin una fecha da error, mantiene el control como esta y no creo otro (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CambiarEstado_ReprogramadoSinFecha_DaErrorYNoCambia()
        {
            var controlId = CargarSeguimiento();

            using var contexto = CrearContexto();
            var servicio = new SeguimientoService(contexto);

            var error = await servicio.CambiarEstadoAsync(controlId, new SeguimientoRequest { Estado = EstadoSeguimiento.Reprogramado }, _refugioId);

            Assert.Equal("Para reprogramar hace falta una fecha nueva, de hoy para adelante", error);
            Assert.Equal(EstadoSeguimiento.Pendiente, Assert.Single(LeerSeguimientos()).Estado);
        }

        /// <summary>
        /// Fecha nueva en reprogramacion tiene qu eser a partir del dia de hoy no pasada (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CambiarEstado_ReprogramadoConFechaPasada_DaErrorYNoCambia()
        {
            var controlId = CargarSeguimiento();

            using var contexto = CrearContexto();
            var servicio = new SeguimientoService(contexto);

            var error = await servicio.CambiarEstadoAsync(controlId, new SeguimientoRequest { Estado = EstadoSeguimiento.Reprogramado, FechaProgramada = FechaExtensions.HoyEnArgentina().AddDays(-1) }, _refugioId);

            Assert.Equal("Para reprogramar hace falta una fecha nueva, de hoy para adelante", error);
            Assert.Equal(EstadoSeguimiento.Pendiente, Assert.Single(LeerSeguimientos()).Estado);
        }


        /// <summary>
        /// Si no llega el estado, no se puede cambiar, da error (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CambiarEstado_SinEstado_DaError()
        {
            var controlId = CargarSeguimiento();

            using var contexto = CrearContexto();
            var servicio = new SeguimientoService(contexto);

            var error = await servicio.CambiarEstadoAsync(controlId, new SeguimientoRequest(), _refugioId);

            Assert.Equal("No se encuentra el estado.", error);
        }

        /// <summary>
        /// No se puede cambiar o volver a pendiernte, es solo realizado , canelado y reprogramado. Uno nuevo con fecha futura si es pendiente (REFUGIO)
        /// </summary>
        /// <returns></returns>

        [Fact]
        public async Task CambiarEstado_APendiente_DaError()
        {
            var controlId = CargarSeguimiento();

            using var contexto = CrearContexto();
            var servicio = new SeguimientoService(contexto);

            var error = await servicio.CambiarEstadoAsync(controlId, new SeguimientoRequest { Estado = EstadoSeguimiento.Pendiente }, _refugioId);

            Assert.Equal("Un control solo puede pasar a Realizado, Cancelado o Reprogramado.", error);
        }

        /// <summary>
        /// Si no esta el control o no existe no se peude cambiar estado, da error (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CambiarEstado_ControlInexistente_DaError()
        {
            using var contexto = CrearContexto();
            var servicio = new SeguimientoService(contexto);

            var error = await servicio.CambiarEstadoAsync(Guid.NewGuid(), new SeguimientoRequest { Estado = EstadoSeguimiento.Realizado }, _refugioId);

            Assert.Equal("No encontramos ese control en tus mascotas.", error);
        }

        /// <summary>
        /// El refugio solo puede cerrar controles propios, no de otro id refugio, da error y continua como esta el control (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CambiarEstado_DeOtroRefugio_DaErrorYNoCambia()
        {
            var controlId = CargarSeguimiento();

            using var contexto = CrearContexto();
            var servicio = new SeguimientoService(contexto);

            var error = await servicio.CambiarEstadoAsync(controlId, new SeguimientoRequest { Estado = EstadoSeguimiento.Realizado }, Guid.NewGuid());

            Assert.Equal("No encontramos ese control en tus mascotas.", error);
            Assert.Equal(EstadoSeguimiento.Pendiente, LeerSeguimiento(controlId).Estado);
        }

        /// <summary>
        /// Control cerrado (realziado), no s epeude modificar, da error (REFUGIO)
        /// </summary>
        /// <returns></returns>

        [Fact]
        public async Task CambiarEstado_YaRealizado_DaErrorYNoCambia()
        {
            var controlId = CargarSeguimiento(estado: EstadoSeguimiento.Realizado);

            using var contexto = CrearContexto();
            var servicio = new SeguimientoService(contexto);

            var error = await servicio.CambiarEstadoAsync(controlId, new SeguimientoRequest { Estado = EstadoSeguimiento.Cancelado }, _refugioId);

            Assert.Equal("Este control ya está cerrado y no se puede modificar.", error);
            Assert.Equal(EstadoSeguimiento.Realizado, LeerSeguimiento(controlId).Estado);
        }

        /// <summary>
        /// Si termino la tenencia, los controles son solo elctura, no se peude crear, cambiar de estado aunque haya pendientes, quedan para historial (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CambiarEstado_TenenciaTerminada_DaErrorYNoCambia()
        {
            var controlId = CargarSeguimiento(CargarTenencia(mascotaId: CargarMascota("Luna"), fechaFinReal: DateTime.UtcNow));

            using var contexto = CrearContexto();
            var servicio = new SeguimientoService(contexto);

            var error = await servicio.CambiarEstadoAsync(controlId, new SeguimientoRequest { Estado = EstadoSeguimiento.Realizado }, _refugioId);

            Assert.Equal("La tenencia ya terminó: su libreta es de solo lectura.", error);
            Assert.Equal(EstadoSeguimiento.Pendiente, LeerSeguimiento(controlId).Estado);
        }
    }
}
