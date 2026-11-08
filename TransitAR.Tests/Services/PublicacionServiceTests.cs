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
    public class PublicacionServiceTests : BaseEnMemoria
    {
        //guids
        private readonly Guid _refugioId = Guid.NewGuid();
        private readonly Guid _especieId = Guid.NewGuid();
        private readonly Guid _condicionId = Guid.NewGuid();
        private readonly Guid _mascotaId = Guid.NewGuid();


        //Creo el nuevo contexto
        public PublicacionServiceTests()
        {
            using var contexto = CrearContexto();

            contexto.Refugios.Add(new Refugio { Id = _refugioId, Nombre = "Patitas", Email = "patitas@test.org", Localidad = "Lomas", Activo = true });
            contexto.Especies.Add(new Especie { Id = _especieId, Nombre = "Gato" });
            contexto.Condiciones.Add(new Condicion { Id = _condicionId, Nombre = "Condicion de prueba" });
            contexto.Mascotas.Add(new Mascota { Id = _mascotaId, Nombre = "Michi", RefugioId = _refugioId, EspecieId = _especieId, CondicionId = _condicionId, Estado = EstadoMascota.EnRefugio });
            contexto.SaveChanges();
        }

        //Datos

        private Guid CargarRefugio(bool activo = true)
        {
            using var contexto = CrearContexto();

            var id = Guid.NewGuid();
            contexto.Refugios.Add(new Refugio { Id = id, Nombre = "Refugio del Sur", Email = $"{id}@test.org", Localidad = "Adrogue", Activo = activo });
            contexto.SaveChanges();

            return id;
        }

        private Guid CargarEspecie(string nombre)
        {
            using var contexto = CrearContexto();

            var id = Guid.NewGuid();
            contexto.Especies.Add(new Especie { Id = id, Nombre = nombre });
            contexto.SaveChanges();

            return id;
        }

        private Guid CargarMascota(Guid? refugioId = null, Guid? especieId = null, Tamanio? tamanio = null, Sexo? sexo = null, EstadoMascota estado = EstadoMascota.EnRefugio)
        {
            using var contexto = CrearContexto();

            var id = Guid.NewGuid();
            contexto.Mascotas.Add(new Mascota
            {
                Id = id,
                Nombre = "Toto",
                RefugioId = refugioId ?? _refugioId,
                EspecieId = especieId ?? _especieId,
                CondicionId = _condicionId,
                Tamanio = tamanio,
                Sexo = sexo,
                Estado = estado
            });
            contexto.SaveChanges();

            return id;
        }

        private Guid CargarFoto(Guid mascotaId, int orden)
        {
            using var contexto = CrearContexto();

            var id = Guid.NewGuid();
            contexto.Add(new FotoMascota { Id = id, MascotaId = mascotaId, Orden = orden, ContentType = "image/jpeg", Contenido = new byte[] { 1 } });
            contexto.SaveChanges();

            return id;

        }
        private Guid CargarPublicacion(Guid? mascotaId = null, TipoPublicacion tipo = TipoPublicacion.Adopcion, EstadoPublicacion estado = EstadoPublicacion.Activa, string ubicacion = "Lomas", DateTime? fecha = null)
        {
            using var contexto = CrearContexto();

            var id = Guid.NewGuid();
            contexto.Publicaciones.Add(new Publicacion
            {
                Id = id,
                MascotaId = mascotaId ?? _mascotaId,
                Titulo = "Busca hogar",
                Descripcion = "Descripcion de prueba",
                Ubicacion = ubicacion,
                Tipo = tipo,
                Estado = estado,
                FechaPublicacion = fecha ?? DateTime.UtcNow
            });
            contexto.SaveChanges();

            return id;
        }


        private Guid CargarPostulacion(Guid publicacionId, EstadoPostulacion estado = EstadoPostulacion.Pendiente)
        {
            using var contexto = CrearContexto();

            var usuarioId = Guid.NewGuid();
            var id = Guid.NewGuid();
            contexto.Usuarios.Add(new Usuario { Id = usuarioId, Email = $"{usuarioId}@test.org", PasswordHash = "hash-de-prueba", Nombre = "Ana", Apellido = "Perez", Rol = RolUsuario.Usuario, Activo = true });
            contexto.Postulaciones.Add(new Postulacion { Id = id, PublicacionId = publicacionId, UsuarioId = usuarioId, Estado = estado });
            contexto.SaveChanges();

            return id;
        }

        private void CargarTenencia(Guid postulacionId)
        {
            using var contexto = CrearContexto();

            contexto.Tenencias.Add(new Tenencia { Id = Guid.NewGuid(), PostulacionId = postulacionId, MascotaId = _mascotaId, Modalidad = TipoPublicacion.Adopcion, FechaInicio = DateTime.UtcNow });
            contexto.SaveChanges();
        }

        private PublicacionRequest NuevoRequest(TipoPublicacion tipo = TipoPublicacion.Adopcion, int? plazo = null, Guid? mascotaId = null) => new()
        {
            MascotaId = mascotaId ?? _mascotaId,
            Titulo = "  Michi busca hogar  ",
            Descripcion = "  Es muy carinoso  ",
            Ubicacion = "  Lomas de Zamora  ",
            Tipo = tipo,
            PlazoEstimado = plazo
        };

        //Lectura con el contexto nuevo, para ver lo que quedo guardado
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

        //cuento para comprobar qu eno se crearon demas
        private int ContarPublicaciones()
        {
            using var contexto = CrearContexto();

            return contexto.Publicaciones.Count();
        }

        //********* PRUBEAS *************

        /// <summary>
        /// Lista las publicaciones propias del refugio, con la cantidad ed postulantes que se postularon (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ListarPublicaciones_DelRefugio_TraeSoloLasSuyasConLaCantidadDePostulaciones()
        {
            var propiaId = CargarPublicacion();
            CargarPostulacion(propiaId);
            CargarPostulacion(propiaId);
            CargarPublicacion(CargarMascota(CargarRefugio()));

            using var contexto = CrearContexto();
            var servicio = new PublicacionService(contexto);

            var publicaciones = await servicio.ListarPublicacionesAsync(_refugioId);

            var publicacion = Assert.Single(publicaciones);
            Assert.Equal(propiaId, publicacion.Id);
            Assert.Equal(2, publicacion.CantidadPostulaciones);
        }


        /// <summary>
        /// Publicaiones s emeustran ya ordenadas de la mas nueva a la vieja (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ListarPublicaciones_VariasPublicaciones_LasOrdenaDeLaMasNueva()
        {
            var vieja = CargarPublicacion(estado: EstadoPublicacion.Cerrada, fecha: DateTime.UtcNow.AddDays(-30));
            var nueva = CargarPublicacion(CargarMascota(), fecha: DateTime.UtcNow);
            var media = CargarPublicacion(CargarMascota(), fecha: DateTime.UtcNow.AddDays(-10));

            using var contexto = CrearContexto();
            var servicio = new PublicacionService(contexto);

            var publicaciones = await servicio.ListarPublicacionesAsync(_refugioId);

            Assert.Equal(new[] { nueva, media, vieja }, publicaciones.Select(p => p.Id));
        }

        /// <summary>
        /// La publciacion tiene que venir con las fotos de las mascotas, manteniendo el orden que se eligio en el refugio (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ListarPublicaciones_MascotaConFotos_TraeLasFotosEnOrden()
        {
            var segunda = CargarFoto(_mascotaId, 2);
            var primera = CargarFoto(_mascotaId, 1);
            CargarPublicacion();

            using var contexto = CrearContexto();
            var servicio = new PublicacionService(contexto);

            var publicaciones = await servicio.ListarPublicacionesAsync(_refugioId);

            Assert.Equal(new[] { primera, segunda }, Assert.Single(publicaciones).FotosIds);
        }

        /// <summary>
        /// No se puede obtener publicaicones de otro refugio, da null (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Obtener_DeOtroRefugio_DaNull()
        {
            var publicacionId = CargarPublicacion();

            using var contexto = CrearContexto();
            var servicio = new PublicacionService(contexto);

            var publicacion = await servicio.ObtenerPublicacionAsync(publicacionId, Guid.NewGuid());

            Assert.Null(publicacion);
        }

        /// <summary>
        /// Crear una publicacion, publicar la mascota tiene qu eestar en el refugio, la publciacion pasa a activa con la fecha que se hizo y se hace trim (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Crear_MascotaEnRefugio_QuedaActivaSinEspacios()
        {
            using var contexto = CrearContexto();
            var servicio = new PublicacionService(contexto);

            var resultado = await servicio.CrearPublicacionAsync(NuevoRequest(), _refugioId);

            var guardada = LeerPublicacion(resultado.Publicacion!.Id);
            Assert.Null(resultado.Error);
            Assert.Equal(EstadoPublicacion.Activa, guardada.Estado);
            Assert.Equal("Michi busca hogar", guardada.Titulo);
            Assert.Equal("Es muy carinoso", guardada.Descripcion);
            Assert.Equal("Lomas de Zamora", guardada.Ubicacion);
            Assert.Equal(DateTime.UtcNow.Date, guardada.FechaPublicacion.Date);
        }

        /// <summary>
        /// Si se crea una adopcion con plazo da error y no se guarda kla publicacion (en la web no esta la op) , solo hay plazo estimado en trasnitos (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Crear_AdopcionConPlazo_NoGuardaElPlazo()
        {
            using var contexto = CrearContexto();
            var servicio = new PublicacionService(contexto);

            var resultado = await servicio.CrearPublicacionAsync(NuevoRequest(TipoPublicacion.Adopcion, plazo: 3), _refugioId);

            Assert.Null(LeerPublicacion(resultado.Publicacion!.Id).PlazoEstimado);
        }


        /// <summary>
        /// Si se crea un trasnito tiene que tener plazo estimado (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Crear_TransitoConPlazo_GuardaElPlazo()
        {
            using var contexto = CrearContexto();
            var servicio = new PublicacionService(contexto);

            var resultado = await servicio.CrearPublicacionAsync(NuevoRequest(TipoPublicacion.Transito, plazo: 3), _refugioId);

            Assert.Equal(3, LeerPublicacion(resultado.Publicacion!.Id).PlazoEstimado);
        }

        /// <summary>
        /// No se peude crear la publciacion con mascota de otro refugio, da error y no cra nada (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Crear_MascotaDeOtroRefugio_DaErrorYNoCrea()
        {
            var ajenaId = CargarMascota(CargarRefugio());

            using var contexto = CrearContexto();
            var servicio = new PublicacionService(contexto);

            var resultado = await servicio.CrearPublicacionAsync(NuevoRequest(mascotaId: ajenaId), _refugioId);

            Assert.Equal("No encontramos esa mascota en tu refugio.", resultado.Error);
            Assert.Equal(0, ContarPublicaciones());
        }

        /// <summary>
        /// No se puede publciar una mascota ya con transito (o adoptada) si ya esta con alguien (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Crear_MascotaEnTransito_DaErrorYNoCrea()
        {
            var enTransitoId = CargarMascota(estado: EstadoMascota.EnTransito);

            using var contexto = CrearContexto();
            var servicio = new PublicacionService(contexto);

            var resultado = await servicio.CrearPublicacionAsync(NuevoRequest(mascotaId: enTransitoId), _refugioId);

            Assert.Equal("Solo se pueden publicar mascotas que estan en el refugio.", resultado.Error);
            Assert.Equal(0, ContarPublicaciones());
        }

        /// <summary>
        /// No se peude publicar si ya hay una publicacion pausada, da error y nio se crea, tiene que tener cerrada o no tener (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Crear_ConOtraPausada_DaErrorYNoCrea()
        {
            CargarPublicacion(estado: EstadoPublicacion.Pausada);

            using var contexto = CrearContexto();
            var servicio = new PublicacionService(contexto);

            var resultado = await servicio.CrearPublicacionAsync(NuevoRequest(), _refugioId);

            Assert.Equal("Esta mascota ya tiene una publicacion activa o pausada. Cerrala antes de crear otra.", resultado.Error);
            Assert.Equal(1, ContarPublicaciones());
        }


        /// <summary>
        /// Se peude crear una nueva publicacion si la mascota cuenta con publciaicon cerrada, queda de historial (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Crear_ConOtraCerrada_LaCrea()
        {
            CargarPublicacion(estado: EstadoPublicacion.Cerrada);

            using var contexto = CrearContexto();
            var servicio = new PublicacionService(contexto);

            var resultado = await servicio.CrearPublicacionAsync(NuevoRequest(), _refugioId);

            Assert.Null(resultado.Error);
            Assert.Equal(2, ContarPublicaciones());
        }

        /// <summary>
        /// Actualizar o editar un apublciaciona ctiva tien eque guardar los cambios, pasando por TRim (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Actualizar_Activa_GuardaLosCambiosSinEspacios()
        {
            var publicacionId = CargarPublicacion();

            using var contexto = CrearContexto();
            var servicio = new PublicacionService(contexto);

            var resultado = await servicio.ActualizarPublicacionAsync(publicacionId, NuevoRequest(), _refugioId);

            var guardada = LeerPublicacion(publicacionId);
            Assert.Null(resultado.Error);
            Assert.Equal("Michi busca hogar", guardada.Titulo);
            Assert.Equal("Es muy carinoso", guardada.Descripcion);
            Assert.Equal("Lomas de Zamora", guardada.Ubicacion);
        }

        /// <summary>
        /// Si no hay postualciones, se peude cambiar el tipo de adopcion a trasnito y guardar el plazo (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Actualizar_ATransitoSinPostulaciones_CambiaElTipo()
        {
            var publicacionId = CargarPublicacion();

            using var contexto = CrearContexto();
            var servicio = new PublicacionService(contexto);

            await servicio.ActualizarPublicacionAsync(publicacionId, NuevoRequest(TipoPublicacion.Transito, plazo: 2), _refugioId);

            var guardada = LeerPublicacion(publicacionId);
            Assert.Equal(TipoPublicacion.Transito, guardada.Tipo);
            Assert.Equal(2, guardada.PlazoEstimado);
        }


        /// <summary>
        /// Si hay postualciones a la publicacion  no se puede cambiar el tipo (adopcion/trasnito) da error y sigue siendo adopcion (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Actualizar_CambiarTipoConPostulaciones_DaErrorYNoCambia()
        {
            var publicacionId = CargarPublicacion();
            CargarPostulacion(publicacionId);

            using var contexto = CrearContexto();
            var servicio = new PublicacionService(contexto);

            var resultado = await servicio.ActualizarPublicacionAsync(publicacionId, NuevoRequest(TipoPublicacion.Transito), _refugioId);

            Assert.Equal("No se puede cambiar entre adopcion y transito porque la publicacion ya tiene postulaciones.", resultado.Error);
            Assert.Equal(TipoPublicacion.Adopcion, LeerPublicacion(publicacionId).Tipo);
        }

        /// <summary>
        /// Se peude editar el texto con postulaciones, no se peude cambiar el tipo nada mas, se guardan los cambios correctamente (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Actualizar_ConPostulacionesSinCambiarElTipo_GuardaLosCambios()
        {
            var publicacionId = CargarPublicacion();
            CargarPostulacion(publicacionId);

            using var contexto = CrearContexto();
            var servicio = new PublicacionService(contexto);

            var resultado = await servicio.ActualizarPublicacionAsync(publicacionId, NuevoRequest(), _refugioId);

            Assert.Null(resultado.Error);
            Assert.Equal("Michi busca hogar", LeerPublicacion(publicacionId).Titulo);
        }

        /// <summary>
        ///  Publicacion cerrada queda solo lectura, no se puede actualziar ni editar, es para historial, si se trata de editar da error y no cambia (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Actualizar_Cerrada_DaErrorYNoCambia()
        {
            var publicacionId = CargarPublicacion(estado: EstadoPublicacion.Cerrada);

            using var contexto = CrearContexto();
            var servicio = new PublicacionService(contexto);

            var resultado = await servicio.ActualizarPublicacionAsync(publicacionId, NuevoRequest(), _refugioId);

            Assert.Equal("Una publicacion cerrada no se edita.", resultado.Error);
            Assert.Equal("Busca hogar", LeerPublicacion(publicacionId).Titulo);
        }

        /// <summary>
        /// No se peude editar una publicacion con id de otro refugio da error y s emantiene como esta (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Actualizar_DeOtroRefugio_DaErrorYNoCambia()
        {
            var publicacionId = CargarPublicacion();

            using var contexto = CrearContexto();
            var servicio = new PublicacionService(contexto);

            var resultado = await servicio.ActualizarPublicacionAsync(publicacionId, NuevoRequest(), Guid.NewGuid());

            Assert.Equal("No encontramos esa publicacion en tu refugio.", resultado.Error);
            Assert.Equal("Busca hogar", LeerPublicacion(publicacionId).Titulo);
        }


        /// <summary>
        /// Cambiar el estado a a pausada, saca del tablon publico de publicaciones, pero no se encuentra cerrada (sin fecha de cierre) (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CambiarEstado_APausada_LaPausaSinFechaDeCierre()
        {
            var publicacionId = CargarPublicacion();

            using var contexto = CrearContexto();
            var servicio = new PublicacionService(contexto);

            var resultado = await servicio.CambiarEstadoAsync(publicacionId, EstadoPublicacion.Pausada, _refugioId);

            var guardada = LeerPublicacion(publicacionId);
            Assert.Null(resultado.Error);
            Assert.Equal(EstadoPublicacion.Pausada, guardada.Estado);
            Assert.Null(guardada.FechaCierre);
        }

        /// <summary>
        /// Si se cierra a mano, el estado pasa a cerrado y se guarda la fecha, automaticamente se rechazan las postulaciones que estaban abiertas 
        /// con un motivo (que puso el refugio o default, no ocupa cupo en lo malo).La que fue aceptada sigue y pasa a tenencia (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CambiarEstado_ACerrada_GuardaLaFechaYRechazaLasAbiertas()
        {
            var publicacionId = CargarPublicacion();
            var pendienteId = CargarPostulacion(publicacionId);
            var enEsperaId = CargarPostulacion(publicacionId, EstadoPostulacion.EnEspera);
            var aceptadaSinEntregaId = CargarPostulacion(publicacionId, EstadoPostulacion.Aceptada);
            var entregadaId = CargarPostulacion(publicacionId, EstadoPostulacion.Aceptada);
            CargarTenencia(entregadaId);
            var retiradaId = CargarPostulacion(publicacionId, EstadoPostulacion.Retirada);

            using var contexto = CrearContexto();
            var servicio = new PublicacionService(contexto);

            await servicio.CambiarEstadoAsync(publicacionId, EstadoPublicacion.Cerrada, _refugioId);

            Assert.NotNull(LeerPublicacion(publicacionId).FechaCierre);
            Assert.Equal(EstadoPostulacion.Rechazada, LeerPostulacion(pendienteId).Estado);
            Assert.Equal(EstadoPostulacion.Rechazada, LeerPostulacion(enEsperaId).Estado);
            Assert.Equal(EstadoPostulacion.Rechazada, LeerPostulacion(aceptadaSinEntregaId).Estado);
            Assert.Equal("El refugio cerro la publicacion.", LeerPostulacion(pendienteId).ObservacionRechazo);
            Assert.Equal(EstadoPostulacion.Aceptada, LeerPostulacion(entregadaId).Estado);
            Assert.Equal(EstadoPostulacion.Retirada, LeerPostulacion(retiradaId).Estado);
        }



        /// <summary>
        /// Publicaicon cerrada no puede cambiar de estado a abierta o pausada, hay que crear nueva, da error y queda cerrada para hsitorial (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CambiarEstado_Cerrada_DaErrorYNoCambia()
        {
            var publicacionId = CargarPublicacion(estado: EstadoPublicacion.Cerrada);

            using var contexto = CrearContexto();
            var servicio = new PublicacionService(contexto);

            var resultado = await servicio.CambiarEstadoAsync(publicacionId, EstadoPublicacion.Activa, _refugioId);

            Assert.Equal("Una publicacion cerrada no se puede reabrir. Si la mascota sigue disponible, crea una publicacion nueva.", resultado.Error);
            Assert.Equal(EstadoPublicacion.Cerrada, LeerPublicacion(publicacionId).Estado);
        }

        /// <summary>
        /// No se peude pausar o cerrar cambiando de estado la publciacion de id refugio de otro, da error y no cambia (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CambiarEstado_DeOtroRefugio_DaErrorYNoCambia()
        {
            var publicacionId = CargarPublicacion();

            using var contexto = CrearContexto();
            var servicio = new PublicacionService(contexto);

            var resultado = await servicio.CambiarEstadoAsync(publicacionId, EstadoPublicacion.Cerrada, Guid.NewGuid());

            Assert.Equal("No encontramos esa publicacion en tu refugio.", resultado.Error);
            Assert.Equal(EstadoPublicacion.Activa, LeerPublicacion(publicacionId).Estado);
        }

        /// <summary>
        /// Obtener las publicaciones activas publicas, trae los datos de la mascota especie y refugio (VISITANTE Y POSTULANTES)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ObtenerActiva_Activa_TraeLosDatosDeLaMascotaYElRefugio()
        {
            var publicacionId = CargarPublicacion();

            using var contexto = CrearContexto();
            var servicio = new PublicacionService(contexto);

            var publicacion = await servicio.ObtenerActivaAsync(publicacionId);

            Assert.Equal("Michi", publicacion!.MascotaNombre);
            Assert.Equal("Gato", publicacion.EspecieNombre);
            Assert.Equal("Patitas", publicacion.RefugioNombre);
            Assert.Equal("Lomas", publicacion.RefugioLocalidad);
        }

        /// <summary>
        /// No se peude obtener el id de una pausada (marcadores) no esta publica , da null (VISITANTE Y POSTULANTES)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ObtenerActiva_Pausada_DaNull()
        {
            var publicacionId = CargarPublicacion(estado: EstadoPublicacion.Pausada);

            using var contexto = CrearContexto();
            var servicio = new PublicacionService(contexto);

            var publicacion = await servicio.ObtenerActivaAsync(publicacionId);

            Assert.Null(publicacion);
        }

        /// <summary>
        /// Obtneer una puvlicaicon activa de un refugio bloqueda da null, no se peuden ver publicamente si un refugio fue bloqueado por un admin (VISITANTE Y POSTULANTES)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ObtenerActiva_DeRefugioBloqueado_DaNull()
        {
            var publicacionId = CargarPublicacion(CargarMascota(CargarRefugio(activo: false)));

            using var contexto = CrearContexto();
            var servicio = new PublicacionService(contexto);

            var publicacion = await servicio.ObtenerActivaAsync(publicacionId);

            Assert.Null(publicacion);
        }


        /// <summary>
        /// Listar las activas publicamente sin filtros, trae todas las abiertas (no las pausadas, cerrdas ni refugio bloqueado) (VISITANTE Y POSTULANTES)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ListarActivas_SinFiltros_TraeSoloLasActivasDeRefugiosActivos()
        {
            var activaId = CargarPublicacion();
            CargarPublicacion(CargarMascota(), estado: EstadoPublicacion.Pausada);
            CargarPublicacion(CargarMascota(), estado: EstadoPublicacion.Cerrada);
            CargarPublicacion(CargarMascota(CargarRefugio(activo: false)));

            using var contexto = CrearContexto();
            var servicio = new PublicacionService(contexto);

            var publicaciones = await servicio.ListarActivasAsync(null, null, null, null, null);

            Assert.Equal(activaId, Assert.Single(publicaciones).Id);
        }

        /// <summary>
        /// Filtro por tipo, solo trae el tipo transito o adopcion en cada caso (VISITANTE Y POSTULANTES)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ListarActivas_PorTipo_TraeSoloEseTipo()
        {
            CargarPublicacion();
            var transitoId = CargarPublicacion(CargarMascota(), TipoPublicacion.Transito);

            using var contexto = CrearContexto();
            var servicio = new PublicacionService(contexto);

            var publicaciones = await servicio.ListarActivasAsync(TipoPublicacion.Transito, null, null, null, null);

            Assert.Equal(transitoId, Assert.Single(publicaciones).Id);
        }

        /// <summary>
        /// Filtro por especie, solo trae las especies filtradas obviamente publicaicones activas (abiertas) (VISITANTE Y POSTULANTES)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ListarActivas_PorEspecie_TraeSoloEsaEspecie()
        {
            CargarPublicacion();
            var perroId = CargarEspecie("Perro");
            var publicacionPerroId = CargarPublicacion(CargarMascota(especieId: perroId));

            using var contexto = CrearContexto();
            var servicio = new PublicacionService(contexto);

            var publicaciones = await servicio.ListarActivasAsync(null, perroId, null, null, null);

            Assert.Equal(publicacionPerroId, Assert.Single(publicaciones).Id);
        }

        /// <summary>
        /// Filtro por tamaño , solo trae el tamñao filtrado de las publciaciones activas (abiertas) (VISITANTE Y POSTULANTES)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ListarActivas_PorTamanio_TraeSoloEseTamanio()
        {
            CargarPublicacion(CargarMascota(tamanio: Tamanio.Chico));
            var grandeId = CargarPublicacion(CargarMascota(tamanio: Tamanio.Grande));

            using var contexto = CrearContexto();
            var servicio = new PublicacionService(contexto);

            var publicaciones = await servicio.ListarActivasAsync(null, null, Tamanio.Grande, null, null);

            Assert.Equal(grandeId, Assert.Single(publicaciones).Id);
        }

        /// <summary>
        /// Filtro por sexo, solo trae el sexo filtrado de las publicaciones activas (abiertas) (VISITANTE Y POSTULANTES)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ListarActivas_PorSexo_TraeSoloEseSexo()
        {
            CargarPublicacion(CargarMascota(sexo: Sexo.Macho));
            var hembraId = CargarPublicacion(CargarMascota(sexo: Sexo.Hembra));

            using var contexto = CrearContexto();
            var servicio = new PublicacionService(contexto);

            var publicaciones = await servicio.ListarActivasAsync(null, null, null, Sexo.Hembra, null);

            Assert.Equal(hembraId, Assert.Single(publicaciones).Id);
        }


        /// <summary>
        /// Filtro por ubicvacion, se busca por parte del texto, uso del trim (VISITANTE Y POSTULANTES)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ListarActivas_PorUbicacion_BuscaPorParteDelTexto()
        {
            var lomasId = CargarPublicacion(ubicacion: "Lomas de Zamora");
            CargarPublicacion(CargarMascota(), ubicacion: "Adrogue");

            using var contexto = CrearContexto();
            var servicio = new PublicacionService(contexto);

            var publicaciones = await servicio.ListarActivasAsync(null, null, null, null, "  Lomas ");

            Assert.Equal(lomasId, Assert.Single(publicaciones).Id);
        }

    }
}
