using TransitAR.Api.Extensions;
using TransitAR.Structures;

namespace TransitAR.Api
{

    /// <summary>
    /// Datos de demo para dev, y mostrar el funcionamiento (simulando bd)
    /// 3 refugios, 5 postulantes y 12 mascotas + imagenes. 
    /// Publicaciones, postulaciones , tenencias controles y chats.
    /// Cargo todo con seeding de EF CORE , basado en la idea de datos cargados en el  init de AdoptAR, pero adaptado este entorno.
    /// </summary>
    public static class DatosDemo
    {

        //especies y condiciones que ya vienen con HasData
        private static readonly Guid Perro = Guid.Parse("11111111-1111-1111-1111-111111111101");
        private static readonly Guid Gato = Guid.Parse("11111111-1111-1111-1111-111111111102");
        private static readonly Guid Conejo = Guid.Parse("11111111-1111-1111-1111-111111111103");
        private static readonly Guid Sano = Guid.Parse("22222222-2222-2222-2222-222222222201");
        private static readonly Guid EnTratamiento = Guid.Parse("22222222-2222-2222-2222-222222222202");
        private static readonly Guid Discapacidad = Guid.Parse("22222222-2222-2222-2222-222222222203");




        /// <summary>
        /// Arma todos los datos. Devuelve usuarios, refugios con las mascotas ,fotos , publicaciones,tenencias,controles y postulaciones. Mensajes en chats tambine.
        /// </summary>
        /// <returns></returns>
        public static (List<Usuario> Usuarios, List<Refugio> Refugios) Armar()
        {
            var ahora = DateTime.UtcNow;
            var hoy = FechaExtensions.HoyEnArgentina();

            //------- REFUGIOS -------------

            var patitas = NuevoRefugio("Patitas al Rescate", "contacto@patitasalrescate.org", "Banfield", "logo-patitas.png", ahora.AddMonths(-6),
                "Rescatamos perros y gatos de la calle en Lomas de Zamora. Los castramos, vacunamos y les buscamos familia.",
                (TipoContacto.Instagram, "https://instagram.com/patitasalrescate.demo"), (TipoContacto.WhatsApp, "https://wa.me/5491100000001"));

            var sur = NuevoRefugio("Refugio del Sur", "info@refugiodelsur.org", "Adrogué", "logo-sur.png", ahora.AddMonths(-5),
                "Refugio familiar en Adrogué. Tenemos perros grandes y mayores que esperan una segunda oportunidad.",
                (TipoContacto.SitioWeb, "https://refugiodelsur.example"), (TipoContacto.Facebook, "https://facebook.com/refugiodelsur.demo"));

            var huellitas = NuevoRefugio("Huellitas Quilmes", "hola@huellitasquilmes.org", "Quilmes", "logo-huellitas.png", ahora.AddMonths(-2),
                "Red de hogares de tránsito en Quilmes. Nos especializamos en cachorros y animales con discapacidad.",
                (TipoContacto.Instagram, "https://instagram.com/huellitasquilmes.demo"), (TipoContacto.Tiktok, "https://tiktok.com/@huellitasquilmes.demo"));

            //------------- USUARIOS -------------

            var laura = NuevoUsuario("contacto@patitasalrescate.org", "patitas2026", "Laura", "Gómez", ahora.AddMonths(-6), patitas);
            var martin = NuevoUsuario("info@refugiodelsur.org", "refugiosur2026", "Martín", "Sosa", ahora.AddMonths(-5), sur);
            var valeria = NuevoUsuario("hola@huellitasquilmes.org", "huellitas2026", "Valeria", "Ruiz", ahora.AddMonths(-2), huellitas);

            var ana = NuevoUsuario("ana.perez@gmail.com", "postulante2026", "Ana", "Pérez", ahora.AddMonths(-4));
            var sofia = NuevoUsuario("sofia.ramirez@gmail.com", "sofia2026", "Sofía", "Ramírez", ahora.AddMonths(-3));
            var carlos = NuevoUsuario("carlos.diaz@gmail.com", "carlos2026", "Carlos", "Díaz", ahora.AddMonths(-4));
            var lucia = NuevoUsuario("lucia.fernandez@gmail.com", "lucia2026", "Lucía", "Fernández", ahora.AddMonths(-2));
            var diego = NuevoUsuario("diego.morales@gmail.com", "diego2026", "Diego", "Morales", ahora.AddMonths(-3));

            //------------- PERFILES -------------

            ana.Perfil = NuevoPerfil(SeleccionUsuario.Ambos, "ana.jpg", "Casa", true, true, false, null, 4, DisponibilidadHorario.Tarde,
                "A 5 cuadras", "Tuve perros toda la vida.", "Tengo patio y tiempo, quiero darle una casa a un perro que lo necesite.",
                ahora.AddMonths(-4), (TipoContacto.Instagram, "https://instagram.com/anaperez.demo"));

            sofia.Perfil = NuevoPerfil(SeleccionUsuario.Adoptante, "sofia.jpg", "Departamento", false, false, false, null, 6, DisponibilidadHorario.Noche,
                "En el mismo barrio", "Tuve un gato de chica.", "Vivo sola y busco un gato tranquilo para acompañarme.",
                ahora.AddMonths(-3));

            carlos.Perfil = NuevoPerfil(SeleccionUsuario.Transito, "carlos.jpg", "Casa", true, true, true, "Un perro adulto, macho, castrado y sociable", 2, DisponibilidadHorario.TodoElDia,
                "A 10 minutos en auto", "Hago tránsitos hace dos años.", "Trabajo desde casa y puedo cuidar animales que se están recuperando.",
                ahora.AddMonths(-4), (TipoContacto.Facebook, "https://facebook.com/carlosdiaz.demo"), (TipoContacto.WhatsApp, "https://wa.me/5491100000002"));

            lucia.Perfil = NuevoPerfil(SeleccionUsuario.Ambos, "lucia.jpg", "PH", true, false, false, null, 5, DisponibilidadHorario.Maniana,
                "A 8 cuadras", "Primera vez.", "Me gustaría ayudar, sea adoptando o como tránsito.",
                ahora.AddMonths(-2));

            diego.Perfil = NuevoPerfil(SeleccionUsuario.Transito, "diego.jpg", "Casa", true, true, true, "Una gata de 5 años", 3, DisponibilidadHorario.Tarde,
                "Enfrente de casa", "Ya transité gatos para otro refugio.", "Puedo recibir gatos o perros chicos por un tiempo.",
                ahora.AddMonths(-3), (TipoContacto.Instagram, "https://instagram.com/diegomorales.demo"));

            //------------- MASCOTAS -------------

            //Patitas al Rescate
            var luna = NuevaMascota(patitas, "Luna", Perro, Sano, "Mestiza", Sexo.Hembra, hoy.AddYears(-3), Tamanio.Mediano, true, ahora.AddMonths(-5), "luna-1.jpg", "luna-2.jpg");
            var toby = NuevaMascota(patitas, "Toby", Perro, Sano, "Mestizo", Sexo.Macho, hoy.AddDays(-45), Tamanio.Chico, false, ahora.AddDays(-20), "toby-1.jpg", "toby-2.jpg");
            var milo = NuevaMascota(patitas, "Milo", Gato, Sano, "Común europeo", Sexo.Macho, hoy.AddYears(-2), Tamanio.Chico, true, ahora.AddMonths(-4), "milo-1.jpg");
            var kira = NuevaMascota(patitas, "Kira", Perro, EnTratamiento, "Basset hound", Sexo.Hembra, hoy.AddYears(-6), Tamanio.Mediano, true, ahora.AddMonths(-2), "kira-1.jpg");
            var pelusa = NuevaMascota(patitas, "Pelusa", Conejo, Sano, null, Sexo.Hembra, hoy.AddYears(-1), Tamanio.Chico, false, ahora.AddDays(-10), "pelusa-1.jpg");

            //Refugio del Sur
            var rocco = NuevaMascota(sur, "Rocco", Perro, Sano, "Ovejero alemán", Sexo.Macho, hoy.AddYears(-2), Tamanio.Grande, true, ahora.AddMonths(-3), "rocco-1.jpg", "rocco-2.jpg");
            var nina = NuevaMascota(sur, "Nina", Gato, Sano, "Tricolor", Sexo.Hembra, hoy.AddYears(-4), Tamanio.Chico, true, ahora.AddMonths(-4), "nina-1.jpg");
            var simon = NuevaMascota(sur, "Simón", Gato, Discapacidad, null, Sexo.Macho, hoy.AddYears(-5), Tamanio.Mediano, true, ahora.AddMonths(-5), "simon-1.jpg");
            var bruno = NuevaMascota(sur, "Bruno", Perro, Sano, "Mestizo", Sexo.Macho, hoy.AddYears(-10), Tamanio.Chico, true, ahora.AddMonths(-3), "bruno-1.jpg");

            //Huellitas Quilmes
            var coco = NuevaMascota(huellitas, "Coco", Perro, Sano, "Caniche", Sexo.Macho, hoy.AddYears(-4), Tamanio.Chico, true, ahora.AddMonths(-1), "coco-1.jpg");
            var mora = NuevaMascota(huellitas, "Mora", Perro, Discapacidad, "Mestiza", Sexo.Hembra, hoy.AddYears(-3), Tamanio.Mediano, true, ahora.AddMonths(-1), "mora-1.jpg");
            var olivia = NuevaMascota(huellitas, "Olivia", Gato, Sano, null, Sexo.Hembra, hoy.AddDays(-60), Tamanio.Chico, false, ahora.AddDays(-15), "olivia-1.jpg");

            //------------- HISTORIAS -------------

            //Luna - adopción habilitada con dos postulaciones pendientes, una retirada y un mensaje sin leer para el refugio
            var pubLuna = NuevaPublicacion(luna, TipoPublicacion.Adopcion, EstadoPublicacion.Activa, ahora.AddDays(-12), "Luna busca familia",
                "Luna es una perra muy cariñosa, se lleva bien con otros perros y le encanta pasear. Está castrada y vacunada.", "Banfield");
            var anaLuna = NuevaPostulacion(pubLuna, ana, EstadoPostulacion.Pendiente, ahora.AddDays(-5));
            NuevaPostulacion(pubLuna, sofia, EstadoPostulacion.Pendiente, ahora.AddDays(-3));
            NuevaPostulacion(pubLuna, lucia, EstadoPostulacion.Retirada, ahora.AddDays(-9), ahora.AddDays(-7));
            NuevoMensaje(anaLuna, ana, "¡Hola! Vi a Luna y me encantó. ¿Se lleva bien con gatos?", ahora.AddDays(-5), leido: true);
            NuevoMensaje(anaLuna, laura, "Hola Ana, sí, convivió con gatos en el tránsito. ¿Cuándo podrías venir a conocerla?", ahora.AddDays(-4), leido: true);
            NuevoMensaje(anaLuna, ana, "¡Buenísimo! Puedo el sábado a la tarde.", ahora.AddHours(-6), leido: false);

            //Toby - tránsito activo de un cachorro, con una postulación pendiente
            var pubToby = NuevaPublicacion(toby, TipoPublicacion.Transito, EstadoPublicacion.Activa, ahora.AddDays(-6), "Tránsito para Toby, cachorro",
                "Toby tiene un mes y medio. Buscamos un tránsito por un mes hasta que tenga todas las vacunas.", "Banfield", plazo: 30);
            NuevaPostulacion(pubToby, carlos, EstadoPostulacion.Pendiente, ahora.AddDays(-2));

            //Milo - adoptado por Sofía, con un control realizado y otro para hoy
            var pubMilo = NuevaPublicacion(milo, TipoPublicacion.Adopcion, EstadoPublicacion.Cerrada, ahora.AddDays(-55), "Milo, gato naranja muy mimoso",
                "Milo es tranquilo, usa bien la piedra y se lleva bien con personas. Ideal para departamento.", "Banfield", cierre: ahora.AddDays(-40));
            var sofiaMilo = NuevaPostulacion(pubMilo, sofia, EstadoPostulacion.Aceptada, ahora.AddDays(-50), ahora.AddDays(-45));
            NuevaPostulacion(pubMilo, ana, EstadoPostulacion.Rechazada, ahora.AddDays(-48), ahora.AddDays(-40));
            var tenMilo = NuevaTenencia(sofiaMilo, TipoPublicacion.Adopcion, ahora.AddDays(-40));
            NuevoControl(tenMilo, hoy.AddDays(-33), EstadoSeguimiento.Realizado, "Se adaptó muy bien, ya duerme en la cama.", ahora.AddDays(-33));
            NuevoControl(tenMilo, hoy, EstadoSeguimiento.Pendiente);
            NuevoMensaje(sofiaMilo, laura, "Hola Sofía, ¿cómo anda Milo? Recordá que hoy toca el control.", ahora.AddDays(-1), leido: true);
            NuevoMensaje(sofiaMilo, sofia, "¡Hola! Está divino, te mando fotos a la tarde.", ahora.AddHours(-20), leido: true);

            //Kira - en tránsito con Carlos, con un control vencido
            var pubKira = NuevaPublicacion(kira, TipoPublicacion.Transito, EstadoPublicacion.Cerrada, ahora.AddDays(-28), "Tránsito para Kira mientras se recupera",
                "Kira está en tratamiento por una infección en la piel. Necesita un hogar tranquilo por un mes.", "Banfield", plazo: 30, cierre: ahora.AddDays(-20));
            var carlosKira = NuevaPostulacion(pubKira, carlos, EstadoPostulacion.Aceptada, ahora.AddDays(-26), ahora.AddDays(-22));
            NuevaPostulacion(pubKira, diego, EstadoPostulacion.Rechazada, ahora.AddDays(-25), ahora.AddDays(-20));
            var tenKira = NuevaTenencia(carlosKira, TipoPublicacion.Transito, ahora.AddDays(-20), finEstimada: hoy.AddDays(10));
            NuevoControl(tenKira, hoy.AddDays(-13), EstadoSeguimiento.Realizado, "Toma bien la medicación y está comiendo.", ahora.AddDays(-13));
            NuevoControl(tenKira, hoy.AddDays(-3), EstadoSeguimiento.Pendiente);
            NuevoMensaje(carlosKira, laura, "Carlos, ¿cómo sigue Kira con los antibióticos?", ahora.AddDays(-2), leido: true);
            NuevoMensaje(carlosKira, carlos, "Mucho mejor, ya terminó la caja. Le está creciendo el pelo.", ahora.AddHours(-10), leido: false);

            //Pelusa - recién llegada, sin publicación (asi puedo usar el boton publicar)

            //Rocco - Ana aceptada, falta confirmar la entrega; la publicación queda pausada y Lucía en espera por si se rechaza Ana
            var pubRocco = NuevaPublicacion(rocco, TipoPublicacion.Adopcion, EstadoPublicacion.Pausada, ahora.AddDays(-20), "Rocco, ovejero joven y activo",
                "Rocco es muy enérgico y necesita una casa con patio y paseos largos. Ideal para una familia activa.", "Adrogué");
            var anaRocco = NuevaPostulacion(pubRocco, ana, EstadoPostulacion.Aceptada, ahora.AddDays(-15), ahora.AddDays(-3));
            NuevaPostulacion(pubRocco, lucia, EstadoPostulacion.EnEspera, ahora.AddDays(-10));
            NuevoMensaje(anaRocco, martin, "¡Hola Ana! Te aceptamos para adoptar a Rocco. ¿Nos mandás una foto del patio?", ahora.AddDays(-3), leido: true);
            NuevoMensaje(anaRocco, ana, "¡Qué alegría! Acá va. Está todo cerrado con reja.", ahora.AddDays(-2), leido: false, "patio-ana.jpg");

            //Nina - llegó en tránsito con Diego y él decidió adoptarla (prueba de pasje de trasnito a adopción)
            var pubNina = NuevaPublicacion(nina, TipoPublicacion.Transito, EstadoPublicacion.Cerrada, ahora.AddDays(-70), "Tránsito para Nina",
                "Nina es una gata adulta muy tranquila. Buscamos tránsito por dos meses.", "Adrogué", plazo: 60, cierre: ahora.AddDays(-60));
            var diegoNina = NuevaPostulacion(pubNina, diego, EstadoPostulacion.Aceptada, ahora.AddDays(-68), ahora.AddDays(-64));
            var tenNina = NuevaTenencia(diegoNina, TipoPublicacion.Transito, ahora.AddDays(-60), finEstimada: hoy.AddDays(5));
            tenNina.Modalidad = TipoPublicacion.Adopcion;
            tenNina.FechaConversion = ahora.AddDays(-10);
            NuevoControl(tenNina, hoy.AddDays(-50), EstadoSeguimiento.Realizado, "Se escondió los primeros días, ya come tranquila.", ahora.AddDays(-50));
            NuevoControl(tenNina, hoy.AddDays(-15), EstadoSeguimiento.Realizado, "Diego nos dijo que la quiere adoptar.", ahora.AddDays(-15));
            NuevoControl(tenNina, hoy.AddDays(20), EstadoSeguimiento.Pendiente);

            //Simón - tránsito terminado (denuevo en el  refugio) y ahora en adopción
            var pubSimon1 = NuevaPublicacion(simon, TipoPublicacion.Transito, EstadoPublicacion.Cerrada, ahora.AddDays(-100), "Tránsito para Simón después de la operación",
                "Simón perdió un ojo y necesita un lugar tranquilo para recuperarse.", "Adrogué", plazo: 60, cierre: ahora.AddDays(-90));
            var carlosSimon = NuevaPostulacion(pubSimon1, carlos, EstadoPostulacion.Aceptada, ahora.AddDays(-97), ahora.AddDays(-93));
            var tenSimon = NuevaTenencia(carlosSimon, TipoPublicacion.Transito, ahora.AddDays(-90), finEstimada: hoy.AddDays(-30));
            tenSimon.FechaFinReal = ahora.AddDays(-30);
            tenSimon.FinalizoBien = true;
            tenSimon.ObservacionCierre = "Simón se recuperó muy bien y volvió al refugio listo para adopción.";
            NuevoControl(tenSimon, hoy.AddDays(-75), EstadoSeguimiento.Realizado, "La herida cicatrizó bien.", ahora.AddDays(-75));
            NuevoControl(tenSimon, hoy.AddDays(-28), EstadoSeguimiento.Cancelado);
            var pubSimon2 = NuevaPublicacion(simon, TipoPublicacion.Adopcion, EstadoPublicacion.Activa, ahora.AddDays(-25), "Simón, gato tuerto y muy compañero",
                "Simón tiene un solo ojo pero no le impide nada. Es tranquilo y le gusta estar cerca de las personas.", "Adrogué");
            NuevaPostulacion(pubSimon2, sofia, EstadoPostulacion.Pendiente, ahora.AddDays(-1));

            //Bruno - tránsito con Lucía que ya pasó la fecha de devolución (vencida), con un control reprogramado
            var pubBruno = NuevaPublicacion(bruno, TipoPublicacion.Transito, EstadoPublicacion.Cerrada, ahora.AddDays(-40), "Tránsito para Bruno, perro mayor",
                "Bruno tiene 10 años y le cuesta el frío del refugio. Buscamos un tránsito por un mes.", "Adrogué", plazo: 30, cierre: ahora.AddDays(-35));
            var luciaBruno = NuevaPostulacion(pubBruno, lucia, EstadoPostulacion.Aceptada, ahora.AddDays(-39), ahora.AddDays(-37));
            var tenBruno = NuevaTenencia(luciaBruno, TipoPublicacion.Transito, ahora.AddDays(-35), finEstimada: hoy.AddDays(-5));
            NuevoControl(tenBruno, hoy.AddDays(-8), EstadoSeguimiento.Reprogramado, "Lucía estaba de viaje, lo pasamos para la semana que viene.");
            NuevoControl(tenBruno, hoy.AddDays(2), EstadoSeguimiento.Pendiente);
            NuevoMensaje(luciaBruno, martin, "Hola Lucía, ya pasó la fecha de devolución de Bruno. ¿Lo traés o lo querés seguir cuidando?", ahora.AddDays(-1), leido: false);

            //Coco - adopción activa, una rechazada con motivo y una pendiente
            var pubCoco = NuevaPublicacion(coco, TipoPublicacion.Adopcion, EstadoPublicacion.Activa, ahora.AddDays(-18), "Coco, caniche adulto",
                "Coco es muy sociable y está acostumbrado a vivir adentro. Necesita que lo bañen seguido.", "Quilmes");
            var sofiaCoco = NuevaPostulacion(pubCoco, sofia, EstadoPostulacion.Rechazada, ahora.AddDays(-16), ahora.AddDays(-14));
            sofiaCoco.ObservacionRechazo = "Coco ladra mucho cuando se queda solo y no es ideal para un departamento.";
            NuevaPostulacion(pubCoco, lucia, EstadoPostulacion.Pendiente, ahora.AddDays(-4));

            //Mora - adopción activa sin postulaciones
            NuevaPublicacion(mora, TipoPublicacion.Adopcion, EstadoPublicacion.Activa, ahora.AddDays(-8), "Mora, perra de tres patas",
                "Mora perdió una pata en un accidente y se mueve perfectamente. Es juguetona y muy dulce.", "Quilmes");

            //Olivia - tránsito activo de una gata con dos postulaciones
            var pubOlivia = NuevaPublicacion(olivia, TipoPublicacion.Transito, EstadoPublicacion.Activa, ahora.AddDays(-7), "Tránsito para Olivia, gatita",
                "Olivia tiene dos meses y necesita un hogar hasta que la castremos.", "Quilmes", plazo: 45);
            NuevaPostulacion(pubOlivia, diego, EstadoPostulacion.Pendiente, ahora.AddDays(-5));
            NuevaPostulacion(pubOlivia, carlos, EstadoPostulacion.Pendiente, ahora.AddDays(-1));

            //los estados de las mascotas quedan como los dejarían los services segun lo puesto
            milo.Estado = EstadoMascota.Adoptada;
            kira.Estado = EstadoMascota.EnTransito;
            nina.Estado = EstadoMascota.Adoptada;
            bruno.Estado = EstadoMascota.EnTransito;

            var usuarios = new List<Usuario> { laura, martin, valeria, ana, sofia, carlos, lucia, diego };
            var refugios = new List<Refugio> { patitas, sur, huellitas };
            return (usuarios, refugios);
        }



