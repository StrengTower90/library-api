using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryAPI.Controllers.v1;
using LibraryAPI.DTOs;
using LibraryAPI.Entities;
using LibraryAPI.Services;
using LibraryAPI.Services.v1;
using LibraryAPITests.Utilities;
using LibraryAPITests.Utilities.Dobles;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using NSubstitute;

namespace LibraryAPITests.UnitTest.Controllers.V1
{

    [TestClass]
    public class AuthorsControllersTests: TestsBase
    {
        IFileStorage almacenadorArchivos = null!;
        ILogger<AuthorsController> logger = null!;
        IOutputCacheStore outputCacheStore = null!;
        IServiceAuthors serviceAuthors = null!;
        private string nombreDB = Guid.NewGuid().ToString();
        private AuthorsController controller = null!;
        private const string container = "authors";
        private const string cache = "authors-obtener";

        // This Method with the Test class initialization will execute before the rest TestMethod
        [TestInitialize]
        public void Setup()
        {
           var context = ConstruirContext(nombreDB);
           var mapper = ConfigurarAutoMapper();
           almacenadorArchivos = Substitute.For<IFileStorage>();
           logger = Substitute.For<ILogger<AuthorsController>>();
           outputCacheStore = Substitute.For<IOutputCacheStore>();
           serviceAuthors = Substitute.For<IServiceAuthors>();

           controller = new AuthorsController(context, mapper, almacenadorArchivos,
            logger, outputCacheStore, serviceAuthors);
        }

        [TestMethod]
        public async Task Get_Retorna404_CuandoAutorConIdExiste()
        {
            // Prueba
            var respuesta = await controller.Get(1);

            // Verificacion
            var resultado = respuesta.Result as StatusCodeResult;
            Assert.AreEqual(expected: 404, actual: resultado!.StatusCode);
        }

        [TestMethod]
        public async Task Get_DebeLlamarGetDelServicionAuthors()
        {
            // Preparacion 
            var paginacionDTO = new PaginationDTO(2, 3);

            // Prueba 
            await controller.Get(paginacionDTO);

            // Verificacion
            await serviceAuthors.Received(1).Get(paginacionDTO);
        }

        [TestMethod]
        public async Task Get_RetornarAuthor_CuandoAutorConIdExiste()
        {
            // Preparacion
           var context = ConstruirContext(nombreDB);

            context.Authors.Add(new Author { Names = "Luis", LastNames = "Escalante" });
            context.Authors.Add(new Author { Names = "claudia", LastNames = "Rodriguez" });

            await context.SaveChangesAsync();

            // Prueba
            var respuesta = await controller.Get(1);

            // Verificacion
            var resultado = respuesta.Value;
            Assert.AreEqual(expected: 1, actual: resultado!.Id);
        }

        [TestMethod]
        public async Task Get_DebeRetornar_UnAuthorConSusLibros_CuandoTieneLibros()
        {
            // Preparacion
            var libro1 = new Book { Title = "Libro 1"};
            var libro2 = new Book { Title = "Libro 2"};

            var author = new Author()
            {
                Names = "Luis",
                LastNames = "Escalante",
                Books = new List<AuthorBook>
                {
                    new AuthorBook{Book = libro1},
                    new AuthorBook{Book = libro2}  
                }
            };

            var context = ConstruirContext(nombreDB);

            context.Add(author);
            
            await context.SaveChangesAsync();

            // Prueba
            var respuesta = await controller.Get(1);

            // Verificacion
            var resultado = respuesta.Value;
            Assert.AreEqual(expected: 1, actual: resultado!.Id);
            Assert.AreEqual(expected: 2, actual: resultado.Books.Count);
        }

        [TestMethod]
        public async Task CreatedAuthor_WhenWSend_An_Author()
        {
            // Preparacion
            var nuevoAuthor = new AuthorCreationDTO { Names = "nuevo", LastNames = "author"}; 

            // Prueba
            var respuesta = await controller.Post(nuevoAuthor);

            // Verificacion
            var resultado = respuesta as CreatedAtRouteResult;
            Assert.IsNotNull(resultado);

            var contexto2 = ConstruirContext(nombreDB);
            var amount = await contexto2.Authors.CountAsync();
            Assert.AreEqual(expected: 1, actual: amount);
        }

