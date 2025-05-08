using System.Net;
using System.Security.Claims;
using System.Text.Json;
using LibraryAPI.DTOs;
using LibraryAPI.Entities;
using LibraryAPITests.Utilities;

namespace LibraryAPITests.PruebasDeIntegracion.Controllers.v1
{
    [TestClass]
    public class AuthorsControllersTests: TestsBase
    {
        private static readonly string url = "/api/v1/authors";
        private string nombreDB = Guid.NewGuid().ToString();

        [TestMethod]
        public async Task Get_Devuelve404_CuandoAuthorNoExiste()
        {
            // Preparacion

            var factory = ConstruirWebApplicationFactory(nombreDB);
            var client = factory.CreateClient();

            // Prueba
            var respuesta = await client.GetAsync($"{url}/1");

            // Verificacion
            var statusCode = respuesta.StatusCode;
            Assert.AreEqual(expected: HttpStatusCode.NotFound, actual: respuesta.StatusCode);
        }

        [TestMethod]
        public async Task Get_DevuelveAuthor_CuandoAuthorExiste()
        {
            // Preparacion
            var context = ConstruirContext(nombreDB);
            context.Authors.Add(new Author() { Names = "Luis", LastNames = "Escalante" });
            context.Authors.Add(new Author() { Names = "Teresa", LastNames = "Rodriguez" });
            await context.SaveChangesAsync();

            var factory = ConstruirWebApplicationFactory(nombreDB);
            var client = factory.CreateClient();

            // Prueba
            var respuesta = await client.GetAsync($"{url}/1");

            // Verificacion
            respuesta.EnsureSuccessStatusCode(); // Expected  a succed statusCode

            var author = JsonSerializer.Deserialize<AuthorWithBooksDTO>(
                await respuesta.Content.ReadAsStringAsync(), jsonSerializerOptions)!;

            Assert.AreEqual(expected: 1, author.Id);
        }

        [TestMethod]
        public async Task Post_Devuelve401_CuandoUsuarioNoEstaAutenticado()
        {
            // Preparacion
            var factory = ConstruirWebApplicationFactory(nombreDB, ignorarSeguridad: false);

            var cliente = factory.CreateClient();
            var authorCreacionDTO = new AuthorCreationDTO
            {
                Names = "Luis",
                LastNames = "Escalante",
                Identification = "123"
            };

            // Prueba
            var respuesta = await cliente.PostAsJsonAsync(url, authorCreacionDTO);

            // Verificacion
            Assert.AreEqual(expected: HttpStatusCode.Unauthorized, actual: respuesta.StatusCode);
        }

        [TestMethod]
        public async Task Post_Devuelve403_CuandoUsuarioNoEsAdmin()
        {
            // Preparacion
            var factory = ConstruirWebApplicationFactory(nombreDB, ignorarSeguridad: false);
            var token = await CrearUsuario(nombreDB, factory);

            var cliente = factory.CreateClient();

            cliente.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var authorCreacionDTO = new AuthorCreationDTO
            {
                Names = "Luis",
                LastNames = "Escalante",
                Identification = "123"
            };

            // Prueba
            var respuesta = await cliente.PostAsJsonAsync(url, authorCreacionDTO);

            // Verificacion
            Assert.AreEqual(expected: HttpStatusCode.Forbidden, actual: respuesta.StatusCode);
        }

        [TestMethod]
        public async Task Post_Devuelve201_CuandoUsuarioEsAdmin()
        {
            // Preparacion
            var factory = ConstruirWebApplicationFactory(nombreDB, ignorarSeguridad: false);
            
            var claims = new List<Claim> { adminClaim };

            var token = await CrearUsuario(nombreDB, factory, claims);

            var cliente = factory.CreateClient();

            cliente.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var authorCreacionDTO = new AuthorCreationDTO
            {
                Names = "Luis",
                LastNames = "Escalante",
                Identification = "123"
            };

            // Prueba
            var respuesta = await cliente.PostAsJsonAsync(url, authorCreacionDTO);

            // Verificacion
            respuesta.EnsureSuccessStatusCode();
            
            Assert.AreEqual(expected: HttpStatusCode.Created, actual: respuesta.StatusCode);
        }
    }
}