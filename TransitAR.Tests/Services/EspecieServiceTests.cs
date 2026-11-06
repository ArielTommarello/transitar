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
    public class EspecieServiceTests : BaseEnMemoria
    {
        private readonly Especie _conejo = new() { Id = Guid.NewGuid(), Nombre = "Conejo" };

        public EspecieServiceTests()
        {
            using var contexto = CrearContexto();

            contexto.Especies.Add(_conejo);
            contexto.SaveChanges();
        }

        //Car la mascota con datos para la pueba en el contexto
        private void CargarMascota()
        {
            using var contexto = CrearContexto();

            var refugio = new Refugio { Id = Guid.NewGuid(), Nombre = "Patitas", Email = "patitas@test.org", Localidad = "Lomas" };
            var condicion = new Condicion { Id = Guid.NewGuid(), Nombre = "Condicion de prueba" };
            contexto.AddRange(refugio, condicion);
            contexto.Mascotas.Add(new Mascota { Id = Guid.NewGuid(), Nombre = "Michi", RefugioId = refugio.Id, EspecieId = _conejo.Id, CondicionId = condicion.Id });
            contexto.SaveChanges();
        }

        //********* PRUBEAS *************

        /// <summary>
        /// crear especie con espacios  (ADMIN)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CrearEspecie_ConNombreNuevo_LaGuardaSinEspacios()
        {
            using var contexto = CrearContexto();
            var servicio = new EspecieService(contexto);

            var resultado = await servicio.CrearEspecieAsync(new EspecieRequest { Nombre = "  Hamster  " });

            Assert.Null(resultado.Error);
            Assert.True(await contexto.Especies.AnyAsync(e => e.Nombre == "Hamster"));
        }


        /// <summary>
        /// Crea especie con un nombre que ya existe tiene que dar error y no guardar  (ADMIN)
        /// </summary>
        /// <returns></returns>

        [Fact]
        public async Task CrearEspecie_ConNombreRepetido_DevuelveErrorYNoGuarda()
        {
            using var contexto = CrearContexto();
            var servicio = new EspecieService(contexto);
            var cantidad = await contexto.Especies.CountAsync();

            var resultado = await servicio.CrearEspecieAsync(new EspecieRequest { Nombre = "Conejo" });

            Assert.Equal("Ya existe una especie llamada Conejo.", resultado.Error);
            Assert.Equal(cantidad, await contexto.Especies.CountAsync());
        }

        /// <summary>
        /// Elimina la especie que esta en uso (regla que  no puede porque la estan usando (ADMIN)
        /// </summary>
        /// <returns></returns>

        [Fact]
        public async Task EliminarEspecie_EnUso_DevuelveErrorYNoBorra()
        {
            CargarMascota();

            using var contexto = CrearContexto();
            var servicio = new EspecieService(contexto);

            var error = await servicio.EliminarEspecieAsync(_conejo.Id);

            Assert.Equal("No se puede borrar: hay mascotas cargadas con esta especie.", error);
            Assert.True(await contexto.Especies.AnyAsync(e => e.Id == _conejo.Id));
        }


        /// <summary>
        /// Eliminar especie que nadie usa exito (ADMIN)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task EliminarEspecie_SinUso_LaBorra()
        {
            using var contexto = CrearContexto();
            var servicio = new EspecieService(contexto);

            var error = await servicio.EliminarEspecieAsync(_conejo.Id);

            Assert.Null(error);
            Assert.False(await contexto.Especies.AnyAsync(e => e.Id == _conejo.Id));
        }
    }
}
