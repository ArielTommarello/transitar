using Microsoft.Extensions.Configuration;
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
    public class PostulacionServiceTests : BaseEnMemoria
    {

        private readonly Guid _refugioId = Guid.NewGuid();
        private readonly Guid _especieId = Guid.NewGuid();
        private readonly Guid _condicionId = Guid.NewGuid();
        private readonly Guid _mascotaId = Guid.NewGuid();
        private readonly Guid _publicacionId = Guid.NewGuid();
        private readonly Guid _anaId = Guid.NewGuid();

        private readonly IConfiguration _configuracion;


        public PostulacionServiceTests()
        {
            //mismo cupo que puse en appsetings 
            _configuracion = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Reglas:CupoPostulacionesAbiertas"] = "3"
                })
                .Build();

            using var contexto = CrearContexto();

            contexto.Refugios.Add(new Refugio { Id = _refugioId, Nombre = "Patitas", Email = "patitas@test.org", Localidad = "Lomas", Activo = true });
            contexto.Especies.Add(new Especie { Id = _especieId, Nombre = "Especie de prueba" });
            contexto.Condiciones.Add(new Condicion { Id = _condicionId, Nombre = "Condicion de prueba" });
            contexto.Mascotas.Add(new Mascota { Id = _mascotaId, Nombre = "Michi", RefugioId = _refugioId, EspecieId = _especieId, CondicionId = _condicionId });
            contexto.Publicaciones.Add(NuevaPublicacion(_publicacionId, TipoPublicacion.Adopcion, EstadoPublicacion.Activa));
            contexto.Usuarios.Add(NuevoUsuario(_anaId, "ana@test.org", "Ana"));
            contexto.PerfilPostulantes.Add(new PerfilPostulante { Id = Guid.NewGuid(), UsuarioId = _anaId, Seleccion = SeleccionUsuario.Ambos });
            contexto.SaveChanges();
        }


        //Datos
        private Publicacion NuevaPublicacion(Guid id, TipoPublicacion tipo, EstadoPublicacion estado) => new()
        {
            Id = id,
            MascotaId = _mascotaId,
            Titulo = "Michi busca hogar",
            Descripcion = "Descripcion de prueba",
            Ubicacion = "Lomas",
            Tipo = tipo,
            Estado = estado
        };

        private static Usuario NuevoUsuario(Guid id, string email, string nombre) => new()
        {
            Id = id,
            Email = email,
            PasswordHash = "hash-de-prueba",
            Nombre = nombre,
            Apellido = "Perez",
            Rol = RolUsuario.Usuario,
            Activo = true
        };

        private Guid CargarPublicacion(TipoPublicacion tipo = TipoPublicacion.Adopcion, EstadoPublicacion estado = EstadoPublicacion.Activa)
        {
            using var contexto = CrearContexto();

            var id = Guid.NewGuid();
            contexto.Publicaciones.Add(NuevaPublicacion(id, tipo, estado));
            contexto.SaveChanges();

            return id;
        }

        private Guid CargarUsuario(string email, string nombre)
        {
            using var contexto = CrearContexto();

            var id = Guid.NewGuid();
            contexto.Usuarios.Add(NuevoUsuario(id, email, nombre));
            contexto.SaveChanges();

            return id;
        }

        private Guid CargarPostulacion(Guid publicacionId, Guid usuarioId, EstadoPostulacion estado, string? observacion = null)
        {
            using var contexto = CrearContexto();

            var id = Guid.NewGuid();
            contexto.Postulaciones.Add(new Postulacion
            {
                Id = id,
                PublicacionId = publicacionId,
                UsuarioId = usuarioId,
                Estado = estado,
                ObservacionRechazo = observacion
            });
            contexto.SaveChanges();

            return id;
        }

        private void CambiarPerfil(SeleccionUsuario seleccion)
        {
            using var contexto = CrearContexto();

            contexto.PerfilPostulantes.Single(p => p.UsuarioId == _anaId).Seleccion = seleccion;
            contexto.SaveChanges();
        }
        
        //leer la postualcion
        private Postulacion LeerPostulacion(Guid id)
        {
            using var contexto = CrearContexto();

            return contexto.Postulaciones.Single(p => p.Id == id);
        }

        private Publicacion LeerPublicacion(Guid id)
        {
            using var contexto = CrearContexto();

            return contexto.Publicaciones.Single(p => p.Id == id);
        }

        //********* PRUBEAS *************

        /// <summary>
        /// Si se postula con todo ok, tiene que pasar a pendeinte ahsta que el refugio lo acepte o elija
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Postularse_TodoEnOrden_QuedaPendienteConLosDatosDeLaMascota()
        {
            using var contexto = CrearContexto();
            var servicio = new PostulacionService(contexto, _configuracion);

            var resultado = await servicio.PostularseAsync(new PostulacionRequest { PublicacionId = _publicacionId }, _anaId);

            Assert.Null(resultado.Error);
            Assert.Equal(EstadoPostulacion.Pendiente, resultado.Postulacion!.Estado);
            Assert.Equal("Michi", resultado.Postulacion.MascotaNombre);
            Assert.Equal("Especie de prueba", resultado.Postulacion.EspecieNombre);
            Assert.Equal("Patitas", resultado.Postulacion.RefugioNombre);
        }


        /// <summary>
        /// Si se postula sin perfil completo de error - redirect en la web a completar perfil
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Postularse_SinPerfil_DevuelveError()
        {
            var usuarioId = CargarUsuario("sinperfil@test.org", "Sofia");

            using var contexto = CrearContexto();
            var servicio = new PostulacionService(contexto, _configuracion);

            var resultado = await servicio.PostularseAsync(new PostulacionRequest { PublicacionId = _publicacionId }, usuarioId);

            Assert.Equal("Tenes que completar tu perfil antes de poder postularte.", resultado.Error);
        }


       /// <summary>
       /// Postularse a una publicaicon que ya no existe da error
       /// </summary>
       /// <returns></returns>
        [Fact]
        public async Task Postularse_APublicacionInexistente_DaError()
        {
            using var contexto = CrearContexto();
            var servicio = new PostulacionService(contexto, _configuracion);

            var resultado = await servicio.PostularseAsync(new PostulacionRequest { PublicacionId = Guid.NewGuid() }, _anaId);

            Assert.Equal("La publicacion no existe.", resultado.Error);
        }

        /// <summary>
        /// Postualrse a una publicacion en pausa da Error 
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Postularse_APublicacionPausada_DaError()
        {
            var publicacionId = CargarPublicacion(estado: EstadoPublicacion.Pausada);

            using var contexto = CrearContexto();
            var servicio = new PostulacionService(contexto, _configuracion);

            var resultado = await servicio.PostularseAsync(new PostulacionRequest { PublicacionId = publicacionId }, _anaId);

            Assert.Equal("La publicacion ya no esta recibiendo postulaciones o se encuentra pausada.", resultado.Error);
        }


        /// <summary>
        /// Postualrse a un refugio bloqueado da error (caso marcadores)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Postularse_ARefugioBloqueado_DaError()
        {
            using (var datos = CrearContexto())
            {
                datos.Refugios.Single(r => r.Id == _refugioId).Activo = false;
                datos.SaveChanges();
            }

            using var contexto = CrearContexto();
            var servicio = new PostulacionService(contexto, _configuracion);

            var resultado = await servicio.PostularseAsync(new PostulacionRequest { PublicacionId = _publicacionId }, _anaId);

            Assert.Equal("La publicacion ya no esta recibiendo postulaciones o se encuentra pausada.", resultado.Error);
        }

        /// <summary>
        /// Postualarse siendo adoptante a trasnisto (marcadores) da error
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Postularse_AdoptanteATransito_DaError()
        {
            CambiarPerfil(SeleccionUsuario.Adoptante);
            var publicacionId = CargarPublicacion(tipo: TipoPublicacion.Transito);

            using var contexto = CrearContexto();
            var servicio = new PostulacionService(contexto, _configuracion);

            var resultado = await servicio.PostularseAsync(new PostulacionRequest { PublicacionId = publicacionId }, _anaId);

            Assert.Equal("Tu perfil dice que buscas adoptar. Si tambien queres transitar, cambialo en Mi perfil.", resultado.Error);
        }

        /// <summary>
        /// Postualarse siendo trasnsito a adopcion (marcadores) da error
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Postularse_TransitoAAdopcion_DaError()
        {
            CambiarPerfil(SeleccionUsuario.Transito);

            using var contexto = CrearContexto();
            var servicio = new PostulacionService(contexto, _configuracion);

            var resultado = await servicio.PostularseAsync(new PostulacionRequest { PublicacionId = _publicacionId }, _anaId);

            Assert.Equal("Tu perfil dice que buscas transitar. Si tambien queres adoptar, cambialo en Mi perfil.", resultado.Error);
        }


        /// <summary>
        /// Postualrse con cupo lleno da error (3)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Postularse_ConCupoLleno_DaError()
        {
            CargarPostulacion(CargarPublicacion(), _anaId, EstadoPostulacion.Pendiente);
            CargarPostulacion(CargarPublicacion(), _anaId, EstadoPostulacion.EnEspera);
            CargarPostulacion(CargarPublicacion(), _anaId, EstadoPostulacion.Aceptada);

            using var contexto = CrearContexto();
            var servicio = new PostulacionService(contexto, _configuracion);

            var resultado = await servicio.PostularseAsync(new PostulacionRequest { PublicacionId = _publicacionId }, _anaId);

            Assert.Equal("Ya tenes 3 postulaciones abiertas. Retira alguna para poder postularte a otra.", resultado.Error);
        }

        /// <summary>
        /// Si se poostual de nuevo y ya fue aceptado no tiene que sumar cupo
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Postularse_ConAceptadaQueYaTieneTenencia_NoOcupaCupo()
        {
            CargarPostulacion(CargarPublicacion(), _anaId, EstadoPostulacion.Pendiente);
            CargarPostulacion(CargarPublicacion(), _anaId, EstadoPostulacion.Pendiente);
            var aceptadaId = CargarPostulacion(CargarPublicacion(), _anaId, EstadoPostulacion.Aceptada);

            using (var datos = CrearContexto())
            {
                datos.Tenencias.Add(new Tenencia { Id = Guid.NewGuid(), PostulacionId = aceptadaId, MascotaId = _mascotaId });
                datos.SaveChanges();
            }

            using var contexto = CrearContexto();
            var servicio = new PostulacionService(contexto, _configuracion);

            var resultado = await servicio.PostularseAsync(new PostulacionRequest { PublicacionId = _publicacionId }, _anaId);

            Assert.Null(resultado.Error);
        }

        /// <summary>
        /// Postualrse dos veces da error (doble click o error pagina)
        /// </summary>
        /// <returns></returns>

        [Fact]
        public async Task Postularse_DosVeces_DaError()
        {
            CargarPostulacion(_publicacionId, _anaId, EstadoPostulacion.Pendiente);

            using var contexto = CrearContexto();
            var servicio = new PostulacionService(contexto, _configuracion);

            var resultado = await servicio.PostularseAsync(new PostulacionRequest { PublicacionId = _publicacionId }, _anaId);

            Assert.Equal("Ya te postulaste a esta publicacion.", resultado.Error);
        }


        /// <summary>
        /// Postualrse despues de ser rechazado da erorr, ya no se puede interectuar en esa publciacion
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Postularse_DespuesDeUnRechazo_DaError()
        {
            CargarPostulacion(_publicacionId, _anaId, EstadoPostulacion.Rechazada);

            using var contexto = CrearContexto();
            var servicio = new PostulacionService(contexto, _configuracion);

            var resultado = await servicio.PostularseAsync(new PostulacionRequest { PublicacionId = _publicacionId }, _anaId);

            Assert.Equal("Ya te postulaste a esta publicacion.", resultado.Error);
        }

        /// <summary>
        /// Volver a postualarse a una publicaicon de la que se retiro, abre la misma id de postulacion para que quede el historial
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Postularse_DespuesDeRetirarse_ReabreLaMismaPostulacion()
        {
            var postulacionId = CargarPostulacion(_publicacionId, _anaId, EstadoPostulacion.Retirada);

            using var contexto = CrearContexto();
            var servicio = new PostulacionService(contexto, _configuracion);

            var resultado = await servicio.PostularseAsync(new PostulacionRequest { PublicacionId = _publicacionId }, _anaId);

            Assert.Null(resultado.Error);
            Assert.Equal(postulacionId, resultado.Postulacion!.Id);
            Assert.Equal(EstadoPostulacion.Pendiente, LeerPostulacion(postulacionId).Estado);
        }



        /// <summary>
        /// Retiro de una pendiente  pasa aa estado retirada
        /// </summary>
        /// <returns></returns>

        [Fact]
        public async Task Retirar_Pendiente_QuedaRetirada()
        {
            var postulacionId = CargarPostulacion(_publicacionId, _anaId, EstadoPostulacion.Pendiente);

            using var contexto = CrearContexto();
            var servicio = new PostulacionService(contexto, _configuracion);

            var resultado = await servicio.RetirarPostulacionAsync(postulacionId, _anaId);

            Assert.Null(resultado.Error);
            Assert.Equal(EstadoPostulacion.Retirada, LeerPostulacion(postulacionId).Estado);
        }


        /// <summary>
        /// No se peude retirar una aceptada , da error
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Retirar_Aceptada_DaErrorYNoCambia()
        {
            var postulacionId = CargarPostulacion(_publicacionId, _anaId, EstadoPostulacion.Aceptada);

            using var contexto = CrearContexto();
            var servicio = new PostulacionService(contexto, _configuracion);

            var resultado = await servicio.RetirarPostulacionAsync(postulacionId, _anaId);

            Assert.Equal("Solo se pueden retirar las postulaciones que siguen en evaluacion o no fueron retiradas.", resultado.Error);
            Assert.Equal(EstadoPostulacion.Aceptada, LeerPostulacion(postulacionId).Estado);
        }

        /// <summary>
        /// Retirar de otro id de usuario da error, no se peude
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Retirar_DeOtroUsuario_DaErrorYNoCambia()
        {
            var postulacionId = CargarPostulacion(_publicacionId, _anaId, EstadoPostulacion.Pendiente);
            var otroId = CargarUsuario("otro@test.org", "Carlos");

            using var contexto = CrearContexto();
            var servicio = new PostulacionService(contexto, _configuracion);

            var resultado = await servicio.RetirarPostulacionAsync(postulacionId, otroId);

            Assert.Equal("No encontramos esa postulacion.", resultado.Error);
            Assert.Equal(EstadoPostulacion.Pendiente, LeerPostulacion(postulacionId).Estado);
        }

        /// <summary>
        /// Si acepto una pasa las otras postulaciones a espera y la publicaicon tiene que ir pausada para no aceptar mas postulaciones
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Aceptar_Pendiente_PasaLasOtrasAEsperaYPausaLaPublicacion()
        {
            var elegidaId = CargarPostulacion(_publicacionId, _anaId, EstadoPostulacion.Pendiente);
            var otraId = CargarPostulacion(_publicacionId, CargarUsuario("otro@test.org", "Carlos"), EstadoPostulacion.Pendiente);

            using var contexto = CrearContexto();
            var servicio = new PostulacionService(contexto, _configuracion);

            var resultado = await servicio.AceptarAsync(_publicacionId, elegidaId, _refugioId);

            Assert.Null(resultado.Error);
            Assert.Equal(EstadoPostulacion.Aceptada, LeerPostulacion(elegidaId).Estado);
            Assert.Equal(EstadoPostulacion.EnEspera, LeerPostulacion(otraId).Estado);
            Assert.Equal(EstadoPublicacion.Pausada, LeerPublicacion(_publicacionId).Estado);
        }


        /// <summary>
        /// No se peude aceptar mas de una postulacion, da error
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Aceptar_ConOtraYaAceptada_DaError()
        {
            CargarPostulacion(_publicacionId, CargarUsuario("otro@test.org", "Carlos"), EstadoPostulacion.Aceptada);
            var postulacionId = CargarPostulacion(_publicacionId, _anaId, EstadoPostulacion.Pendiente);

            using var contexto = CrearContexto();
            var servicio = new PostulacionService(contexto, _configuracion);

            var resultado = await servicio.AceptarAsync(_publicacionId, postulacionId, _refugioId);

            Assert.Equal("Ya aceptaste a un candidato. Resolve esa postulacion antes de aceptar otra.", resultado.Error);
        }



        /// <summary>
        /// No se puede aceptar publciaciones de otros refugios da error y se mantiene
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Aceptar_DeOtroRefugio_DaErrorYNoCambia()
        {
            var postulacionId = CargarPostulacion(_publicacionId, _anaId, EstadoPostulacion.Pendiente);

            using var contexto = CrearContexto();
            var servicio = new PostulacionService(contexto, _configuracion);

            var resultado = await servicio.AceptarAsync(_publicacionId, postulacionId, Guid.NewGuid());

            Assert.Equal("No encontramos esa publicacion.", resultado.Error);
            Assert.Equal(EstadoPostulacion.Pendiente, LeerPostulacion(postulacionId).Estado);
        }


        /// <summary>
        /// Rechazar una pendiente gaurda la observacion (puede ser escrita o default, para futuro) y no afecta la publicaciojn
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Rechazar_Pendiente_GuardaLaObservacionYNoAfectaLaPublicacion()
        {
            var postulacionId = CargarPostulacion(_publicacionId, _anaId, EstadoPostulacion.Pendiente);

            using var contexto = CrearContexto();
            var servicio = new PostulacionService(contexto, _configuracion);

            var resultado = await servicio.RechazarAsync(_publicacionId, postulacionId, "  No tiene patio  ", _refugioId);

            var guardada = LeerPostulacion(postulacionId);
            Assert.Null(resultado.Error);
            Assert.Equal(EstadoPostulacion.Rechazada, guardada.Estado);
            Assert.Equal("No tiene patio", guardada.ObservacionRechazo);
            Assert.Equal(EstadoPublicacion.Activa, LeerPublicacion(_publicacionId).Estado);
        }


        /// <summary>
        /// Rechazar una aceptada, reabre una publicacion y devuevle las otras a pendeintes para volver a aceptar otra
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Rechazar_LaAceptada_ReabreLaPublicacionYDevuelveLasOtrasAPendiente()
        {
            var aceptadaId = CargarPostulacion(_publicacionId, _anaId, EstadoPostulacion.Aceptada);
            var enEsperaId = CargarPostulacion(_publicacionId, CargarUsuario("otro@test.org", "Carlos"), EstadoPostulacion.EnEspera);

            using (var datos = CrearContexto())
            {
                datos.Publicaciones.Single(p => p.Id == _publicacionId).Estado = EstadoPublicacion.Pausada;
                datos.SaveChanges();
            }

            using var contexto = CrearContexto();
            var servicio = new PostulacionService(contexto, _configuracion);

            var resultado = await servicio.RechazarAsync(_publicacionId, aceptadaId, null, _refugioId);

            Assert.Null(resultado.Error);
            Assert.Equal(EstadoPostulacion.Rechazada, LeerPostulacion(aceptadaId).Estado);
            Assert.Equal(EstadoPostulacion.Pendiente, LeerPostulacion(enEsperaId).Estado);
            Assert.Equal(EstadoPublicacion.Activa, LeerPublicacion(_publicacionId).Estado);
        }

        /// <summary>
        /// No se puede rechazar una retirada , da error
        /// </summary>
        /// <returns></returns>

        [Fact]
        public async Task Rechazar_Retirada_DaError()
        {
            var postulacionId = CargarPostulacion(_publicacionId, _anaId, EstadoPostulacion.Retirada);

            using var contexto = CrearContexto();
            var servicio = new PostulacionService(contexto, _configuracion);

            var resultado = await servicio.RechazarAsync(_publicacionId, postulacionId, null, _refugioId);

            Assert.Equal("Esa postulacion ya estaba resuelta.", resultado.Error);
        }

        /// <summary>
        /// Obtener una consulta o postulacion de otro usuario da null
        /// </summary>
        /// <returns></returns>

        [Fact]
        public async Task Obtener_DeOtroUsuario_DaNull()
        {
            var postulacionId = CargarPostulacion(_publicacionId, _anaId, EstadoPostulacion.Pendiente);
            var otroId = CargarUsuario("otro@test.org", "Carlos");

            using var contexto = CrearContexto();
            var servicio = new PostulacionService(contexto, _configuracion);

            var postulacion = await servicio.ObtenerPostulacionAsync(postulacionId, otroId);

            Assert.Null(postulacion);
        }

        /// <summary>
        /// Si el motivo del rechazo es una observacion, reemplaza la default y muestra la escrita
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task MotivoRechazo_ConObservacion_MuestraLaDelRefugio()
        {
            var postulacionId = CargarPostulacion(_publicacionId, _anaId, EstadoPostulacion.Rechazada, "No tiene patio");

            using var contexto = CrearContexto();
            var servicio = new PostulacionService(contexto, _configuracion);

            var postulacion = await servicio.ObtenerPostulacionAsync(postulacionId, _anaId);

            Assert.Equal("No tiene patio", postulacion!.MotivoRechazo);
        }


        /// <summary>
        /// Si se rechaza sin motivo, se muestra la leyenda puesta  por el refugio
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task MotivoRechazo_SinObservacion_MuestraElMensajeAutomaticoDelRefugio()
        {
            using (var datos = CrearContexto())
            {
                datos.Refugios.Single(r => r.Id == _refugioId).MensajeRechazoAutomatico = "Gracias por postularte";
                datos.SaveChanges();
            }

            var postulacionId = CargarPostulacion(_publicacionId, _anaId, EstadoPostulacion.Rechazada);

            using var contexto = CrearContexto();
            var servicio = new PostulacionService(contexto, _configuracion);

            var postulacion = await servicio.ObtenerPostulacionAsync(postulacionId, _anaId);

            Assert.Equal("Gracias por postularte", postulacion!.MotivoRechazo);
        }

        /// <summary>
        /// Si se rechaza sin motivo, se muestra la leyenda default del programa
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task MotivoRechazo_SinNingunTexto_MuestraLaLeyendaPorDefecto()
        {
            var postulacionId = CargarPostulacion(_publicacionId, _anaId, EstadoPostulacion.Rechazada);

            using var contexto = CrearContexto();
            var servicio = new PostulacionService(contexto, _configuracion);

            var postulacion = await servicio.ObtenerPostulacionAsync(postulacionId, _anaId);

            Assert.Equal("El refugio selecciono a otro candidato para esta publicacion.", postulacion!.MotivoRechazo);
        }

        /// <summary>
        /// Listar la publciacion del refugio, trae datos del postulante para armar la ficha
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ListarPublicacion_DelRefugio_TraeLosDatosDelPostulante()
        {
            CargarPostulacion(_publicacionId, _anaId, EstadoPostulacion.Pendiente);

            using var contexto = CrearContexto();
            var servicio = new PostulacionService(contexto, _configuracion);

            var postulaciones = await servicio.ListarPostulacionesDePublicacionAsync(_publicacionId, _refugioId);

            var postulacion = Assert.Single(postulaciones);
            Assert.Equal("Ana", postulacion.Nombre);
            Assert.Equal(SeleccionUsuario.Ambos, postulacion.Seleccion);
        }


        /// <summary>
        /// Si se lista la publicaicon de otro refugio da vacio 
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ListarPublicacion_DeOtroRefugio_DevuelveVacio()
        {
            CargarPostulacion(_publicacionId, _anaId, EstadoPostulacion.Pendiente);

            using var contexto = CrearContexto();
            var servicio = new PostulacionService(contexto, _configuracion);

            var postulaciones = await servicio.ListarPostulacionesDePublicacionAsync(_publicacionId, Guid.NewGuid());

            Assert.Empty(postulaciones);
        }


        /// <summary>
        /// Postularse sin usuario da Error
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Postularse_SinUsuario_DaError()
        {
            using var contexto = CrearContexto();
            var servicio = new PostulacionService(contexto, _configuracion);

            var resultado = await servicio.PostularseAsync(new PostulacionRequest { PublicacionId = _publicacionId }, Guid.Empty);

            Assert.Equal("No se recibieron los datos de la postulacion, intentelo de nuevo mas tarde.", resultado.Error);
        }

        /// <summary>
        /// Retirar una postulacion en espera queda retirada ok
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Retirar_EnEspera_QuedaRetirada()
        {
            var postulacionId = CargarPostulacion(_publicacionId, _anaId, EstadoPostulacion.EnEspera);

            using var contexto = CrearContexto();
            var servicio = new PostulacionService(contexto, _configuracion);

            var resultado = await servicio.RetirarPostulacionAsync(postulacionId, _anaId);

            Assert.Null(resultado.Error);
            Assert.Equal(EstadoPostulacion.Retirada, LeerPostulacion(postulacionId).Estado);
        }

        /// <summary>
        /// Aceptar la postulaicon de otra publicacion da error y no cambia
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Aceptar_PostulacionDeOtraPublicacion_DaErrorYNoCambia()
        {
            var postulacionId = CargarPostulacion(CargarPublicacion(), _anaId, EstadoPostulacion.Pendiente);

            using var contexto = CrearContexto();
            var servicio = new PostulacionService(contexto, _configuracion);

            var resultado = await servicio.AceptarAsync(_publicacionId, postulacionId, _refugioId);

            Assert.Equal("Esa postulacion no pertenece a esta publicacion.", resultado.Error);
            Assert.Equal(EstadoPostulacion.Pendiente, LeerPostulacion(postulacionId).Estado);
        }

        /// <summary>
        /// Aceptar una retirada da error y no cambia
        /// </summary>
        /// <returns></returns>

        [Fact]
        public async Task Aceptar_Retirada_DaErrorYNoCambia()
        {
            var postulacionId = CargarPostulacion(_publicacionId, _anaId, EstadoPostulacion.Retirada);

            using var contexto = CrearContexto();
            var servicio = new PostulacionService(contexto, _configuracion);

            var resultado = await servicio.AceptarAsync(_publicacionId, postulacionId, _refugioId);

            Assert.Equal("Solo se pueden aceptar postulaciones pendientes.", resultado.Error);
            Assert.Equal(EstadoPostulacion.Retirada, LeerPostulacion(postulacionId).Estado);
        }

        /// <summary>
        /// Rechazar la postualcion de otro refugio
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Rechazar_DeOtroRefugio_DaErrorYNoCambia()
        {
            var postulacionId = CargarPostulacion(_publicacionId, _anaId, EstadoPostulacion.Pendiente);

            using var contexto = CrearContexto();
            var servicio = new PostulacionService(contexto, _configuracion);

            var resultado = await servicio.RechazarAsync(_publicacionId, postulacionId, null, Guid.NewGuid());

            Assert.Equal("No encontramos esa publicacion.", resultado.Error);
            Assert.Equal(EstadoPostulacion.Pendiente, LeerPostulacion(postulacionId).Estado);
        }

        /// <summary>cambia
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Rechazar_PostulacionDeOtraPublicacion_DaErrorYNoCambia()
        {
            var postulacionId = CargarPostulacion(CargarPublicacion(), _anaId, EstadoPostulacion.Pendiente);

            using var contexto = CrearContexto();
            var servicio = new PostulacionService(contexto, _configuracion);

            var resultado = await servicio.RechazarAsync(_publicacionId, postulacionId, null, _refugioId);

            Assert.Equal("Esa postulacion no pertenece a esta publicacion.", resultado.Error);
            Assert.Equal(EstadoPostulacion.Pendiente, LeerPostulacion(postulacionId).Estado);
        }

        /// <summary>
        /// Listar Mis postulacion que tiene varios usuarios solo trae las postualdas 
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ListarMisPostulaciones_ConVariosUsuarios_TraeSoloLasPropias()
        {
            var propiaId = CargarPostulacion(_publicacionId, _anaId, EstadoPostulacion.Pendiente);
            CargarPostulacion(_publicacionId, CargarUsuario("otro@test.org", "Carlos"), EstadoPostulacion.Pendiente);

            using var contexto = CrearContexto();
            var servicio = new PostulacionService(contexto, _configuracion);

            var postulaciones = await servicio.ListarMisPostulacionesAsync(_anaId);

            var postulacion = Assert.Single(postulaciones);
            Assert.Equal(propiaId, postulacion.Id);
        }

        /// <summary>
        /// Listar la publicaicon de un posutlante con contactos (redes) trae todas sus redes cargadas
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ListarDePublicacion_PostulanteConContactos_DevuelveSusContactos()
        {
            CargarPostulacion(_publicacionId, _anaId, EstadoPostulacion.Pendiente);

            using (var datos = CrearContexto())
            {
                var perfil = datos.PerfilPostulantes.Single(p => p.UsuarioId == _anaId);
                datos.Add(new ContactoPostulante { Id = Guid.NewGuid(), PerfilPostulanteId = perfil.Id, Tipo = TipoContacto.Instagram, Url = "instagram.com/ana" });
                datos.SaveChanges();
            }

            using var contexto = CrearContexto();
            var servicio = new PostulacionService(contexto, _configuracion);

            var postulaciones = await servicio.ListarPostulacionesDePublicacionAsync(_publicacionId, _refugioId);

            var contacto = Assert.Single(Assert.Single(postulaciones).Contactos);
            Assert.Equal("instagram.com/ana", contacto.Url);
        }

    }
}
