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
    public class MascotaServiceTests:BaseEnMemoria
    {
        //guids
        private readonly Guid _refugioId = Guid.NewGuid();
        private readonly Guid _especieId = Guid.NewGuid();
        private readonly Guid _condicionId = Guid.NewGuid();
        private readonly Guid _mascotaId = Guid.NewGuid();
        private readonly Guid _anaId = Guid.NewGuid();
        private readonly DateTime _fechaAlta = new DateTime(2026, 1, 10);

        //Creo el nuevo contexto
        public MascotaServiceTests()
        {
            using var contexto = CrearContexto();

            contexto.Refugios.Add(new Refugio { Id = _refugioId, Nombre = "Patitas", Email = "patitas@test.org", Localidad = "Lomas", Activo = true });
            contexto.Especies.Add(new Especie { Id = _especieId, Nombre = "Especie de prueba" });
            contexto.Condiciones.Add(new Condicion { Id = _condicionId, Nombre = "Condicion de prueba" });
            contexto.Mascotas.Add(NuevaMascota(_mascotaId, "Michi", _refugioId));
            contexto.Usuarios.Add(NuevoUsuario(_anaId, "Ana"));
            contexto.SaveChanges();
        }

        //Datos

        private Mascota NuevaMascota(Guid id, string nombre, Guid refugioId, EstadoMascota estado = EstadoMascota.EnRefugio) => new()
        {
            Id = id,
            Nombre = nombre,
            RefugioId = refugioId,
            EspecieId = _especieId,
            CondicionId = _condicionId,
            Estado = estado,
            FechaAlta = _fechaAlta
        };

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

        private MascotaRequest NuevoRequest(string nombre = "Luna") => new()
        {
            Nombre = nombre,
            EspecieId = _especieId,
            CondicionId = _condicionId,
            Raza = "Mestiza",
            Sexo = Sexo.Hembra,
            Tamanio = Tamanio.Chico,
            Vacunado = true
        };

        private Guid CargarMascota(string nombre, Guid? refugioId = null, EstadoMascota estado = EstadoMascota.EnRefugio, DateTime? nacimiento = null)
        {
            using var contexto = CrearContexto();

            var id = Guid.NewGuid();
            var mascota = NuevaMascota(id, nombre, refugioId ?? _refugioId, estado);
            mascota.FechaNacimientoAproximada = nacimiento;
            contexto.Mascotas.Add(mascota);
            contexto.SaveChanges();

            return id;
        }

        private Guid CargarRefugio()
        {
            using var contexto = CrearContexto();

            var id = Guid.NewGuid();
            contexto.Refugios.Add(new Refugio { Id = id, Nombre = "Refugio del Sur", Email = "sur@test.org", Localidad = "Adrogue", Activo = true });
            contexto.SaveChanges();

            return id;
        }

        private Guid CargarUsuario(string nombre)
        {
            using var contexto = CrearContexto();

            var id = Guid.NewGuid();
            contexto.Usuarios.Add(NuevoUsuario(id, nombre));
            contexto.SaveChanges();

            return id;
        }

        private Guid CargarPublicacion(Guid mascotaId)
        {
            using var contexto = CrearContexto();

            var id = Guid.NewGuid();
            contexto.Publicaciones.Add(new Publicacion { Id = id, MascotaId = mascotaId, Titulo = "Busca hogar", Descripcion = "Descripcion de prueba", Ubicacion = "Lomas", Tipo = TipoPublicacion.Transito, Estado = EstadoPublicacion.Activa });
            contexto.SaveChanges();

            return id;
        }

        //la postulacion va en una publicacion y la tenencia apunta a la mascota que se pase
        private Guid CargarTenencia(Guid mascotaId, Guid usuarioId, Guid? publicacionId = null)
        {
            var postulacionId = Guid.NewGuid();
            var publicacion = publicacionId ?? CargarPublicacion(mascotaId);

            using var contexto = CrearContexto();

            var id = Guid.NewGuid();
            contexto.Postulaciones.Add(new Postulacion { Id = postulacionId, PublicacionId = publicacion, UsuarioId = usuarioId, Estado = EstadoPostulacion.Aceptada });
            contexto.Tenencias.Add(new Tenencia { Id = id, PostulacionId = postulacionId, MascotaId = mascotaId, Modalidad = TipoPublicacion.Transito, FechaInicio = DateTime.UtcNow.AddDays(-10) });
            contexto.SaveChanges();

            return id;
        }

        //carga las fotos con el orden 0,1,2 y el orden se devuelve con ids en ese orden
        private List<Guid> CargarFotos(Guid mascotaId, int cantidad)
        {
            using var contexto = CrearContexto();

            var ids = new List<Guid>();
            for (var i = 0; i < cantidad; i++)
            {
                var id = Guid.NewGuid();
                contexto.FotosMascota.Add(new FotoMascota { Id = id, MascotaId = mascotaId, Orden = i, ContentType = "image/jpeg", Contenido = new byte[] { (byte)i } });
                ids.Add(id);
            }
            contexto.SaveChanges();

            return ids;
        }

        private static ArchivoAdjunto NuevaFoto(byte contenido = 9) => new()
        {
            NombreArchivo = "foto.jpg",
            ContentType = "image/png",
            Contenido = new byte[] { contenido }
        };

        //Lectura con el contexto nuevo, para ver lo que quedo guardado
        private Mascota? LeerMascota(Guid id)
        {
            using var contexto = CrearContexto();

            return contexto.Mascotas.SingleOrDefault(m => m.Id == id);
        }

        private List<Mascota> LeerMascotas()
        {
            using var contexto = CrearContexto();

            return contexto.Mascotas.ToList();
        }

        //ids de las fotos de la mascota, ordenadas por orden
        private List<FotoMascota> LeerFotos(Guid mascotaId)
        {
            using var contexto = CrearContexto();

            return contexto.FotosMascota.Where(f => f.MascotaId == mascotaId).OrderBy(f => f.Orden).ToList();
        }

        //********* PRUBEAS *************

        /// <summary>
        /// Listar las mascotas de un refugio, trae solo las pertenecientes a ese, ya ordenadas por nombre (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ListarMascotas_DelRefugio_TraeSoloLasSuyasOrdenadasPorNombre()
        {
            CargarMascota("Bruno");
            CargarMascota("Toto", CargarRefugio());

            using var contexto = CrearContexto();
            var servicio = new MascotaService(contexto);

            var mascotas = await servicio.ListarMascotasAsync(_refugioId);

            Assert.Equal(new[] { "Bruno", "Michi" }, mascotas.Select(m => m.Nombre));
        }


        /// <summary>
        /// Listar las mascotas ,trae la mascota que vienen con las fotos y  el orden elegido y la seleccion de la principal (REFGUIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ListarMascotas_ConFotos_TraeLosIdsEnOrden()
        {
            var fotos = CargarFotos(_mascotaId, 3);

            using var contexto = CrearContexto();
            var servicio = new MascotaService(contexto);

            var mascota = Assert.Single(await servicio.ListarMascotasAsync(_refugioId));

            Assert.Equal(fotos, mascota.FotosIds);
        }

        /// <summary>
        /// Obtener una mascota del refugio la trae con su especie y condicion (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ObtenerMascota_DelRefugio_LaTraeConEspecieYCondicion()
        {
            using var contexto = CrearContexto();
            var servicio = new MascotaService(contexto);

            var mascota = await servicio.ObtenerMascotaAsync(_mascotaId, _refugioId);

            Assert.NotNull(mascota);
            Assert.Equal("Michi", mascota.Nombre);
            Assert.Equal("Especie de prueba", mascota.EspecieNombre);
            Assert.Equal("Condicion de prueba", mascota.CondicionNombre);
            Assert.Null(mascota.EdadAproximadaMeses);
        }


        /// <summary>
        /// Obtener la mascota solo puede ser con las de su propio refugio , no con otro , da null aunque se tenga el id (Relacion refugio id y mascota id) (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ObtenerMascota_DeOtroRefugio_DaNull()
        {
            var otroRefugio = CargarRefugio();

            using var contexto = CrearContexto();
            var servicio = new MascotaService(contexto);

            Assert.Null(await servicio.ObtenerMascotaAsync(_mascotaId, otroRefugio));
        }


        /// <summary>
        /// Obtener mascota, trae la mascota junto a la fecha de nacieminto y eso tiene que calcualr la edad aproximada (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ObtenerMascota_ConFechaDeNacimiento_CalculaLaEdad()
        {
            var id = CargarMascota("Cachorro", nacimiento: DateTime.UtcNow.AddDays(-40));

            using var contexto = CrearContexto();
            var servicio = new MascotaService(contexto);

            var mascota = await servicio.ObtenerMascotaAsync(id, _refugioId);

            Assert.NotNull(mascota);
            Assert.Equal(40, mascota.EdadAproximadaDias);
            Assert.Equal(1, mascota.EdadAproximadaMeses);
        }

        /// <summary>
        /// Obtnere trae la masctoa, que el usuario tiene en la tenencia (POSTULANTE)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ObtenerMascotaDeTenencia_DeLaPersona_LaTrae()
        {
            var tenencia = CargarTenencia(_mascotaId, _anaId);

            using var contexto = CrearContexto();
            var servicio = new MascotaService(contexto);

            var mascota = await servicio.ObtenerMascotaDeTenenciaAsync(tenencia, _anaId);

            Assert.NotNull(mascota);
            Assert.Equal(_mascotaId, mascota.Id);
        }


        /// <summary>
        /// Obtneer la mascota de otra persona o postulante da null, y no se puede ya que la tenencia no es suya (POSTULANTE)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ObtenerMascotaDeTenencia_DeOtraPersona_DaNull()
        {
            var tenencia = CargarTenencia(_mascotaId, _anaId);
            var carlos = CargarUsuario("Carlos");

            using var contexto = CrearContexto();
            var servicio = new MascotaService(contexto);

            Assert.Null(await servicio.ObtenerMascotaDeTenenciaAsync(tenencia, carlos));
        }

        /// <summary>
        /// Si la tenencia no existe no se puede obtneer la masctoa (POSTULANTE)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ObtenerMascotaDeTenencia_Inexistente_DaNull()
        {
            using var contexto = CrearContexto();
            var servicio = new MascotaService(contexto);

            Assert.Null(await servicio.ObtenerMascotaDeTenenciaAsync(Guid.NewGuid(), _anaId));
        }

        /// <summary>
        /// Crear la mascota nueva , la deja en el refugio para luego crear la publicaicon, verifica los datos y guarda todo sin espacios y fecha de alta (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CrearMascota_ConDatosValidos_LaGuardaEnElRefugioSinEspacios()
        {
            var request = NuevoRequest("  Luna  ");
            request.Raza = "  Mestiza ";

            using var contexto = CrearContexto();
            var servicio = new MascotaService(contexto);

            var respuesta = await servicio.CrearMascotaAsync(request, _refugioId);

            Assert.NotNull(respuesta);
            Assert.Equal("Especie de prueba", respuesta.EspecieNombre);
            var guardada = LeerMascota(respuesta.Id);
            Assert.NotNull(guardada);
            Assert.Equal("Luna", guardada.Nombre);
            Assert.Equal("Mestiza", guardada.Raza);
            Assert.Equal(_refugioId, guardada.RefugioId);
            Assert.Equal(EstadoMascota.EnRefugio, guardada.Estado);
            Assert.Equal(Sexo.Hembra, guardada.Sexo);
            Assert.True(guardada.Vacunado);
            Assert.True(guardada.FechaAlta > DateTime.UtcNow.AddMinutes(-1));
        }


        /// <summary>
        /// No se puede crear la mascota sin datos, da null (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CrearMascota_SinDatos_DaNull()
        {
            using var contexto = CrearContexto();
            var servicio = new MascotaService(contexto);

            Assert.Null(await servicio.CrearMascotaAsync(null!, _refugioId));
        }


        /// <summary>
        /// No se puede crear mascota sin el refugio, da null y no crea (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CrearMascota_SinRefugio_DaNullYNoCrea()
        {
            using var contexto = CrearContexto();
            var servicio = new MascotaService(contexto);

            Assert.Null(await servicio.CrearMascotaAsync(NuevoRequest(), Guid.Empty));
            Assert.Single(LeerMascotas());
        }


        /// <summary>
        /// Crear la mascota tiene que tener especie existente en el catalogo, si es inexistente da null y no crea la mascota (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CrearMascota_EspecieInexistente_DaNullYNoCrea()
        {
            var request = NuevoRequest();
            request.EspecieId = Guid.NewGuid();

            using var contexto = CrearContexto();
            var servicio = new MascotaService(contexto);

            Assert.Null(await servicio.CrearMascotaAsync(request, _refugioId));
            Assert.Single(LeerMascotas());
        }


        /// <summary>
        /// Crear la mascota tiene que tener condicion existente en el catalogo, si es inexistente da null y no crea la mascota (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CrearMascota_CondicionInexistente_DaNullYNoCrea()
        {
            var request = NuevoRequest();
            request.CondicionId = Guid.NewGuid();

            using var contexto = CrearContexto();
            var servicio = new MascotaService(contexto);

            Assert.Null(await servicio.CrearMascotaAsync(request, _refugioId));
            Assert.Single(LeerMascotas());
        }

        /// <summary>
        /// Actualizar la masctoa o editarla, cambia sus datos pero no el estado de la mascota, ni el refugio ni la fecha de alta (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ActualizarMascota_DelRefugio_CambiaLosDatosYNoElEstado()
        {
            var id = CargarMascota("Toto", estado: EstadoMascota.Adoptada);
            var request = NuevoRequest(" Toto Junior ");
            request.Raza = null;

            using var contexto = CrearContexto();
            var servicio = new MascotaService(contexto);

            var respuesta = await servicio.ActualizarMascotaAsync(id, request, _refugioId);

            Assert.NotNull(respuesta);
            var guardada = LeerMascota(id);
            Assert.NotNull(guardada);
            Assert.Equal("Toto Junior", guardada.Nombre);
            Assert.Null(guardada.Raza);
            Assert.Equal(Tamanio.Chico, guardada.Tamanio);
            Assert.Equal(EstadoMascota.Adoptada, guardada.Estado);
            Assert.Equal(_refugioId, guardada.RefugioId);
            Assert.Equal(_fechaAlta, guardada.FechaAlta);
        }

        /// <summary>
        /// Actualziar y editar la masctoa sin datos, no edita y da null (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ActualizarMascota_SinDatos_DaNull()
        {
            using var contexto = CrearContexto();
            var servicio = new MascotaService(contexto);

            Assert.Null(await servicio.ActualizarMascotaAsync(_mascotaId, null!, _refugioId));
        }

        /// <summary>
        /// Actualizar y editar la masctoa de otro refugio da null y no cambia los datos de esta (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ActualizarMascota_DeOtroRefugio_DaNullYNoCambia()
        {
            var otroRefugio = CargarRefugio();

            using var contexto = CrearContexto();
            var servicio = new MascotaService(contexto);

            Assert.Null(await servicio.ActualizarMascotaAsync(_mascotaId, NuevoRequest(), otroRefugio));
            Assert.Equal("Michi", LeerMascota(_mascotaId)!.Nombre);
        }


        /// <summary>
        /// Actualizar o editar una masctoa con una especie inexistente en el catalogo da null y no cambia (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ActualizarMascota_EspecieInexistente_DaNullYNoCambia()
        {
            var request = NuevoRequest();
            request.EspecieId = Guid.NewGuid();

            using var contexto = CrearContexto();
            var servicio = new MascotaService(contexto);

            Assert.Null(await servicio.ActualizarMascotaAsync(_mascotaId, request, _refugioId));
            Assert.Equal("Michi", LeerMascota(_mascotaId)!.Nombre);
        }

        /// <summary>
        /// Una mascota sin historial se peude borrar junto con sus fotos (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task EliminarMascota_SinHistoria_LaBorraConSusFotos()
        {
            CargarFotos(_mascotaId, 2);

            using var contexto = CrearContexto();
            var servicio = new MascotaService(contexto);

            Assert.Null(await servicio.EliminarMascotaAsync(_mascotaId, _refugioId));
            Assert.Null(LeerMascota(_mascotaId));
            Assert.Empty(LeerFotos(_mascotaId));
        }


        /// <summary>
        /// Eliminar una mascota de otro refugio da error y no la borra porque no pertenece a ese refugio (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task EliminarMascota_DeOtroRefugio_DaErrorYNoBorra()
        {
            var otroRefugio = CargarRefugio();

            using var contexto = CrearContexto();
            var servicio = new MascotaService(contexto);

            Assert.NotNull(await servicio.EliminarMascotaAsync(_mascotaId, otroRefugio));
            Assert.NotNull(LeerMascota(_mascotaId));
        }

        /// <summary>
        /// Eliminar una mascota que tiene una publicacion, da error y no la borra para no perder la historia (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task EliminarMascota_ConPublicacion_DaErrorYNoBorra()
        {
            CargarPublicacion(_mascotaId);

            using var contexto = CrearContexto();
            var servicio = new MascotaService(contexto);

            Assert.NotNull(await servicio.EliminarMascotaAsync(_mascotaId, _refugioId));
            Assert.NotNull(LeerMascota(_mascotaId));
        }


        /// <summary>
        /// liminar una mascota que tiene una tenencia, da error y no la borra para no perder la historia ni la tenencia(REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task EliminarMascota_ConTenencia_DaErrorYNoBorra()
        {
            var publicacionDeOtra = CargarPublicacion(CargarMascota("Toto"));
            CargarTenencia(_mascotaId, _anaId, publicacionDeOtra);

            using var contexto = CrearContexto();
            var servicio = new MascotaService(contexto);

            Assert.NotNull(await servicio.EliminarMascotaAsync(_mascotaId, _refugioId));
            Assert.NotNull(LeerMascota(_mascotaId));
        }


        /// <summary>
        /// Si se agregan fotos a la mascota del refugio se agregan al final de las anteriores para mantener el orden (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AgregarFotos_AMascotaDelRefugio_LasAgregaAlFinal()
        {
            var vieja = Assert.Single(CargarFotos(_mascotaId, 1));

            using var contexto = CrearContexto();
            var servicio = new MascotaService(contexto);

            var error = await servicio.AgregarFotosAsync(_mascotaId, _refugioId, new List<ArchivoAdjunto> { NuevaFoto(7), NuevaFoto(8) }, 6);

            Assert.Null(error);
            var fotos = LeerFotos(_mascotaId);
            Assert.Equal(new[] { 0, 1, 2 }, fotos.Select(f => f.Orden));
            Assert.Equal(vieja, fotos[0].Id);
            Assert.Equal(new byte[] { 7 }, fotos[1].Contenido);
            Assert.Equal("image/png", fotos[2].ContentType);
        }

        /// <summary>
        /// Agregar la cantidad maxima de fotos esta permitido y da ok (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AgregarFotos_JustoAlMaximo_LasAgrega()
        {
            CargarFotos(_mascotaId, 2);

            using var contexto = CrearContexto();
            var servicio = new MascotaService(contexto);

            Assert.Null(await servicio.AgregarFotosAsync(_mascotaId, _refugioId, new List<ArchivoAdjunto> { NuevaFoto() }, 3));
            Assert.Equal(3, LeerFotos(_mascotaId).Count);
        }


        /// <summary>
        /// Agregar mas de la cantidad de fotos da error y no agrega , en web avisa (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AgregarFotos_PasandoElMaximo_DaErrorYNoAgrega()
        {
            CargarFotos(_mascotaId, 2);

            using var contexto = CrearContexto();
            var servicio = new MascotaService(contexto);

            var error = await servicio.AgregarFotosAsync(_mascotaId, _refugioId, new List<ArchivoAdjunto> { NuevaFoto(), NuevaFoto() }, 3);

            Assert.NotNull(error);
            Assert.Contains("3", error);
            Assert.Equal(2, LeerFotos(_mascotaId).Count);
        }


        /// <summary>
        /// Otro refugio no puede agregar fotos a una mascota de otro refugio , da error u no agrega (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AgregarFotos_DeOtroRefugio_DaErrorYNoAgrega()
        {
            var otroRefugio = CargarRefugio();

            using var contexto = CrearContexto();
            var servicio = new MascotaService(contexto);

            Assert.NotNull(await servicio.AgregarFotosAsync(_mascotaId, otroRefugio, new List<ArchivoAdjunto> { NuevaFoto() }, 6));
            Assert.Empty(LeerFotos(_mascotaId));
        }


        /// <summary>
        /// Borrar una foto de la masctoa dl refugio, acomoda y reordena las otras (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task BorrarFoto_DelRefugio_LaBorraYReordenaLasOtras()
        {
            var fotos = CargarFotos(_mascotaId, 3);

            using var contexto = CrearContexto();
            var servicio = new MascotaService(contexto);

            Assert.True(await servicio.BorrarFotoAsync(_mascotaId, fotos[1], _refugioId));

            var quedan = LeerFotos(_mascotaId);
            Assert.Equal(new[] { fotos[0], fotos[2] }, quedan.Select(f => f.Id));
            Assert.Equal(new[] { 0, 1 }, quedan.Select(f => f.Orden));
        }


        /// <summary>
        /// Borrar una foto de una mascota de otro refugio da false y no la borra, sin acceso (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task BorrarFoto_DeOtroRefugio_DaFalseYNoBorra()
        {
            var foto = Assert.Single(CargarFotos(_mascotaId, 1));
            var otroRefugio = CargarRefugio();

            using var contexto = CrearContexto();
            var servicio = new MascotaService(contexto);

            Assert.False(await servicio.BorrarFotoAsync(_mascotaId, foto, otroRefugio));
            Assert.Single(LeerFotos(_mascotaId));
        }


        /// <summary>
        /// Borrar foto de otra mascota dentro del refugio  da false y no la borra, sin acceso (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task BorrarFoto_DeOtraMascota_DaFalseYNoBorra()
        {
            var otra = CargarMascota("Toto");
            var foto = Assert.Single(CargarFotos(otra, 1));

            using var contexto = CrearContexto();
            var servicio = new MascotaService(contexto);

            Assert.False(await servicio.BorrarFotoAsync(_mascotaId, foto, _refugioId));
            Assert.Single(LeerFotos(otra));
        }

        /// <summary>
        /// En la mascota si se hace principal a la foto, la pasa primera y corre o reordena las otras (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task HacerPrincipal_DelRefugio_LaPasaPrimeraYCorreLasOtras()
        {
            var fotos = CargarFotos(_mascotaId, 3);

            using var contexto = CrearContexto();
            var servicio = new MascotaService(contexto);

            Assert.True(await servicio.HacerPrincipalAsync(_mascotaId, fotos[2], _refugioId));

            Assert.Equal(new[] { fotos[2], fotos[0], fotos[1] }, LeerFotos(_mascotaId).Select(f => f.Id));
        }


        /// <summary>
        /// Hacer principal la foto de una masctoa de otro refugio, da false error y no cambia, sin acceso (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task HacerPrincipal_DeOtroRefugio_DaFalseYNoCambia()
        {
            var fotos = CargarFotos(_mascotaId, 2);
            var otroRefugio = CargarRefugio();

            using var contexto = CrearContexto();
            var servicio = new MascotaService(contexto);

            Assert.False(await servicio.HacerPrincipalAsync(_mascotaId, fotos[1], otroRefugio));
            Assert.Equal(fotos, LeerFotos(_mascotaId).Select(f => f.Id));
        }


        /// <summary>
        /// Obtener una foto que esta existente en base, la devuelve para ver (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ObtenerFoto_Existente_LaDevuelve()
        {
            var foto = Assert.Single(CargarFotos(_mascotaId, 1));

            using var contexto = CrearContexto();
            var servicio = new MascotaService(contexto);

            var archivo = await servicio.ObtenerFotoAsync(foto);

            Assert.NotNull(archivo);
            Assert.Equal("image/jpeg", archivo.ContentType);
            Assert.Equal(new byte[] { 0 }, archivo.Contenido);
        }

        /// <summary>
        /// Obtener una fotoid inexistente  no la devuelve y da null (REFUGIO)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ObtenerFoto_Inexistente_DaNull()
        {
            using var contexto = CrearContexto();
            var servicio = new MascotaService(contexto);

            Assert.Null(await servicio.ObtenerFotoAsync(Guid.NewGuid()));
        }
    }
}
