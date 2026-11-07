using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransitAR.Api.Services;
using TransitAR.Structures;
using TransitAR.Structures.Requests;
using TransitAR.Tests.Soporte;

namespace TransitAR.Tests.Services
{
    public class TenenciaServiceTests : BaseEnMemoria
    {
        private readonly Guid _refugioId = Guid.NewGuid();
        private readonly Guid _especieId = Guid.NewGuid();
        private readonly Guid _condicionId = Guid.NewGuid();
        private readonly Guid _mascotaId = Guid.NewGuid();
        private readonly Guid _anaId = Guid.NewGuid();

        public TenenciaServiceTests() 
        {
            using var contexto = CrearContexto();

            contexto.Refugios.Add(new Refugio { Id = _refugioId, Nombre = "Patitas", Email = "patitas@test.org", Localidad = "Lomas", Activo = true });
            contexto.Especies.Add(new Especie { Id = _especieId, Nombre = "Especie de prueba" });
            contexto.Condiciones.Add(new Condicion { Id = _condicionId, Nombre = "Condicion de prueba" });
            contexto.Mascotas.Add(new Mascota { Id = _mascotaId, Nombre = "Michi", RefugioId = _refugioId, EspecieId = _especieId, CondicionId = _condicionId });
            contexto.Usuarios.Add(NuevoUsuario(_anaId, "ana@test.org", "Ana"));
            contexto.SaveChanges();
        }

        //Datos para cargar
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

        private Guid CargarPublicacion(TipoPublicacion tipo, EstadoPublicacion estado = EstadoPublicacion.Pausada, Guid? mascotaId = null)
        {
            using var contexto = CrearContexto();

            var id = Guid.NewGuid();
            contexto.Publicaciones.Add(new Publicacion
            {
                Id = id,
                MascotaId = mascotaId ?? _mascotaId,
                Titulo = "Michi busca hogar",
                Descripcion = "Descripcion de prueba",
                Ubicacion = "Lomas",
                Tipo = tipo,
                Estado = estado
            });
            contexto.SaveChanges();

            return id;
        }

        private Guid CargarPostulacion(Guid publicacionId, Guid usuarioId, EstadoPostulacion estado = EstadoPostulacion.Aceptada)
        {
            using var contexto = CrearContexto();

            var id = Guid.NewGuid();
            contexto.Postulaciones.Add(new Postulacion { Id = id, PublicacionId = publicacionId, UsuarioId = usuarioId, Estado = estado });
            contexto.SaveChanges();

            return id;
        }

        //Cargo cadena completa de tenencia: publicaicon, postualcion aceptada y tenenecia
        private Guid CargarTenencia(TipoPublicacion modalidad = TipoPublicacion.Transito, DateTime? fechaFinEstimada = null, DateTime? fechaFinReal = null, Guid? mascotaId = null, Guid? usuarioId = null)
        {
            var publicacionId = CargarPublicacion(modalidad, EstadoPublicacion.Cerrada, mascotaId);
            var postulacionId = CargarPostulacion(publicacionId, usuarioId ?? _anaId);

            using var contexto = CrearContexto();

            var id = Guid.NewGuid();
            contexto.Tenencias.Add(new Tenencia
            {
                Id = id,
                PostulacionId = postulacionId,
                MascotaId = mascotaId ?? _mascotaId,
                Modalidad = modalidad,
                FechaInicio = DateTime.UtcNow,
                FechaFinEstimada = fechaFinEstimada,
                FechaFinReal = fechaFinReal
            });
            contexto.SaveChanges();

            return id;
        }

        private Guid CargarSeguimiento(Guid tenenciaId, EstadoSeguimiento estado = EstadoSeguimiento.Pendiente)
        {
            using var contexto = CrearContexto();

            var id = Guid.NewGuid();
            contexto.Seguimientos.Add(new Seguimiento { Id = id, TenenciaId = tenenciaId, FechaProgramada = DateTime.UtcNow.AddDays(7), Estado = estado });
            contexto.SaveChanges();

            return id;
        }