        // --------------- AUXILIARES -----------------------

        /// <summary>
        /// lee una foto de Resources , si no esta sigue sin la foto
        /// </summary>
        /// <param name="nombre"></param>
        /// <returns></returns>
        private static byte[]? LeerFoto(string nombre)
        {
            var carpeta = Path.GetDirectoryName(typeof(DatosDemo).Assembly.Location)!;
            var ruta = Path.Combine(carpeta, "Resources", "DatosDemo", nombre);

            return File.Exists(ruta) ? File.ReadAllBytes(ruta) : null;
        }


        private static string TipoDeFoto(string nombre)
            => nombre.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ? "image/png" : "image/jpeg";


        /// <summary>
        /// Para crear el nuevo refugio
        /// </summary>
        /// <param name="nombre"></param>
        /// <param name="email"></param>
        /// <param name="localidad"></param>
        /// <param name="logo"></param>
        /// <param name="alta"></param>
        /// <param name="descripcion"></param>
        /// <param name="contactos"></param>
        /// <returns></returns>
        private static Refugio NuevoRefugio(string nombre, string email, string localidad, string logo, DateTime alta, string descripcion,
           params (TipoContacto Tipo, string Url)[] contactos)
        {
            var refugio = new Refugio
            {
                Id = Guid.NewGuid(),
                Nombre = nombre,
                Descripcion = descripcion,
                Email = email,
                Localidad = localidad,
                Activo = true,
                FechaAlta = alta,
                FotoPerfil = LeerFoto(logo)
            };
            refugio.FotoContentType = refugio.FotoPerfil != null ? TipoDeFoto(logo) : null;

            foreach (var c in contactos)
                refugio.Contactos.Add(new ContactoRefugio { Id = Guid.NewGuid(), RefugioId = refugio.Id, Tipo = c.Tipo, Url = c.Url });

            return refugio;
        }


