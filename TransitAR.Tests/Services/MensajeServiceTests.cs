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
    public class MensajeServiceTests: BaseEnMemoria
    {

        //guids
        private readonly Guid _refugioId = Guid.NewGuid();
        private readonly Guid _especieId = Guid.NewGuid();
        private readonly Guid _condicionId = Guid.NewGuid();
        private readonly Guid _mascotaId = Guid.NewGuid();
        private readonly Guid _publicacionId = Guid.NewGuid();
        private readonly Guid _lauraId = Guid.NewGuid();
        private readonly Guid _anaId = Guid.NewGuid();
        private readonly Guid _postulacionId = Guid.NewGuid();


        //config en memoria,  el limite de adjuntos baja a 1 MB  - Docuemntacion de Microsoft
        private readonly IConfiguration _configuracion = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Reglas:AdjuntoMaxMB"] = "1" })
            .Build();

        //Creo el nuevo contexto
        public MensajeServiceTests()
        {
            using var contexto = CrearContexto();

            contexto.Refugios.Add(new Refugio { Id = _refugioId, Nombre = "Patitas", Email = "patitas@test.org", Localidad = "Lomas", Activo = true });
            contexto.Especies.Add(new Especie { Id = _especieId, Nombre = "Especie de prueba" });
            contexto.Condiciones.Add(new Condicion { Id = _condicionId, Nombre = "Condicion de prueba" });
            contexto.Mascotas.Add(new Mascota { Id = _mascotaId, Nombre = "Michi", RefugioId = _refugioId, EspecieId = _especieId, CondicionId = _condicionId });
            contexto.Publicaciones.Add(NuevaPublicacion(_publicacionId, _mascotaId));
            contexto.Usuarios.Add(NuevoUsuario(_lauraId, "Laura"));
            contexto.Usuarios.Add(NuevoUsuario(_anaId, "Ana"));
            contexto.Postulaciones.Add(new Postulacion { Id = _postulacionId, PublicacionId = _publicacionId, UsuarioId = _anaId, Estado = EstadoPostulacion.Pendiente });
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

        private static Publicacion NuevaPublicacion(Guid id, Guid mascotaId) => new()
        {
            Id = id,
            MascotaId = mascotaId,
            Titulo = "Busca hogar",
            Descripcion = "Descripcion de prueba",
            Ubicacion = "Lomas",
            Tipo = TipoPublicacion.Adopcion,
            Estado = EstadoPublicacion.Activa
        };

        private Guid CargarUsuario(string nombre)
        {
            using var contexto = CrearContexto();

            var id = Guid.NewGuid();
            contexto.Usuarios.Add(NuevoUsuario(id, nombre));
            contexto.SaveChanges();

            return id;
        }

        private Guid CargarPublicacion(Guid? refugioId = null)
        {
            using var contexto = CrearContexto();

            var mascotaId = Guid.NewGuid();
            var publicacionId = Guid.NewGuid();
            contexto.Mascotas.Add(new Mascota { Id = mascotaId, Nombre = "Toto", RefugioId = refugioId ?? _refugioId, EspecieId = _especieId, CondicionId = _condicionId });
            contexto.Publicaciones.Add(NuevaPublicacion(publicacionId, mascotaId));
            contexto.SaveChanges();

            return publicacionId;
        }

        private Guid CargarRefugio()
        {
            using var contexto = CrearContexto();

            var id = Guid.NewGuid();
            contexto.Refugios.Add(new Refugio { Id = id, Nombre = "Refugio del Sur", Email = "sur@test.org", Localidad = "Adrogue", Activo = true });
            contexto.SaveChanges();

            return id;
        }

        private Guid CargarPostulacion(Guid publicacionId, Guid usuarioId, EstadoPostulacion estado = EstadoPostulacion.Pendiente)
        {
            using var contexto = CrearContexto();

            var id = Guid.NewGuid();
            contexto.Postulaciones.Add(new Postulacion { Id = id, PublicacionId = publicacionId, UsuarioId = usuarioId, Estado = estado });
            contexto.SaveChanges();

            return id;
        }

        private void CargarEntrega(DateTime? fechaFinReal = null)
        {
            using var contexto = CrearContexto();

            contexto.Postulaciones.Single(p => p.Id == _postulacionId).Estado = EstadoPostulacion.Aceptada;
            contexto.Tenencias.Add(new Tenencia { Id = Guid.NewGuid(), PostulacionId = _postulacionId, MascotaId = _mascotaId, Modalidad = TipoPublicacion.Transito, FechaInicio = DateTime.UtcNow.AddDays(-30), FechaFinReal = fechaFinReal });
            contexto.SaveChanges();
        }

        private Guid CargarMensaje(Guid? postulacionId = null, Guid? emisorId = null, string texto = "Hola", DateTime? fecha = null)
        {
            using var contexto = CrearContexto();

            var id = Guid.NewGuid();
            contexto.Mensajes.Add(new Mensaje { Id = id, PostulacionId = postulacionId ?? _postulacionId, EmisorId = emisorId ?? _lauraId, Texto = texto, FechaEnvio = fecha ?? DateTime.UtcNow });
            contexto.SaveChanges();

            return id;
        }

        private Guid CargarAdjunto(Guid mensajeId)
        {
            using var contexto = CrearContexto();

            var id = Guid.NewGuid();
            contexto.AdjuntosMensaje.Add(new AdjuntoMensaje { Id = id, MensajeId = mensajeId, NombreArchivo = "michi.jpg", ContentType = "image/jpeg", TamanioBytes = 3, Contenido = new byte[] { 1, 2, 3 } });
            contexto.SaveChanges();

            return id;
        }

        private static ArchivoAdjunto NuevoArchivo(int bytes = 3, string tipo = "image/jpeg", string nombre = "foto.jpg") => new()
        {
            NombreArchivo = nombre,
            ContentType = tipo,
            Contenido = new byte[bytes]
        };

        //Lectura con el contexto nuevo, para ver lo que quedo guardado
        private Mensaje LeerMensaje(Guid id)
        {
            using var contexto = CrearContexto();

            return contexto.Mensajes.Single(m => m.Id == id);
        }

        private List<Mensaje> LeerMensajes()
        {
            using var contexto = CrearContexto();

            return contexto.Mensajes.ToList();
        }

        private List<AdjuntoMensaje> LeerAdjuntos()
        {
            using var contexto = CrearContexto();

            return contexto.AdjuntosMensaje.ToList();
        }

        //********* PRUBEAS *************

        /// <summary>
        /// No se pueden listar los chats sn el usuario, devuelve lista vacia (POSTULANTE)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ListarChats_SinUsuario_DevuelveVacio()
        {
            using var contexto = CrearContexto();
            var servicio = new MensajeService(contexto, _configuracion);

            var chats = await servicio.ListarChatsAsync(Guid.Empty, null, null, null);

            Assert.Empty(chats);
        }


        /// <summary>
        /// Solo se peuden ver las conversaciones o lsitar las conversaciones propias, solo trae las propias con los refugios correspondientes (POSTULANTE)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ListarChats_ComoPostulante_TraeSoloLosSuyosConElRefugio()
        {
            CargarPostulacion(_publicacionId, CargarUsuario("Carlos"));

            using var contexto = CrearContexto();
            var servicio = new MensajeService(contexto, _configuracion);

            var chats = await servicio.ListarChatsAsync(_anaId, null, null, null);

            var chat = Assert.Single(chats);
            Assert.Equal(_postulacionId, chat.PostulacionId);
            Assert.Equal("Patitas", chat.Receptor);
            Assert.Equal("Michi", chat.MascotaNombre);
        }

        /// <summary>
        /// Listar los chats como refugio, trae todas las conversaciones de las publicaciones, con el nombre del usuario (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ListarChats_ComoRefugio_TraeSoloLosDeSusPublicacionesConLaPersona()
        {
            CargarPostulacion(CargarPublicacion(CargarRefugio()), CargarUsuario("Carlos"));

            using var contexto = CrearContexto();
            var servicio = new MensajeService(contexto, _configuracion);

            var chats = await servicio.ListarChatsAsync(_lauraId, _refugioId, null, null);

            var chat = Assert.Single(chats);
            Assert.Equal(_postulacionId, chat.PostulacionId);
            Assert.Equal("Ana Perez", chat.Receptor);
        }

        /// <summary>
        /// Filtrar por publciaicon, aparecen solo las conversaciones con las personas de esa publciacion (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ListarChats_PorPublicacion_TraeSoloLosDeEsaPublicacion()
        {
            CargarPostulacion(CargarPublicacion(), CargarUsuario("Carlos"));

            using var contexto = CrearContexto();
            var servicio = new MensajeService(contexto, _configuracion);

            var chats = await servicio.ListarChatsAsync(_lauraId, _refugioId, _publicacionId, null);

            Assert.Equal(_postulacionId, Assert.Single(chats).PostulacionId);
        }


        /// <summary>
        /// Filtrar por mascota, trae solo los mensajes con las personas sobre ese animal (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ListarChats_PorMascota_TraeSoloLosDeEsaMascota()
        {
            CargarPostulacion(CargarPublicacion(), CargarUsuario("Carlos"));

            using var contexto = CrearContexto();
            var servicio = new MensajeService(contexto, _configuracion);

            var chats = await servicio.ListarChatsAsync(_lauraId, _refugioId, null, _mascotaId);

            Assert.Equal(_postulacionId, Assert.Single(chats).PostulacionId);
        }

        /// <summary>
        /// Listar chats, vienen ordenados por el ultimo mensaje mas nuevo hasta las que no tienen mensajes (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ListarChats_VariosChats_LosOrdenaPorElUltimoMensaje()
        {
            var sinMensajesId = CargarPostulacion(CargarPublicacion(), CargarUsuario("Carlos"));
            var recienteId = CargarPostulacion(CargarPublicacion(), CargarUsuario("Sofia"));
            CargarMensaje(_postulacionId, fecha: DateTime.UtcNow.AddDays(-2));
            CargarMensaje(recienteId, fecha: DateTime.UtcNow);

            using var contexto = CrearContexto();
            var servicio = new MensajeService(contexto, _configuracion);

            var chats = await servicio.ListarChatsAsync(_lauraId, _refugioId, null, null);

            Assert.Equal(new[] { recienteId, _postulacionId, sinMensajesId }, chats.Select(c => c.PostulacionId));
        }

        /// <summary>
        /// Listar chat con mensajes muestra el ultimo mensaje ne la preview o adjunto y muestra el contador de no leidos (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ListarChats_ConMensajes_MuestraElUltimoYCuentaLosNoLeidos()
        {
            CargarMensaje(texto: "Primero", fecha: DateTime.UtcNow.AddMinutes(-10));
            CargarMensaje(emisorId: _anaId, texto: "Respuesta de Ana", fecha: DateTime.UtcNow.AddMinutes(-5));
            CargarAdjunto(CargarMensaje(texto: "Te mando una foto", fecha: DateTime.UtcNow));

            using var contexto = CrearContexto();
            var servicio = new MensajeService(contexto, _configuracion);

            var chats = await servicio.ListarChatsAsync(_anaId, null, null, null);

            var chat = Assert.Single(chats);
            Assert.Equal("Te mando una foto", chat.UltimoMensaje);
            Assert.True(chat.UltimoTieneAdjunto);
            Assert.Equal(2, chat.NoLeidos);
        }


        /// <summary>
        /// Obtener el chat , abrirlo, masrca como leidos los mensajes que se recibieron. Los que fueron mandados indican sin leer hasta qu elo vea la otra personas (POSTULANTE)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ObtenerChat_ComoPostulante_MarcaLeidosSoloLosQueRecibio()
        {
            var deLauraId = CargarMensaje();
            var deAnaId = CargarMensaje(emisorId: _anaId);

            using var contexto = CrearContexto();
            var servicio = new MensajeService(contexto, _configuracion);

            await servicio.ObtenerChatAsync(_postulacionId, _anaId, null);

            Assert.NotNull(LeerMensaje(deLauraId).FechaLectura);
            Assert.Null(LeerMensaje(deAnaId).FechaLectura);
        }

        /// <summary>
        /// El chat al obtenerse trae del mas viejo al neuvo y quien los mando, si esta la publciaicon abierta s epeude escribir (POSTULANTE)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ObtenerChat_ConMensajes_LosTraeEnOrdenConElEmisor()
        {
            CargarMensaje(emisorId: _anaId, texto: "Segundo", fecha: DateTime.UtcNow);
            CargarMensaje(texto: "Primero", fecha: DateTime.UtcNow.AddMinutes(-5));

            using var contexto = CrearContexto();
            var servicio = new MensajeService(contexto, _configuracion);

            var chat = await servicio.ObtenerChatAsync(_postulacionId, _anaId, null);

            Assert.True(chat!.PuedeEscribir);
            Assert.Equal(new[] { "Primero", "Segundo" }, chat.Mensajes.Select(m => m.Texto));
            Assert.Equal("Laura Perez", chat.Mensajes[0].EmisorNombre);
            Assert.False(chat.Mensajes[0].EsPropio);
            Assert.True(chat.Mensajes[1].EsPropio);
        }

        /// <summary>
        /// Obtener chat de otro postulante da nuell y los mensajes continuan sin leer (POSTULANTE)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ObtenerChat_DeOtroPostulante_DaNullYNoMarcaLeidos()
        {
            var mensajeId = CargarMensaje();
            var carlosId = CargarUsuario("Carlos");

            using var contexto = CrearContexto();
            var servicio = new MensajeService(contexto, _configuracion);

            var chat = await servicio.ObtenerChatAsync(_postulacionId, carlosId, null);

            Assert.Null(chat);
            Assert.Null(LeerMensaje(mensajeId).FechaLectura);
        }

        /// <summary>
        /// Obtener chat de otro refugio da null y los mensajes quedan sin leer (POSTULANTE)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ObtenerChat_DeOtroRefugio_DaNull()
        {
            using var contexto = CrearContexto();
            var servicio = new MensajeService(contexto, _configuracion);

            var chat = await servicio.ObtenerChatAsync(_postulacionId, _lauraId, Guid.NewGuid());

            Assert.Null(chat);
        }

        /// <summary>
        /// Obtener un chat sin usuario da null, no se abre ningun chat (POSTULANTE)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ObtenerChat_SinUsuario_DaNull()
        {
            using var contexto = CrearContexto();
            var servicio = new MensajeService(contexto, _configuracion);

            var chat = await servicio.ObtenerChatAsync(_postulacionId, Guid.Empty, null);

            Assert.Null(chat);
        }

        /// <summary>
        /// Enviar un mensaje como postulante , guarda el texto sin espcaios (trim) + emisor y lo marca sin leer (POSTULANTE)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Enviar_ComoPostulante_LoGuardaSinEspaciosYSinLeer()
        {
            using var contexto = CrearContexto();
            var servicio = new MensajeService(contexto, _configuracion);

            var resultado = await servicio.EnviarAsync(_postulacionId, new MensajeRequest { Texto = "  Hola, sigue disponible?  " }, new List<ArchivoAdjunto>(), _anaId, null);

            var guardado = Assert.Single(LeerMensajes());
            Assert.Null(resultado.Error);
            Assert.Equal("Hola, sigue disponible?", guardado.Texto);
            Assert.Equal(_anaId, guardado.EmisorId);
            Assert.Null(guardado.FechaLectura);
            Assert.Equal("Ana Perez", resultado.Mensaje!.EmisorNombre);
        }

        /// <summary>
        /// Enviar un mensaje como refugio, o contestar como este , guarda el emisor usuario-refugio que lo envio (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Enviar_ComoRefugio_LoGuardaConQuienEscribio()
        {
            using var contexto = CrearContexto();
            var servicio = new MensajeService(contexto, _configuracion);

            var resultado = await servicio.EnviarAsync(_postulacionId, new MensajeRequest { Texto = "Si, esta disponible" }, new List<ArchivoAdjunto>(), _lauraId, _refugioId);

            Assert.Null(resultado.Error);
            Assert.Equal(_lauraId, Assert.Single(LeerMensajes()).EmisorId);
        }


        /// <summary>
        /// Enviar adjuntos los guarda con el mensaje, tamaño y el tipo normalizado (trim) (POSTUALNTE)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Enviar_ConAdjuntos_LosGuardaConSuTamanio()
        {
            using var contexto = CrearContexto();
            var servicio = new MensajeService(contexto, _configuracion);

            var resultado = await servicio.EnviarAsync(_postulacionId, new MensajeRequest(), new List<ArchivoAdjunto> { NuevoArchivo(5, " IMAGE/PNG ", " michi.png ") }, _anaId, null);

            var adjunto = Assert.Single(LeerAdjuntos());
            Assert.Null(resultado.Error);
            Assert.Equal("michi.png", adjunto.NombreArchivo);
            Assert.Equal("image/png", adjunto.ContentType);
            Assert.Equal(5, adjunto.TamanioBytes);
        }


        /// <summary>
        /// Sin datos del mensaje da Errror (Postulante)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Enviar_SinDatos_DaError()
        {
            using var contexto = CrearContexto();
            var servicio = new MensajeService(contexto, _configuracion);

            var resultado = await servicio.EnviarAsync(_postulacionId, null!, new List<ArchivoAdjunto>(), _anaId, null);

            Assert.Equal("No se recibieron los datos del mensaje.", resultado.Error);
        }


        /// <summary>
        /// Otro postulabte no puede escribri en la conversacion entre refugio-postulante. No se guarda (POSTULANTE)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Enviar_DeOtroPostulante_DaErrorYNoGuarda()
        {
            var carlosId = CargarUsuario("Carlos");

            using var contexto = CrearContexto();
            var servicio = new MensajeService(contexto, _configuracion);

            var resultado = await servicio.EnviarAsync(_postulacionId, new MensajeRequest { Texto = "Hola" }, new List<ArchivoAdjunto>(), carlosId, null);

            Assert.Equal("No encontramos esa conversacion.", resultado.Error);
            Assert.Empty(LeerMensajes());
        }

        /// <summary>
        /// Otro refugio no puede escribir en conversaciones entre postulante y otro refugio, da error y no guarda (POSTULANTE)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Enviar_DeOtroRefugio_DaErrorYNoGuarda()
        {
            using var contexto = CrearContexto();
            var servicio = new MensajeService(contexto, _configuracion);

            var resultado = await servicio.EnviarAsync(_postulacionId, new MensajeRequest { Texto = "Hola" }, new List<ArchivoAdjunto>(), _lauraId, Guid.NewGuid());

            Assert.Equal("No encontramos esa conversacion.", resultado.Error);
            Assert.Empty(LeerMensajes());
        }

        /// <summary>
        /// COn la postulacion rechazada el chat queda solo lectura, no s epeude mandar msj (POSTULANTE)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Enviar_PostulacionRechazada_DaErrorYNoGuarda()
        {
            var rechazadaId = CargarPostulacion(CargarPublicacion(), _anaId, EstadoPostulacion.Rechazada);

            using var contexto = CrearContexto();
            var servicio = new MensajeService(contexto, _configuracion);

            var resultado = await servicio.EnviarAsync(rechazadaId, new MensajeRequest { Texto = "Hola" }, new List<ArchivoAdjunto>(), _anaId, null);

            Assert.Equal("Esta conversacion esta cerrada. Podes leerla pero ya no se pueden enviar mensajes.", resultado.Error);
            Assert.Empty(LeerMensajes());
        }


        /// <summary>
        /// Enviar mensjae con mascota entregada  se puede, el chat sigue abierto, para coordinar seguimientos ,etc (POSTULANTE)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Enviar_ConLaMascotaEntregada_LoGuarda()
        {
            CargarEntrega();

            using var contexto = CrearContexto();
            var servicio = new MensajeService(contexto, _configuracion);

            var resultado = await servicio.EnviarAsync(_postulacionId, new MensajeRequest { Texto = "Michi esta muy bien" }, new List<ArchivoAdjunto>(), _anaId, null);

            Assert.Null(resultado.Error);
            Assert.Single(LeerMensajes());
        }


        /// <summary>
        /// Enviar con una tenencia teerminada (transito que volvio) da error y no guarda el msj , queda solo lectura (POSTULANTE)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Enviar_TenenciaTerminada_DaErrorYNoGuarda()
        {
            CargarEntrega(fechaFinReal: DateTime.UtcNow);

            using var contexto = CrearContexto();
            var servicio = new MensajeService(contexto, _configuracion);

            var resultado = await servicio.EnviarAsync(_postulacionId, new MensajeRequest { Texto = "Hola" }, new List<ArchivoAdjunto>(), _anaId, null);

            Assert.Equal("Esta conversacion esta cerrada. Podes leerla pero ya no se pueden enviar mensajes.", resultado.Error);
            Assert.Empty(LeerMensajes());
        }

        /// <summary>
        /// Se pueden adjuntar maximo 3 archvos juntos (limite de EFCORE) , con mas da error y no guarda, en la web muestra el error (POSTULANTE)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Enviar_ConMasDeTresAdjuntos_DaErrorYNoGuarda()
        {
            var archivos = new List<ArchivoAdjunto> { NuevoArchivo(), NuevoArchivo(), NuevoArchivo(), NuevoArchivo() };

            using var contexto = CrearContexto();
            var servicio = new MensajeService(contexto, _configuracion);

            var resultado = await servicio.EnviarAsync(_postulacionId, new MensajeRequest { Texto = "Fotos" }, archivos, _anaId, null);

            Assert.Equal("Se pueden adjuntar hasta 3 archivos por mensaje.", resultado.Error);
            Assert.Empty(LeerMensajes());
        }

        /// <summary>
        /// Enviar un archivo mas grande que el limite da error y no guarda (LIMITE EFCORE, en prueba 1MB) (POSTULANTE)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Enviar_AdjuntoMasGrandeQueElLimite_DaErrorYNoGuarda()
        {
            var grande = NuevoArchivo(1024 * 1024 + 1, nombre: "video.mp4", tipo: "video/mp4");

            using var contexto = CrearContexto();
            var servicio = new MensajeService(contexto, _configuracion);

            var resultado = await servicio.EnviarAsync(_postulacionId, new MensajeRequest(), new List<ArchivoAdjunto> { grande }, _anaId, null);

            Assert.Equal("El archivo video.mp4 supera los 1 MB permitidos.", resultado.Error);
            Assert.Empty(LeerMensajes());
        }


        /// <summary>
        /// Enviar archivo adjunto vacio da error (POSTULANTE)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Enviar_AdjuntoVacio_DaError()
        {
            using var contexto = CrearContexto();
            var servicio = new MensajeService(contexto, _configuracion);

            var resultado = await servicio.EnviarAsync(_postulacionId, new MensajeRequest(), new List<ArchivoAdjunto> { NuevoArchivo(0) }, _anaId, null);

            Assert.Equal("El archivo foto.jpg esta vacio.", resultado.Error);
        }

        /// <summary>
        /// Enviar adjunto de otro tipo que el acetado da error , imagenes y videos mp4 (POSTULANTE)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Enviar_AdjuntoDeOtroTipo_DaError()
        {
            using var contexto = CrearContexto();
            var servicio = new MensajeService(contexto, _configuracion);

            var resultado = await servicio.EnviarAsync(_postulacionId, new MensajeRequest(), new List<ArchivoAdjunto> { NuevoArchivo(tipo: "application/pdf", nombre: "dni.pdf") }, _anaId, null);

            Assert.Equal("Solo se pueden adjuntar imagenes (jpg, png, webp) o videos mp4.", resultado.Error);
        }


        /// <summary>
        /// Obtener adjunto permite ver el adjunto tanto de postulante como refugio
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ObtenerAdjunto_ComoParteDelChat_LoDevuelve()
        {
            var adjuntoId = CargarAdjunto(CargarMensaje());

            using var contexto = CrearContexto();
            var servicio = new MensajeService(contexto, _configuracion);

            var archivo = await servicio.ObtenerAdjuntoAsync(adjuntoId, _anaId, null);

            Assert.Equal("michi.jpg", archivo!.NombreArchivo);
            Assert.Equal(new byte[] { 1, 2, 3 }, archivo.Contenido);
        }


        /// <summary>
        /// Obtener adjunto de otro postulante (conversacion ajena) da null, aunque tenga id (POSTULANTE)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ObtenerAdjunto_DeOtroPostulante_DaNull()
        {
            var adjuntoId = CargarAdjunto(CargarMensaje());
            var carlosId = CargarUsuario("Carlos");

            using var contexto = CrearContexto();
            var servicio = new MensajeService(contexto, _configuracion);

            var archivo = await servicio.ObtenerAdjuntoAsync(adjuntoId, carlosId, null);

            Assert.Null(archivo);
        }


        /// <summary>
        /// Otro refugio tampoco puede obtener el adjunto de otro, da null (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ObtenerAdjunto_DeOtroRefugio_DaNull()
        {
            var adjuntoId = CargarAdjunto(CargarMensaje());

            using var contexto = CrearContexto();
            var servicio = new MensajeService(contexto, _configuracion);

            var archivo = await servicio.ObtenerAdjuntoAsync(adjuntoId, _lauraId, Guid.NewGuid());

            Assert.Null(archivo);
        }


        /// <summary>
        /// Obtener adjunto sin usuario da null,no se peude entregar sin el usuario (POSTULANTE)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ObtenerAdjunto_SinUsuario_DaNull()
        {
            var adjuntoId = CargarAdjunto(CargarMensaje());

            using var contexto = CrearContexto();
            var servicio = new MensajeService(contexto, _configuracion);

            var archivo = await servicio.ObtenerAdjuntoAsync(adjuntoId, Guid.Empty, null);

            Assert.Null(archivo);
        }


        /// <summary>
        /// Obtener adjunto que no existe da null (POSTULANTE)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ObtenerAdjunto_Inexistente_DaNull()
        {
            using var contexto = CrearContexto();
            var servicio = new MensajeService(contexto, _configuracion);

            var archivo = await servicio.ObtenerAdjuntoAsync(Guid.NewGuid(), _anaId, null);

            Assert.Null(archivo);
        }
    }
}