        //Lectura con el contexto nuevo, para ver lo que quedo guardado

        private Tenencia LeerTenencia(Guid id)
        {
            using var contexto = CrearContexto();

            return contexto.Tenencias.Single(t => t.Id == id);
        }

        private Mascota LeerMascota(Guid id)
        {
            using var contexto = CrearContexto();

            return contexto.Mascotas.Single(m => m.Id == id);
        }

        private Publicacion LeerPublicacion(Guid id)
        {
            using var contexto = CrearContexto();

            return contexto.Publicaciones.Single(p => p.Id == id);
        }

        private Postulacion LeerPostulacion(Guid id)
        {
            using var contexto = CrearContexto();

            return contexto.Postulaciones.Single(p => p.Id == id);
        }

        private Seguimiento LeerSeguimiento(Guid id)
        {
            using var contexto = CrearContexto();

            return contexto.Seguimientos.Single(s => s.Id == id);
        }

        private int ContarTenencias()
        {
            using var contexto = CrearContexto();

            return contexto.Tenencias.Count();
        }



        //********* PRUBEAS *************

        /// <summary>
        /// Confirmar una entrega de adocpciont iene que cerrar la publicacion para no aceptar mas candidatos y crear la tenencia para hacer el seguimiento
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ConfirmarEntrega_Adopcion_CreaLaTenenciaYCierraLaPublicacion()
        {
            var publicacionId = CargarPublicacion(TipoPublicacion.Adopcion);
            var postulacionId = CargarPostulacion(publicacionId, _anaId);

            using var contexto = CrearContexto();
            var servicio = new TenenciaService(contexto);

            var resultado = await servicio.ConfirmarEntregaAsync(new TenenciaRequest { PostulacionId = postulacionId }, _refugioId);

            Assert.Null(resultado.Error);
            Assert.Equal(TipoPublicacion.Adopcion, resultado.Tenencia!.Modalidad);
            Assert.Equal("Michi", resultado.Tenencia.MascotaNombre);
            Assert.Equal("Especie de prueba", resultado.Tenencia.EspecieNombre);
            Assert.Equal("Ana", resultado.Tenencia.Nombre);
            Assert.Equal(EstadoPublicacion.Cerrada, LeerPublicacion(publicacionId).Estado);
            Assert.Equal(EstadoMascota.Adoptada, LeerMascota(_mascotaId).Estado);
        }


        /// <summary>
        /// Confirmar el trasnito de la mascota deja la amscota en trasito con la fecha Estimada y ccierra la publicaicon
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ConfirmarEntrega_Transito_CierraLaPublicacionYDejaLaMascotaEnTransito()
        {
            var publicacionId = CargarPublicacion(TipoPublicacion.Transito);
            var postulacionId = CargarPostulacion(publicacionId, _anaId);
            var fechaEstimada = DateTime.UtcNow.Date.AddDays(30);

            using var contexto = CrearContexto();
            var servicio = new TenenciaService(contexto);

            var resultado = await servicio.ConfirmarEntregaAsync(new TenenciaRequest { PostulacionId = postulacionId, FechaFinEstimada = fechaEstimada }, _refugioId);

            Assert.Null(resultado.Error);
            Assert.Equal(fechaEstimada, LeerTenencia(resultado.Tenencia!.Id).FechaFinEstimada);
            Assert.Equal(EstadoMascota.EnTransito, LeerMascota(_mascotaId).Estado);
            Assert.Equal(EstadoPublicacion.Cerrada, LeerPublicacion(publicacionId).Estado);
        }

