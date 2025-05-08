using System.Net;
using System.Net.Http.Headers;
using LibraryAPI.Entities;
using LibraryAPITests.Utilities;
using Microsoft.EntityFrameworkCore;
using NSubstitute.Core;

namespace LibraryAPITests.PruebasDeIntegracion.Controllers.v1
{
    [TestClass]
    public class CommentsControllersTests: TestsBase
    {
        private readonly string url = "/api/v1/books/1/comments";
        private string nombreDB = Guid.NewGuid().ToString();

        private async Task CrearDataDePrueba()
        {
            var context = ConstruirContext(nombreDB);
            var author = new Author { Names = "Luis", LastNames = "Escalante" };
            context.Add(author);
            await context.SaveChangesAsync();

            var book = new Book { Title = "titulo" };
            book.Authors.Add(new AuthorBook { Author = author });
            context.Add(book);
            await context.SaveChangesAsync();
        }

        [TestMethod]
        public async Task Delete_Devuelve204_CuandoUsuarioBorraSuPropioComentario()
        {
            // Preparacion
            await CrearDataDePrueba();

            var factory = ConstruirWebApplicationFactory(nombreDB, ignorarSeguridad: false);

            var token = await CrearUsuario(nombreDB, factory);

            var context = ConstruirContext(nombreDB);
            var usuario = await context.Users.FirstAsync();

            var comentario = new Comment
            {
                Body = "contenido",
                UserId = usuario!.Id,
                BookId = 1
            };

            context.Add(comentario);
            await context.SaveChangesAsync();

            var cliente = factory.CreateClient();
            cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Prueba
            var respuesta = await cliente.DeleteAsync($"{url}/{comentario.Id}");

            // Verificacion
            Assert.AreEqual(expected: HttpStatusCode.NoContent, actual: respuesta.StatusCode);
        }

        [TestMethod]
        public async Task Delete_Devuelve403_CuandoUsuarioIntentaBorrarElComentarioDeOtro()
        {
            // Preparacion
            await CrearDataDePrueba();

            var factory = ConstruirWebApplicationFactory(nombreDB, ignorarSeguridad: false);

            var emailCreadorComentario = "creador-comentario@hotmail.com";
            await CrearUsuario(nombreDB, factory, [], emailCreadorComentario);

            var context = ConstruirContext(nombreDB);
            var usuarioCreadorComentario = await context.Users.FirstAsync();

            var comentario = new Comment
            {
                Body = "contenido",
                UserId = usuarioCreadorComentario!.Id,
                BookId = 1
            };

            context.Add(comentario);
            await context.SaveChangesAsync();

            var tokenUsuarioDistinto = await CrearUsuario(nombreDB, factory,
                [], "usuario-distinto@hotmail.com");

            var cliente = factory.CreateClient();
            cliente.DefaultRequestHeaders.Authorization = 
                    new AuthenticationHeaderValue("Bearer", tokenUsuarioDistinto);

            // Prueba
            var respuesta = await cliente.DeleteAsync($"{url}/{comentario.Id}");

            // Verificacion
            Assert.AreEqual(expected: HttpStatusCode.Forbidden, actual: respuesta.StatusCode);
        }
    }
}