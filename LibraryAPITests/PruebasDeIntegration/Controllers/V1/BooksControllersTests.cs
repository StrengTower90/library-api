using System.Net;
using LibraryAPI.DTOs;
using LibraryAPITests.Utilities;

namespace LibraryAPITests.PruebasDeIntegracion.Controllers.v1
{
    [TestClass]
    public class BooksControllersTests: TestsBase
    {
        private readonly string url = "/api/v1/books";
        private string nombreDB = Guid.NewGuid().ToString();

        [TestMethod]
        public async Task Post_Devuelve400_CuandoAuthoresIdsEsVacio()
        {
            // Preparacion
            var factory = ConstruirWebApplicationFactory(nombreDB);
            var cliente = factory.CreateClient();
            var bookCreationDTO = new BookCreationDTO { Title = "Titulo" };

            // prueba
            var respuesta = await cliente.PostAsJsonAsync(url, bookCreationDTO);

            // Verificacion
            Assert.AreEqual(expected: HttpStatusCode.BadRequest, actual: respuesta.StatusCode);
        }
    }
}