        /// <summary>
        ///  Confirmar entrega de adopcion con fecha estiamda no la guarda , porque no deberia tener devolcuion (ignora el REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ConfirmarEntrega_AdopcionConFechaEstimada_NoLaGuarda()
        {
            var postulacionId = CargarPostulacion(CargarPublicacion(TipoPublicacion.Adopcion), _anaId);

            using var contexto = CrearContexto();
            var servicio = new TenenciaService(contexto);

            var resultado = await servicio.ConfirmarEntregaAsync(new TenenciaRequest { PostulacionId = postulacionId, FechaFinEstimada = DateTime.UtcNow.AddDays(30) }, _refugioId);

            Assert.Null(resultado.Error);
            Assert.Null(LeerTenencia(resultado.Tenencia!.Id).FechaFinEstimada);
        }

        /// <summary>
        /// Si se confirma la entrega, los otros candidatos que estaban en espera tienen que ser rechazados, si son retirados no los tooc (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ConfirmarEntrega_ConOtrasPostulaciones_RechazaSoloLasAbiertas()
        {
            var publicacionId = CargarPublicacion(TipoPublicacion.Adopcion);
            var elegidaId = CargarPostulacion(publicacionId, _anaId);
            var enEsperaId = CargarPostulacion(publicacionId, CargarUsuario("carlos@test.org", "Carlos"), EstadoPostulacion.EnEspera);
            var pendienteId = CargarPostulacion(publicacionId, CargarUsuario("sofia@test.org", "Sofia"), EstadoPostulacion.Pendiente);
            var retiradaId = CargarPostulacion(publicacionId, CargarUsuario("laura@test.org", "Laura"), EstadoPostulacion.Retirada);

            using var contexto = CrearContexto();
            var servicio = new TenenciaService(contexto);

            await servicio.ConfirmarEntregaAsync(new TenenciaRequest { PostulacionId = elegidaId }, _refugioId);

            Assert.Equal(EstadoPostulacion.Aceptada, LeerPostulacion(elegidaId).Estado);
            Assert.Equal(EstadoPostulacion.Rechazada, LeerPostulacion(enEsperaId).Estado);
            Assert.Equal(EstadoPostulacion.Rechazada, LeerPostulacion(pendienteId).Estado);
            Assert.Equal(EstadoPostulacion.Retirada, LeerPostulacion(retiradaId).Estado);
        }


        /// <summary>
        /// No se peude confirmar la entrega sin los datos del refugio (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ConfirmarEntrega_SinRefugio_DaError()
        {
            var postulacionId = CargarPostulacion(CargarPublicacion(TipoPublicacion.Adopcion), _anaId);

            using var contexto = CrearContexto();
            var servicio = new TenenciaService(contexto);

            var resultado = await servicio.ConfirmarEntregaAsync(new TenenciaRequest { PostulacionId = postulacionId }, Guid.Empty);

            Assert.Equal("No se recibieron los datos de la entrega.", resultado.Error);
        }



        /// <summary>
        /// No se puede confirmar la entrega de una postulacion de otro refugio (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ConfirmarEntrega_DeOtroRefugio_DaErrorYNoCrea()
        {
            var postulacionId = CargarPostulacion(CargarPublicacion(TipoPublicacion.Adopcion), _anaId);

            using var contexto = CrearContexto();
            var servicio = new TenenciaService(contexto);

            var resultado = await servicio.ConfirmarEntregaAsync(new TenenciaRequest { PostulacionId = postulacionId }, Guid.NewGuid());

            Assert.Equal("No encontramos esa postulacion en tus publicaciones.", resultado.Error);
            Assert.Equal(0, ContarTenencias());
        }


        /// <summary>
        /// SOlo se le puede entregar a un candiato aceptado. Si la postulacion la tengo en pendiente es error y no crea la tenencia(REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ConfirmarEntrega_PostulacionPendiente_DaErrorYNoCrea()
        {
            var postulacionId = CargarPostulacion(CargarPublicacion(TipoPublicacion.Adopcion), _anaId, EstadoPostulacion.Pendiente);

            using var contexto = CrearContexto();
            var servicio = new TenenciaService(contexto);

            var resultado = await servicio.ConfirmarEntregaAsync(new TenenciaRequest { PostulacionId = postulacionId }, _refugioId);

            Assert.Equal("Solo se puede confirmar la entrega de una postulacion aceptada.", resultado.Error);
            Assert.Equal(0, ContarTenencias());
        }

