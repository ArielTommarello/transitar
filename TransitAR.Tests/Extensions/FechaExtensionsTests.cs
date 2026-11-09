using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransitAR.Api.Extensions;



namespace TransitAR.Tests.Extensions
{
    public class FechaExtensionsTests 
    {
        /// <summary>
        /// Si paso las 22hs en argentina, en UTC es un dia despues, tiene que seguir siendo el mismo dia
        /// </summary>
        [Fact]
        public void DiaEnArgentina_ALasDiezDeLaNoche_EsElMismoDia()
        {
            var utc = new DateTime(2026, 3, 10, 1, 0, 0, DateTimeKind.Utc);

            Assert.Equal(new DateTime(2026, 3, 9), FechaExtensions.DiaEnArgentina(utc));
        }



        /// <summary>
        /// Durante el dia es el mismo dia en UTC y en Argentina
        /// </summary>
        [Fact]
        public void DiaEnArgentina_DuranteElDia_EsElMismoDia()
        {
            var utc = new DateTime(2026, 3, 9, 18, 0, 0, DateTimeKind.Utc);

            Assert.Equal(new DateTime(2026, 3, 9), FechaExtensions.DiaEnArgentina(utc));
        }



        /// <summary>
        /// A la medianoche en Argenitna ya es dia siguiente.
        /// </summary>
        [Fact]
        public void DiaEnArgentina_ALaMedianoche_EsElDiaSiguiente()
        {
            var utc = new DateTime(2026, 3, 10, 3, 0, 0, DateTimeKind.Utc);

            Assert.Equal(new DateTime(2026, 3, 10), FechaExtensions.DiaEnArgentina(utc));
        }
    }
}