        [TestMethod]
        public async Task Put_Retorna404_CuandoAuthorNoExiste()
        {
            // Prueba
            var respuesta = await controller.Put(1, authorCreationDTO: null!);

            // Verificacion
            var resultado = respuesta as StatusCodeResult;
            Assert.AreEqual(404, resultado!.StatusCode);
        }

        [TestMethod]
        public async Task Put_ActualizarAuthor_CuandoEnviemosAuthorSinFoto()
        {
            // Preparacion 
            var context = ConstruirContext(nombreDB);

            context.Authors.Add(new Author
            {
                Names = "Luis",
                LastNames = "Escalante",
                Identification = "Id"
            });

            await context.SaveChangesAsync();

            var authorCreationDTO = new AuthorCreationDTOWithPhoto
            {
                Names = "Luis2",
                LastNames = "Escalante2",
                Identification = "Id2"
            };
            // Prueba
            var respuesta = await controller.Put(1, authorCreationDTO);

            // Verificacion
            var resultado = respuesta as StatusCodeResult;
            Assert.AreEqual(204, resultado!.StatusCode);

            var context3 = ConstruirContext(nombreDB);
            var authorActualizado = await context3.Authors.SingleAsync();

            Assert.AreEqual(expected: "Luis2", actual: authorActualizado.Names);
            Assert.AreEqual(expected: "Escalante2", actual: authorActualizado.LastNames);
            Assert.AreEqual(expected: "Id2", actual: authorActualizado.Identification);
            await outputCacheStore.Received(1).EvictByTagAsync(cache, default);
            await almacenadorArchivos.DidNotReceiveWithAnyArgs().Edit(default, default!, default!);
        }

        [TestMethod]
        public async Task Put_ActualizarAuthor_CuandoEnviemosAuthorConFoto()
        {
            // Preparacion 
            var context = ConstruirContext(nombreDB);

            var urlAnterior = "URL-1";
            var urlNueva = "URL-2";
            almacenadorArchivos.Edit(default!, default!, default!).ReturnsForAnyArgs(urlNueva);

            context.Authors.Add(new Author
            {
                Names = "Luis",
                LastNames = "Escalante",
                Identification = "Id",
                Photo = urlAnterior
            });

            await context.SaveChangesAsync();

            var formFile = Substitute.For<IFormFile>();

            var authorCreationDTO = new AuthorCreationDTOWithPhoto
            {
                Names = "Luis2",
                LastNames = "Escalante2",
                Identification = "Id2",
                Photo = formFile
            };
            // Prueba
            var respuesta = await controller.Put(1, authorCreationDTO);

            // Verificacion
            var resultado = respuesta as StatusCodeResult;
            Assert.AreEqual(204, resultado!.StatusCode);

            var context3 = ConstruirContext(nombreDB);
            var authorActualizado = await context3.Authors.SingleAsync();

            Assert.AreEqual(expected: "Luis2", actual: authorActualizado.Names);
            Assert.AreEqual(expected: "Escalante2", actual: authorActualizado.LastNames);
            Assert.AreEqual(expected: "Id2", actual: authorActualizado.Identification);
            Assert.AreEqual(expected: urlNueva, actual: authorActualizado.Photo);
            await outputCacheStore.Received(1).EvictByTagAsync(cache, default);
            await almacenadorArchivos.Received(1).Edit(urlAnterior, container, formFile);
        }

        [TestMethod]
        public async Task Patch_Retorna400_CuandoPatchDocEsNulo()
        {
            // Prueba
            var respuesta = await controller.Patch(1, patchDTO: null!);

            // Verificacion
            var resultado = respuesta as StatusCodeResult;
            Assert.AreEqual(400, resultado!.StatusCode);

        }

        [TestMethod]
        public async Task Patch_Retorna404_CuandoAutorNoExiste()
        {
            // Preparacion
            var patchDTO = new JsonPatchDocument<AuthorPatchDTO>();

            // Prueba
            var respuesta = await controller.Patch(1, patchDTO);

            // Verificacion
            var resultado = respuesta as StatusCodeResult;
            Assert.AreEqual(404, resultado!.StatusCode);

        }