        /// <summary>
        /// No confirmar la entrega dos veces, solo una tenencia , no se crea otra (Refugio)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ConfirmarEntrega_DosVeces_DaErrorYNoCrea()
        {
            var postulacionId = LeerTenencia(CargarTenencia()).PostulacionId;

            using var contexto = CrearContexto();
            var servicio = new TenenciaService(contexto);

            var resultado = await servicio.ConfirmarEntregaAsync(new TenenciaRequest { PostulacionId = postulacionId }, _refugioId);

            Assert.Equal("Esta entrega ya fue confirmada.", resultado.Error);
            Assert.Equal(1, ContarTenencias());
        }

        /// <summary>
        /// Fecha de devolcuion para un transito tiene que ser a partir del dia, para el futuro. Si tiene fecha pasada da error (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ConfirmarEntrega_TransitoConFechaPasada_DaErrorYNoCrea()
        {
            var postulacionId = CargarPostulacion(CargarPublicacion(TipoPublicacion.Transito), _anaId);

            using var contexto = CrearContexto();
            var servicio = new TenenciaService(contexto);

            var resultado = await servicio.ConfirmarEntregaAsync(new TenenciaRequest { PostulacionId = postulacionId, FechaFinEstimada = DateTime.UtcNow.AddDays(-1) }, _refugioId);

            Assert.Equal("La fecha estimada de devolucion tiene que ser posterior a hoy.", resultado.Error);
            Assert.Equal(0, ContarTenencias());
        }


        /// <summary>
        /// No se puede ver la tenencia de otro refugio, da null (REFUGIO)
        /// </summary>
        /// <returns></returns>

        [Fact]
        public async Task Obtener_DeOtroRefugio_DaNull()
        {
            var tenenciaId = CargarTenencia();

            using var contexto = CrearContexto();
            var servicio = new TenenciaService(contexto);

            var tenencia = await servicio.ObtenerTenenciaAsync(tenenciaId, Guid.NewGuid());

            Assert.Null(tenencia);
        }

        /// <summary>
        /// Si tengo un transito en curso con fecha de evolcuion pasada, se pasa a vencido (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Obtener_TransitoConFechaPasada_MarcaVencida()
        {
            var tenenciaId = CargarTenencia(fechaFinEstimada: DateTime.UtcNow.AddDays(-2));

            using var contexto = CrearContexto();
            var servicio = new TenenciaService(contexto);

            var tenencia = await servicio.ObtenerTenenciaAsync(tenenciaId, _refugioId);

            Assert.True(tenencia!.EstaVencida);
        }

        /// <summary>
        /// Si tiene un control pendiente la tenencia , es decir seguimiento por hacer, cuando se busca el response avisa al refugio (en web con una card) (Refugio)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Obtener_ConControlPendiente_LoMarca()
        {
            var tenenciaId = CargarTenencia();
            CargarSeguimiento(tenenciaId);

            using var contexto = CrearContexto();
            var servicio = new TenenciaService(contexto);

            var tenencia = await servicio.ObtenerTenenciaAsync(tenenciaId, _refugioId);

            Assert.True(tenencia!.TieneControlPendiente);
        }

        /// <summary>
        /// La agenda del refugio muestra solo las tenencia propias, no las de otro id refugio (REFUGIO)
        /// </summary>
        /// <returns></returns>

