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
    public class CondicionServiceTests : BaseEnMemoria
    {

        /// <summary>
        /// Creo el contexto y dos condiciones
        /// </summary>
        private readonly Condicion _sana = new() { Id = Guid.NewGuid(), Nombre = "Sana" };
        private readonly Condicion _enTratamiento = new() { Id = Guid.NewGuid(), Nombre = "En tratamiento" };

        public CondicionServiceTests()
        {
            using var contexto = CrearContexto();

            contexto.Condiciones.AddRange(_sana, _enTratamiento);
            contexto.SaveChanges();
        }

        //Car la mascota con datos para la pueba en el contexto
        private void CargarMascota()
        {
            using var contexto = CrearContexto();

            var refugio = new Refugio { Id = Guid.NewGuid(), Nombre = "Patitas", Email = "patitas@test.org", Localidad = "Lomas" };
            var especie = new Especie { Id = Guid.NewGuid(), Nombre = "Especie de prueba" };
            contexto.AddRange(refugio, especie);
            contexto.Mascotas.Add(new Mascota { Id = Guid.NewGuid(), Nombre = "Michi", RefugioId = refugio.Id, EspecieId = especie.Id, CondicionId = _sana.Id });
            contexto.SaveChanges();
        }


        //********* PRUBEAS *************

        /// <summary>
        /// crear condicion con espacios  (ADMIN)
        /// </summary>
        /// <returns></returns>

        [Fact]
        public async Task CrearCondicion_ConNombreNuevo_LaGuardaSinEspacios()
        {
            using var contexto = CrearContexto();
            var servicio = new CondicionService(contexto);

            var resultado = await servicio.CrearCondicionAsync(new CondicionRequest { Nombre = "  Ciega  " });

            Assert.Null(resultado.Error);
            Assert.True(await contexto.Condiciones.AnyAsync(c => c.Nombre == "Ciega"));
        }

        /// <summary>
        /// Crea condicion con un nombre que ya existe tiene que dar error y no guardar  (ADMIN)
        /// </summary>
        /// <returns></returns>

        [Fact]
        public async Task CrearCondicion_ConNombreRepetido_DevuelveErrorYNoGuarda()
        {
            using var contexto = CrearContexto();
            var servicio = new CondicionService(contexto);
            var cantidad = await contexto.Condiciones.CountAsync();

            var resultado = await servicio.CrearCondicionAsync(new CondicionRequest { Nombre = "Sana" });

            Assert.Equal("Ya existe una condición llamada Sana.", resultado.Error);
            Assert.Equal(cantidad, await contexto.Condiciones.CountAsync());
        }


        /// <summary>
        /// Actualizo condicion con nombre que ya existe y tiene que dar error (ADMIN)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ActualizarCondicion_ConNombreDeOtra_DevuelveError()
        {
            using var contexto = CrearContexto();
            var servicio = new CondicionService(contexto);

            var resultado = await servicio.ActualizarCondicionAsync(_sana.Id, new CondicionRequest { Nombre = "En tratamiento" });

            Assert.Equal("Ya existe otra condición llamada En tratamiento.", resultado.Error);
        }

        /// <summary>
        /// Actualizar la condicion con el mismo nombre es ok (ADMIN)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ActualizarCondicion_ConSuMismoNombre_Guarda()
        {
            using var contexto = CrearContexto();
            var servicio = new CondicionService(contexto);

            var resultado = await servicio.ActualizarCondicionAsync(_sana.Id, new CondicionRequest { Nombre = "Sana", Descripcion = "Sin problemas" });

            Assert.Null(resultado.Error);
            Assert.Equal("Sin problemas", resultado.Condicion!.Descripcion);
        }

        /// <summary>
        /// Actualizar algo que no existe , no se encuentra (ADMIN)
        /// </summary>
        /// <returns></returns>

        [Fact]
        public async Task ActualizarCondicion_Inexistente_DevuelveError()
        {
            using var contexto = CrearContexto();
            var servicio = new CondicionService(contexto);

            var resultado = await servicio.ActualizarCondicionAsync(Guid.NewGuid(), new CondicionRequest { Nombre = "Otra" });

            Assert.Equal("No encontramos esa condición.", resultado.Error);
        }

        /// <summary>
        /// Eliminar una condicion en uso da error y no la borra (ADMIN)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task EliminarCondicion_EnUso_DevuelveErrorYNoBorra()
        {
            CargarMascota();

            using var contexto = CrearContexto();
            var servicio = new CondicionService(contexto);

            var error = await servicio.EliminarCondicionAsync(_sana.Id);

            Assert.Equal("No se puede borrar: hay mascotas cargadas con esta condición.", error);
            Assert.True(await contexto.Condiciones.AnyAsync(c => c.Id == _sana.Id));
        }

        /// <summary>
        /// Eliminar condicion sin uso si la borra ok (ADMIN)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task EliminarCondicion_SinUso_LaBorra()
        {
            using var contexto = CrearContexto();
            var servicio = new CondicionService(contexto);

            var error = await servicio.EliminarCondicionAsync(_sana.Id);

            Assert.Null(error);
            Assert.False(await contexto.Condiciones.AnyAsync(c => c.Id == _sana.Id));
        }

    }
}