        /// <summary>
        /// Si viene con refugio es una cuenta que va a ir para refugio hecho por la cuenta fundadora, sino es un postulante
        /// </summary>
        /// <param name="email"></param>
        /// <param name="clave"></param>
        /// <param name="nombre"></param>
        /// <param name="apellido"></param>
        /// <param name="alta"></param>
        /// <param name="refugio"></param>
        /// <returns></returns>
        private static Usuario NuevoUsuario(string email, string clave, string nombre, string apellido, DateTime alta, Refugio? refugio = null) => new()
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(clave),
            Nombre = nombre,
            Apellido = apellido,
            Rol = refugio != null ? RolUsuario.Refugio : RolUsuario.Usuario,
            RolRefugio = refugio != null ? RolRefugio.Fundador : null,
            RefugioId = refugio?.Id,
            Activo = true,
            FechaAlta = alta
        };


        /// <summary>
        /// Para completar el perfil del postulante y poder postularse
        /// </summary>
        /// <param name="seleccion"></param>
        /// <param name="foto"></param>
        /// <param name="vivienda"></param>
        /// <param name="patio"></param>
        /// <param name="patioCerrado"></param>
        /// <param name="otrasMascotas"></param>
        /// <param name="detalleOtras"></param>
        /// <param name="horasSolo"></param>
        /// <param name="disponibilidad"></param>
        /// <param name="veterinaria"></param>
        /// <param name="experiencia"></param>
        /// <param name="motivo"></param>
        /// <param name="completado"></param>
        /// <param name="contactos"></param>
        /// <returns></returns>
        private static PerfilPostulante NuevoPerfil(SeleccionUsuario seleccion, string foto, string vivienda, bool patio, bool patioCerrado,
           bool otrasMascotas, string? detalleOtras, int horasSolo, DisponibilidadHorario disponibilidad, string veterinaria,
           string experiencia, string motivo, DateTime completado, params (TipoContacto Tipo, string Url)[] contactos)
        {
            var perfil = new PerfilPostulante
            {
                Id = Guid.NewGuid(),
                Seleccion = seleccion,
                FotoPerfil = LeerFoto(foto),
                TipoVivienda = vivienda,
                TienePatio = patio,
                PatioCerrado = patio && patioCerrado,
                TieneOtrasMascotas = otrasMascotas,
                DetalleOtrasMascotas = otrasMascotas ? detalleOtras : null,
                HorasSoloPorDia = horasSolo,
                DisponibilidadHorario = disponibilidad,
                CercaniaVeterinaria = veterinaria,
                ExperienciaPrevia = experiencia,
                MotivoPostulacion = motivo,
                FechaCompletado = completado
            };
            perfil.FotoContentType = perfil.FotoPerfil != null ? TipoDeFoto(foto) : null;

            foreach (var c in contactos)
                perfil.Contactos.Add(new ContactoPostulante { Id = Guid.NewGuid(), PerfilPostulanteId = perfil.Id, Tipo = c.Tipo, Url = c.Url });

            return perfil;
        }