        [Fact]
        public async Task Listar_SinFiltros_TraeSoloLasDelRefugio()
        {
            var propiaId = CargarTenencia();
            CargarTenencia(mascotaId: CargarMascotaDeOtroRefugio());

            using var contexto = CrearContexto();
            var servicio = new TenenciaService(contexto);

            var tenencias = await servicio.ListarTenenciasAsync(_refugioId, null, null, false, false);

            Assert.Equal(propiaId, Assert.Single(tenencias).Id);
        }


        /// <summary>
        /// Uso el filtro adopcion, no pueden venir los trasnsitos tambien (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Listar_PorModalidad_TraeSoloEsaModalidad()
        {
            var adopcionId = CargarTenencia(TipoPublicacion.Adopcion);
            CargarTenencia(TipoPublicacion.Transito);

            using var contexto = CrearContexto();
            var servicio = new TenenciaService(contexto);

            var tenencias = await servicio.ListarTenenciasAsync(_refugioId, TipoPublicacion.Adopcion, null, false, false);

            Assert.Equal(adopcionId, Assert.Single(tenencias).Id);
        }

        /// <summary>
        /// Filtro en curso , tiene que traer todas las que estan abiertas (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Listar_EnCurso_TraeSoloLasAbiertas()
        {
            var abiertaId = CargarTenencia();
            CargarTenencia(fechaFinReal: DateTime.UtcNow);

            using var contexto = CrearContexto();
            var servicio = new TenenciaService(contexto);

            var tenencias = await servicio.ListarTenenciasAsync(_refugioId, null, true, false, false);

            Assert.Equal(abiertaId, Assert.Single(tenencias).Id);
        }


        /// <summary>
        /// Filtro finalizadas, solo trea las que estan terminadas (REFUGIO)
        /// </summary>
        /// <returns></returns>

        [Fact]
        public async Task Listar_Finalizadas_TraeSoloLasCerradas()
        {
            CargarTenencia();
            var cerradaId = CargarTenencia(fechaFinReal: DateTime.UtcNow);

            using var contexto = CrearContexto();
            var servicio = new TenenciaService(contexto);

            var tenencias = await servicio.ListarTenenciasAsync(_refugioId, null, false, false, false);

            Assert.Equal(cerradaId, Assert.Single(tenencias).Id);
        }


        /// <summary>
        /// Filtro vencidas, trae solo los transitos en fuera de curso de la fecha de entrega, es decir pasados. (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Listar_Vencidas_TraeSoloLosTransitosAtrasados()
        {
            var ayer = DateTime.UtcNow.AddDays(-1);
            var atrasadaId = CargarTenencia(fechaFinEstimada: ayer);
            CargarTenencia(fechaFinEstimada: DateTime.UtcNow.AddDays(10));
            CargarTenencia(fechaFinEstimada: ayer, fechaFinReal: DateTime.UtcNow);
            CargarTenencia(TipoPublicacion.Adopcion, fechaFinEstimada: ayer);

            using var contexto = CrearContexto();
            var servicio = new TenenciaService(contexto);

            var tenencias = await servicio.ListarTenenciasAsync(_refugioId, null, null, true, false);

            Assert.Equal(atrasadaId, Assert.Single(tenencias).Id);
        }


        /// <summary>
        /// Filtro sin controlos, muestra las que no tienen ningun seguimiento o control por hacer, para si el rfugio queiere agendar uno (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Listar_SinControles_NoTraeLasQueTienenUnoPendiente()
        {
            var sinControlId = CargarTenencia();
            var realizadoId = CargarTenencia();
            CargarSeguimiento(realizadoId, EstadoSeguimiento.Realizado);
            CargarSeguimiento(CargarTenencia());

            using var contexto = CrearContexto();
            var servicio = new TenenciaService(contexto);

            var tenencias = await servicio.ListarTenenciasAsync(_refugioId, null, null, false, true);

            Assert.Equal(2, tenencias.Count);
            Assert.Contains(tenencias, t => t.Id == sinControlId);
            Assert.Contains(tenencias, t => t.Id == realizadoId);
        }


