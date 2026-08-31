using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransitAR.Structures
{
    /// <summary>
    /// Contexto de TransitAR 
    /// </summary>
    public class TransitARContext: DbContext
    {

        /// <summary>
        /// Inicializa el contexto
        /// </summary>
        /// <param name="options"></param>
        public TransitARContext(DbContextOptions<TransitARContext> options) : base(options) { 
        
        }
        
        //Cuentas registradas (postulante, refugio, admin)
        public DbSet<Usuario> Usuarios { get; set; }

        //Requerimientos y cuenta de postulantes
        public DbSet<PerfilPostulante> PerfilPostulantes { get; set; }

        //Redes Sociles de los postualntes
        public DbSet<ContactoPostulante> ContactoPostulantes { get; set; }


        //Refugios registrados en la plataforma
        public DbSet<Refugio> Refugios { get; set; }

        //Redes Sociles de los refugios
        public DbSet<ContactoRefugio> ContactosRefugio { get; set; }

        //Especies, administradas por el admin
        public DbSet<Especie> Especies { get; set; }

        //Condiciones sanitarias, administradas por el admin
        public DbSet<Condicion> Condiciones { get; set; }

        //Lista de mascotas cargadas por el refugio
        public DbSet<Mascota> Mascotas { get; set; }

        //Publicaciones hecas por el refugio
        public DbSet<Publicacion> Publicaciones { get; set; }

        //Postulaciones de los usuarios sobre cada publicacion
        public DbSet<Postulacion> Postulaciones { get; set; }


        //Tenencia de cada animal (donde esta, estuvo o estara, hasta cuando)
        public DbSet<Tenencia> Tenencias { get; set; }


        //Controles y visitas de cada tenencia del animal
        public DbSet<Seguimiento> Seguimientos { get; set; }


        //PChat interno entre refugio y postulante
        public DbSet<Mensaje> Mensajes { get; set; }

        //Configuracion para los modos de delete, 2 tipos de relacion refugio Tenencia . Si borro un refugio, se borrarian las mascotas y demas por defecto
        //Relacion tenencia - mascota 2 caminos error FK
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            foreach (var fk in modelBuilder.Model.GetEntityTypes().SelectMany(t => t.GetForeignKeys()))
                fk.DeleteBehavior = DeleteBehavior.Restrict;
        }

    }
}