        /// <summary>
        /// Para cargar la nueva mascota
        /// </summary>
        /// <param name="refugio"></param>
        /// <param name="nombre"></param>
        /// <param name="especie"></param>
        /// <param name="condicion"></param>
        /// <param name="raza"></param>
        /// <param name="sexo"></param>
        /// <param name="nacimiento"></param>
        /// <param name="tamanio"></param>
        /// <param name="vacunado"></param>
        /// <param name="alta"></param>
        /// <param name="fotos"></param>
        /// <returns></returns>
        private static Mascota NuevaMascota(Refugio refugio, string nombre, Guid especie, Guid condicion, string? raza, Sexo sexo,
            DateTime nacimiento, Tamanio tamanio, bool vacunado, DateTime alta, params string[] fotos)
        {
            var mascota = new Mascota
            {
                Id = Guid.NewGuid(),
                Nombre = nombre,
                RefugioId = refugio.Id,
                EspecieId = especie,
                CondicionId = condicion,
                Raza = raza,
                Sexo = sexo,
                FechaNacimientoAproximada = nacimiento,
                Tamanio = tamanio,
                Vacunado = vacunado,
                Estado = EstadoMascota.EnRefugio,
                FechaAlta = alta
            };

            //va desde el 0 como cuando se suben desde la Web
            foreach (var foto in fotos)
            {
                var contenido = LeerFoto(foto);
                if (contenido != null)
                    mascota.Fotos.Add(new FotoMascota { Id = Guid.NewGuid(), MascotaId = mascota.Id, Orden = mascota.Fotos.Count, ContentType = TipoDeFoto(foto), Contenido = contenido });
            }

            refugio.Mascotas.Add(mascota);
            return mascota;
        }