        /// <summary>
        /// Devolver, la tenencia se ciera con el balance y obsercacion que se haya querido poner o defecto, y la mascota vuelve al refugio (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Devolver_EnCurso_CierraLaTenenciaYLaMascotaVuelveAlRefugio()
        {
            var tenenciaId = CargarTenencia();

            using (var datos = CrearContexto())
            {
                datos.Mascotas.Single(m => m.Id == _mascotaId).Estado = EstadoMascota.EnTransito;
                datos.SaveChanges();
            }

            using var contexto = CrearContexto();
            var servicio = new TenenciaService(contexto);

            var resultado = await servicio.DevolverAsync(tenenciaId, new DevolucionRequest { FinalizoBien = true, ObservacionCierre = "  Todo bien  " }, _refugioId);

            var guardada = LeerTenencia(tenenciaId);
            Assert.Null(resultado.Error);
            Assert.NotNull(guardada.FechaFinReal);
            Assert.True(guardada.FinalizoBien);
            Assert.Equal("Todo bien", guardada.ObservacionCierre);
            Assert.Equal(EstadoMascota.EnRefugio, LeerMascota(_mascotaId).Estado);
        }

        /// <summary>
        /// Si se devuelve, los controles que faltaba, quedan cancelados automaticamente, se guarda los realziados para historial (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Devolver_ConControles_CancelaSoloLosPendientes()
        {
            var tenenciaId = CargarTenencia();
            var pendienteId = CargarSeguimiento(tenenciaId);
            var realizadoId = CargarSeguimiento(tenenciaId, EstadoSeguimiento.Realizado);

            using var contexto = CrearContexto();
            var servicio = new TenenciaService(contexto);

            await servicio.DevolverAsync(tenenciaId, new DevolucionRequest { FinalizoBien = true }, _refugioId);

            Assert.Equal(EstadoSeguimiento.Cancelado, LeerSeguimiento(pendienteId).Estado);
            Assert.Equal(EstadoSeguimiento.Realizado, LeerSeguimiento(realizadoId).Estado);
        }
        

        /// <summary>
        /// Si no se tienen los datos de la devolucion , da error (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Devolver_SinDatos_DaError()
        {
            var tenenciaId = CargarTenencia();

            using var contexto = CrearContexto();
            var servicio = new TenenciaService(contexto);

            var resultado = await servicio.DevolverAsync(tenenciaId, null!, _refugioId);

            Assert.Equal("No se recibieron los datos de la devolucion.", resultado.Error);
        }


        /// <summary>
        /// Un refugio no puede cerrar la tenenica de otro refugio, da error y no cambia como estaba la tenencia (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Devolver_DeOtroRefugio_DaErrorYNoCambia()
        {
            var tenenciaId = CargarTenencia();

            using var contexto = CrearContexto();
            var servicio = new TenenciaService(contexto);

            var resultado = await servicio.DevolverAsync(tenenciaId, new DevolucionRequest(), Guid.NewGuid());

            Assert.Equal("No encontramos esa tenencia.", resultado.Error);
            Assert.Null(LeerTenencia(tenenciaId).FechaFinReal);
        }


        /// <summary>
        /// Una tenencia ya cerrada no se puede devolver de nuevo (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Devolver_YaCerrada_DaError()
        {
            var tenenciaId = CargarTenencia(fechaFinReal: DateTime.UtcNow);

            using var contexto = CrearContexto();
            var servicio = new TenenciaService(contexto);

            var resultado = await servicio.DevolverAsync(tenenciaId, new DevolucionRequest(), _refugioId);

            Assert.Equal("Esta tenencia ya estaba cerrada.", resultado.Error);
        }