        [TestMethod]
        public async Task Patch_RetornaValidationProblem_CuandoHayErrorDeValidacion()
        {
            // Preparacion
            var context = ConstruirContext(nombreDB);
            context.Authors.Add(new Author
            {
                Names = "Luis",
                LastNames = "Escalante",
                Identification = "123"
            });

            await context.SaveChangesAsync();

            var objectValidator = Substitute.For<IObjectModelValidator>();
            controller.ObjectValidator = objectValidator;

            var mensajeDeError = "mensaje de error";
            controller.ModelState.AddModelError("", mensajeDeError);

            var patchDoc = new JsonPatchDocument<AuthorPatchDTO>();

            // Prueba
            var respuesta = await controller.Patch(1, patchDoc);

            // Verificacion
            var resultado = respuesta as ObjectResult;
            var problemDetails = resultado!.Value as ValidationProblemDetails;
            Assert.IsNotNull(problemDetails);
            Assert.AreEqual(expected: 1, actual: problemDetails.Errors.Keys.Count);
            Assert.AreEqual(expected: mensajeDeError, actual: problemDetails.Errors.Values.First().First());

        }

        [TestMethod]
        public async Task Patch_ActualizarUnCampo_CuandoSeleEnvieUnaOperacion()
        {
            // Preparacion
            var context = ConstruirContext(nombreDB);
            context.Authors.Add(new Author
            {
                Names = "Luis",
                LastNames = "Escalante",
                Identification = "123",
                Photo = "URL-1"
            });

            await context.SaveChangesAsync();

            var objectValidator = Substitute.For<IObjectModelValidator>();
            controller.ObjectValidator = objectValidator;

            var patchDoc = new JsonPatchDocument<AuthorPatchDTO>();
            patchDoc.Operations.Add(new Operation<AuthorPatchDTO>("replace", "/names", null, "Luis2"));

            // Prueba
            var respuesta = await controller.Patch(1, patchDoc);

            // Verificacion
            var resultado = respuesta as StatusCodeResult;
            Assert.AreEqual(expected: 204, resultado!.StatusCode);

            await outputCacheStore.Received(1).EvictByTagAsync(cache, default);

            var context2 = ConstruirContext(nombreDB);
            var authorDB = await context2.Authors.SingleAsync();

            Assert.AreEqual(expected: "Luis2", actual: authorDB.Names);
            Assert.AreEqual(expected: "Escalante", actual: authorDB.LastNames);
            Assert.AreEqual(expected: "123", actual: authorDB.Identification);
            Assert.AreEqual(expected: "URL-1", actual: authorDB.Photo);
        }

        [TestMethod]
        public async Task Delete_Retornar404_CuandoAuthorNoExiste()
        {
            // Prueba
            var respuesta = await controller.Delete(1);

            // Verificacion
            var resultado = respuesta as StatusCodeResult;
            Assert.AreEqual(404, resultado!.StatusCode);
        }

        [TestMethod]
        public async Task Delete_Retornar204_CuandoAuthorExiste()
        {
            // Preparacion 
            var urlPhoto = "URL-1";     

            var   context = ConstruirContext(nombreDB);

            context.Authors.Add(new Author { Names = "Author1", LastNames = "Author1", Photo = urlPhoto });
            context.Authors.Add(new Author { Names = "Author2", LastNames = "Author2" });

            await context.SaveChangesAsync();

            // Prueba
            var respuesta = await controller.Delete(1);

            // Verificacion
            var resultado = respuesta as StatusCodeResult;
            Assert.AreEqual(204, resultado!.StatusCode);

            var context2 = ConstruirContext(nombreDB);
            var authorsAmount = await context2.Authors.CountAsync();
            Assert.AreEqual(expected: 1, actual: authorsAmount);   

            var author2Exist = await context2.Authors.AnyAsync(x => x.Names == "Author2");
            Assert.IsTrue(author2Exist);

            await  outputCacheStore.Received(1).EvictByTagAsync(cache, default);
            await almacenadorArchivos.Received(1).Delete(urlPhoto, container);                          
        }
    }
}