        /// <summary>
        /// Para cargar la nueva publicación
        /// </summary>
        /// <param name="mascota"></param>
        /// <param name="tipo"></param>
        /// <param name="estado"></param>
        /// <param name="fecha"></param>
        /// <param name="titulo"></param>
        /// <param name="descripcion"></param>
        /// <param name="ubicacion"></param>
        /// <param name="plazo"></param>
        /// <param name="cierre"></param>
        /// <returns></returns>
        private static Publicacion NuevaPublicacion(Mascota mascota, TipoPublicacion tipo, EstadoPublicacion estado, DateTime fecha,
            string titulo, string descripcion, string ubicacion, int? plazo = null, DateTime? cierre = null)
        {
            var publicacion = new Publicacion
            {
                Id = Guid.NewGuid(),
                MascotaId = mascota.Id,
                Titulo = titulo,
                Descripcion = descripcion,
                Ubicacion = ubicacion,
                Tipo = tipo,
                Estado = estado,
                PlazoEstimado = tipo == TipoPublicacion.Transito ? plazo : null,
                FechaPublicacion = fecha,
                FechaCierre = cierre
            };

            mascota.Publicaciones.Add(publicacion);
            return publicacion;
        }



        /// <summary>
        /// Para cargar la nueva postulación
        /// </summary>
        /// <param name="publicacion"></param>
        /// <param name="usuario"></param>
        /// <param name="estado"></param>
        /// <param name="fecha"></param>
        /// <param name="resolucion"></param>
        /// <returns></returns>
        private static Postulacion NuevaPostulacion(Publicacion publicacion, Usuario usuario, EstadoPostulacion estado, DateTime fecha, DateTime? resolucion = null)
        {
            var postulacion = new Postulacion
            {
                Id = Guid.NewGuid(),
                PublicacionId = publicacion.Id,
                Publicacion = publicacion,
                UsuarioId = usuario.Id,
                Estado = estado,
                FechaPostulacion = fecha,
                FechaResolucion = resolucion
            };

            publicacion.Postulaciones.Add(postulacion);
            return postulacion;
        }