        /// <summary>
        /// Un transito pasa a adopcion sobre la misma tenencia (para que quede en el historuial) , se cambia la modalidad a adopcion y se borra la fecha de devolucion
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ConvertirAAdopcion_Transito_CambiaLaMismaTenenciaYLaMascotaQuedaAdoptada()
        {
            var tenenciaId = CargarTenencia(fechaFinEstimada: DateTime.UtcNow.AddDays(10));

            using var contexto = CrearContexto();
            var servicio = new TenenciaService(contexto);

            var resultado = await servicio.ConvertirAAdopcionAsync(tenenciaId, _refugioId);

            var guardada = LeerTenencia(tenenciaId);
            Assert.Null(resultado.Error);
            Assert.Equal(1, ContarTenencias());
            Assert.Equal(TipoPublicacion.Adopcion, guardada.Modalidad);
            Assert.NotNull(guardada.FechaConversion);
            Assert.Null(guardada.FechaFinEstimada);
            Assert.Equal(EstadoMascota.Adoptada, LeerMascota(_mascotaId).Estado);
        }


        /// <summary>
        /// No se puede convertir la tenencia de otro refugio, da error y no cambia la tenencia (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ConvertirAAdopcion_DeOtroRefugio_DaErrorYNoCambia()
        {
            var tenenciaId = CargarTenencia();

            using var contexto = CrearContexto();
            var servicio = new TenenciaService(contexto);

            var resultado = await servicio.ConvertirAAdopcionAsync(tenenciaId, Guid.NewGuid());

            Assert.Equal("No encontramos esa tenencia.", resultado.Error);
            Assert.Equal(TipoPublicacion.Transito, LeerTenencia(tenenciaId).Modalidad);
        }

        /// <summary>
        /// Un transito devuelvo, no se peude convertir, da error y continua en transito (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ConvertirAAdopcion_YaCerrada_DaErrorYNoCambia()
        {
            var tenenciaId = CargarTenencia(fechaFinReal: DateTime.UtcNow);

            using var contexto = CrearContexto();
            var servicio = new TenenciaService(contexto);

            var resultado = await servicio.ConvertirAAdopcionAsync(tenenciaId, _refugioId);

            Assert.Equal("Esta tenencia ya estaba cerrada.", resultado.Error);
            Assert.Equal(TipoPublicacion.Transito, LeerTenencia(tenenciaId).Modalidad);
        }

        /// <summary>
        /// Solo puedo convertir de trasnito a adopcion, no de adiocio a adopcion, ni a transito (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ConvertirAAdopcion_UnaAdopcion_DaError()
        {
            var tenenciaId = CargarTenencia(TipoPublicacion.Adopcion);

            using var contexto = CrearContexto();
            var servicio = new TenenciaService(contexto);

            var resultado = await servicio.ConvertirAAdopcionAsync(tenenciaId, _refugioId);

            Assert.Equal("Solo se puede convertir un transito: esta tenencia ya es una adopcion.", resultado.Error);
            Assert.Null(LeerTenencia(tenenciaId).FechaConversion);
        }

        /// <summary>
        /// Sin usuario no puedo tener historial de tenencia, me da null
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task HistorialPostulante_SinUsuario_DaNull()
        {
            using var contexto = CrearContexto();
            var servicio = new TenenciaService(contexto);

            var historial = await servicio.ObtenerHistorialPostulanteAsync(Guid.Empty, _refugioId);

            Assert.Null(historial);
        }

        /// <summary>
        /// Solo se ve el historial de un postulante a las publciaicones del refugio, no se puede ver de todo (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task HistorialPostulante_QueNoSePostuloAlRefugio_DaNull()
        {
            CargarTenencia(mascotaId: CargarMascotaDeOtroRefugio());

            using var contexto = CrearContexto();
            var servicio = new TenenciaService(contexto);

            var historial = await servicio.ObtenerHistorialPostulanteAsync(_anaId, _refugioId);

            Assert.Null(historial);
        }

