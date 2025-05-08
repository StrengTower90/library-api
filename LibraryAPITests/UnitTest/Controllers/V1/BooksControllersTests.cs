using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryAPI.Controllers.v1;
using LibraryAPI.DTOs;
using LibraryAPITests.Utilities;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OutputCaching;
using NSubstitute;

namespace LibraryAPITests.UnitTest.Controllers.V1
{
    [TestClass]
    public class BooksControllersTests: TestsBase
    {
        [TestMethod]
        public async Task Get_RetornaCeroLibros_CuandoNoHayLibros()
        {
            // Preparacion
            var nombreDB = Guid.NewGuid().ToString();
            var context = ConstruirContext(nombreDB);
            var mapper = ConfigurarAutoMapper();
            IDataProtectionProvider dataProtectionProvider = Substitute.For<IDataProtectionProvider>();
            IOutputCacheStore outputCacheStore = null!;

            var controller = new BooksControllers(context, mapper, dataProtectionProvider,
             outputCacheStore);

             controller.ControllerContext.HttpContext = new DefaultHttpContext();

             var paginationDTO = new PaginationDTO(1, 1);

            // Prueba
            var respuesta = await controller.Get(paginationDTO);

            // Verificacion
            Assert.AreEqual(expected: 0, actual: respuesta.Count());
        }
    }
}