        /// <summary>
        /// Para cargar las nuevas tenencias
        /// </summary>
        /// <param name="postulacion"></param>
        /// <param name="modalidad"></param>
        /// <param name="inicio"></param>
        /// <param name="finEstimada"></param>
        /// <returns></returns>
        private static Tenencia NuevaTenencia(Postulacion postulacion, TipoPublicacion modalidad, DateTime inicio, DateTime? finEstimada = null)
        {
            var tenencia = new Tenencia
            {
                Id = Guid.NewGuid(),
                PostulacionId = postulacion.Id,
                MascotaId = postulacion.Publicacion?.MascotaId ?? Guid.Empty,
                Modalidad = modalidad,
                FechaInicio = inicio,
                FechaFinEstimada = modalidad == TipoPublicacion.Transito ? finEstimada : null
            };

            postulacion.Tenencia = tenencia;
            return tenencia;
        }


        /// <summary>
        /// Cargo el nuevo control o seguimiento
        /// </summary>
        /// <param name="tenencia"></param>
        /// <param name="programada"></param>
        /// <param name="estado"></param>
        /// <param name="observacion"></param>
        /// <param name="realizada"></param>
        private static void NuevoControl(Tenencia tenencia, DateTime programada, EstadoSeguimiento estado, string? observacion = null, DateTime? realizada = null)
            => tenencia.Seguimientos.Add(new Seguimiento
            {
                Id = Guid.NewGuid(),
                TenenciaId = tenencia.Id,
                FechaProgramada = programada,
                Estado = estado,
                Observacion = observacion,
                FechaRealizada = estado == EstadoSeguimiento.Realizado ? realizada : null
            });


        /// <summary>
        /// Nuevo mensjae para replicar en el seguimiento o tenencia
        /// </summary>
        /// <param name="postulacion"></param>
        /// <param name="emisor"></param>
        /// <param name="texto"></param>
        /// <param name="fecha"></param>
        /// <param name="leido"></param>
        /// <param name="adjunto"></param>
        private static void NuevoMensaje(Postulacion postulacion, Usuario emisor, string texto, DateTime fecha, bool leido, string? adjunto = null)
        {
            var mensaje = new Mensaje
            {
                Id = Guid.NewGuid(),
                PostulacionId = postulacion.Id,
                EmisorId = emisor.Id,
                Texto = texto,
                FechaEnvio = fecha,
                FechaLectura = leido ? fecha.AddHours(1) : null
            };

            var contenido = adjunto != null ? LeerFoto(adjunto) : null;
            if (contenido != null)
                mensaje.Adjuntos.Add(new AdjuntoMensaje { Id = Guid.NewGuid(), MensajeId = mensaje.Id, NombreArchivo = adjunto!, ContentType = TipoDeFoto(adjunto!), TamanioBytes = contenido.Length, Contenido = contenido });

            postulacion.Mensajes.Add(mensaje);
        }

    }
}