        /// <summary>
        /// Postualnte se psutlo a publicaiocn de un refugio, el refugio si peude ver el hiostorial de tenenica de otros refugios si es que tiene, solo los de ese ususario
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task HistorialPostulante_QueSePostuloAlRefugio_TraeTambienLasDeOtrosRefugios()
        {
            CargarPostulacion(CargarPublicacion(TipoPublicacion.Adopcion), _anaId, EstadoPostulacion.Pendiente);
            CargarTenencia(mascotaId: CargarMascotaDeOtroRefugio());

            using var contexto = CrearContexto();
            var servicio = new TenenciaService(contexto);

            var historial = await servicio.ObtenerHistorialPostulanteAsync(_anaId, _refugioId);

            var tenencia = Assert.Single(historial!);
            Assert.Equal("Toto", tenencia.MascotaNombre);
            Assert.Equal("Refugio del Sur", tenencia.RefugioNombre);
        }

        /// <summary>
        /// Solo se ven los ocntroles de un usuario que se postulo a una publciacion del refugio
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ControlesPostulante_QueNoSePostuloAlRefugio_DaNull()
        {
            CargarSeguimiento(CargarTenencia(mascotaId: CargarMascotaDeOtroRefugio()));

            using var contexto = CrearContexto();
            var servicio = new TenenciaService(contexto);

            var controles = await servicio.ObtenerControlesPostulanteAsync(_anaId, _refugioId);

            Assert.Null(controles);
        }

        /// <summary>
        /// Se traen lso controles del usuario postulado y no otro usuario. viene mascota, nombre y el refugio de dodne s ehizo el control (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ControlesPostulante_QueSePostuloAlRefugio_TraeSoloLosSuyos()
        {
            CargarSeguimiento(CargarTenencia());
            CargarSeguimiento(CargarTenencia(usuarioId: CargarUsuario("carlos@test.org", "Carlos")));

            using var contexto = CrearContexto();
            var servicio = new TenenciaService(contexto);

            var controles = await servicio.ObtenerControlesPostulanteAsync(_anaId, _refugioId);

            var control = Assert.Single(controles!);
            Assert.Equal("Ana Perez", control.PersonaNombre);
            Assert.Equal("Michi", control.MascotaNombre);
            Assert.Equal("Patitas", control.RefugioNombre);
        }

        /// <summary>
        /// Si no hay usuario , la lista se devuelve vacia de las tenencias (Postulante)
        /// </summary>
        /// <returns></returns>

        [Fact]
        public async Task MisTenencias_SinUsuario_DevuelveVacio()
        {
            CargarTenencia();

            using var contexto = CrearContexto();
            var servicio = new TenenciaService(contexto);

            var tenencias = await servicio.ObtenerMisTenenciasAsync(Guid.Empty);

            Assert.Empty(tenencias);
        }

        /// <summary>
        /// Se trae solo las tenencias del usuario , no de otroso posutlantes (Postualnte)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task MisTenencias_ConVariosUsuarios_TraeSoloLasPropias()
        {
            var propiaId = CargarTenencia();
            CargarTenencia(usuarioId: CargarUsuario("carlos@test.org", "Carlos"));

            using var contexto = CrearContexto();
            var servicio = new TenenciaService(contexto);

            var tenencias = await servicio.ObtenerMisTenenciasAsync(_anaId);

            Assert.Equal(propiaId, Assert.Single(tenencias).Id);
        }

        /// <summary>
        /// El psotulante ve el transito en curso y vencido si ya paso la fecha de devolcuoon (POSTULANTE)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task MisTenencias_TransitoConFechaPasada_MarcaEnCursoYVencida()
        {
            CargarTenencia(fechaFinEstimada: DateTime.UtcNow.AddDays(-2));

            using var contexto = CrearContexto();
            var servicio = new TenenciaService(contexto);

            var tenencias = await servicio.ObtenerMisTenenciasAsync(_anaId);

            var tenencia = Assert.Single(tenencias);
            Assert.True(tenencia.EnCurso);
            Assert.True(tenencia.VencidaEnCurso);
        }

    }
}
