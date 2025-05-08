using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryAPI.Controllers.v1;
using LibraryAPI.DTOs;
using LibraryAPI.Entities;
using LibraryAPI.Services;
using LibraryAPITests.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using NSubstitute;

namespace LibraryAPITests.UnitTest.Controllers.V1
{
    [TestClass]
    public class UsersControllersTests: TestsBase
    {
        private string nombreDB = Guid.NewGuid().ToString();
        private UserManager<User> userManager = null!;
        private SignInManager<User> signInManager = null!;
        private UserControllers controller = null!;

        [TestInitialize]
        public void Setup()
        {
            var context = ConstruirContext(nombreDB);
            userManager = Substitute.For<UserManager<User>>(
                Substitute.For<IUserStore<User>>(), null, null, null, null, null, null, null, null);

            var miConfiguracion = new Dictionary<string, string>
            {
                {
                    "keyjwt", "askaslkalskaljlkajlkjlkajlksjlkllkldldsldkslds"
                }
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(miConfiguracion!)
                .Build();

            var contextAccessor = Substitute.For<IHttpContextAccessor>();
            var userClaimsFactory = Substitute.For<IUserClaimsPrincipalFactory<User>>();

            signInManager = Substitute.For<SignInManager<User>>(userManager, 
                contextAccessor, userClaimsFactory, null, null, null, null);

            var servicioUsuario = Substitute.For<IServicesUser>();

            var mapper = ConfigurarAutoMapper();

            controller = new UserControllers(userManager, configuration, signInManager,
                servicioUsuario, context, mapper);
        }

        [TestMethod]
        public async Task Registrar_DevuelveValidationProblem_CuandoNoEsExitoso()
        {
            // Preparacion
            var mensajeError = "Prueba";
            var credenciales = new UserCredentialsDTO
            {
                Email = "prueba@hotmial.com",
                Password = "aA123456!"
            };

            userManager.CreateAsync(Arg.Any<User>(), Arg.Any<string>())
                        .Returns(IdentityResult.Failed(new IdentityError
                        {
                            Code = "prueba",
                            Description = mensajeError
                        }));

            // Prueba
            var respuesta = await controller.Register(credenciales);

            // Verificacion
            var resultado = respuesta.Result as ObjectResult;
            var problemDetails = resultado!.Value as ValidationProblemDetails;
            Assert.IsNotNull(problemDetails);
            Assert.AreEqual(expected: 1, actual: problemDetails.Errors.Keys.Count);
            Assert.AreEqual(expected: mensajeError, actual: problemDetails.Errors.Values.First().First());
        }

        [TestMethod]
        public async Task Registrar_DevuelveToken_CuandoEsExitoso()
        {
            // Preparacion
            var credenciales = new UserCredentialsDTO
            {
                Email = "prueba@hotmial.com",
                Password = "aA123456!"
            };

            userManager.CreateAsync(Arg.Any<User>(), Arg.Any<string>())
                        .Returns(IdentityResult.Success);

            // Prueba
            var respuesta = await controller.Register(credenciales);

            // Verificacion
            Assert.IsNotNull(respuesta.Value);
            Assert.IsNotNull(respuesta.Value.Token);
        }

        [TestMethod]
        public async Task Login_DevuelveValidationProblem_CuandoUsuarioNoExiste()
        {
            // Preparacion
           var credenciales = new UserCredentialsDTO
            {
                Email = "prueba@hotmial.com",
                Password = "aA123456!"
            };

            userManager.FindByEmailAsync(credenciales.Email)!.Returns(Task.FromResult<User>(null!));   

            // Prueba

            var respuesta = await controller.Login(credenciales);

            // 
            var resultado = respuesta.Result as ObjectResult;
            var problemDetails = resultado!.Value as ValidationProblemDetails;
            Assert.IsNotNull(problemDetails);
            Assert.AreEqual(expected: 1, actual: problemDetails.Errors.Keys.Count);
            Assert.AreEqual(expected: "Incorrect login",
                    actual: problemDetails.Errors.Values.First().First());
        }

        [TestMethod]
        public async Task Login_DevuelveValidationProblem_CuandoLoginEsIncorrecto()
        {
            // Preparacion
           var credenciales = new UserCredentialsDTO
            {
                Email = "prueba@hotmial.com",
                Password = "aA123456!"
            };

            var usuario = new User { Email = credenciales.Email };

            userManager.FindByEmailAsync(credenciales.Email)!.Returns(Task.FromResult<User>(usuario)); 

            signInManager.CheckPasswordSignInAsync(usuario, credenciales.Password, false)
                         .Returns(Microsoft.AspNetCore.Identity.SignInResult.Failed);   

            // Prueba

            var respuesta = await controller.Login(credenciales);

            // 
            var resultado = respuesta.Result as ObjectResult;
            var problemDetails = resultado!.Value as ValidationProblemDetails;
            Assert.IsNotNull(problemDetails);
            Assert.AreEqual(expected: 1, actual: problemDetails.Errors.Keys.Count);
            Assert.AreEqual(expected: "Incorrect login",
                    actual: problemDetails.Errors.Values.First().First());
        }

        [TestMethod]
        public async Task Login_DevuelveToken_CuandoLoginEsCorrecto()
        {
            // Preparacion
           var credenciales = new UserCredentialsDTO
            {
                Email = "prueba@hotmial.com",
                Password = "aA123456!"
            };

            var usuario = new User { Email = credenciales.Email };

            userManager.FindByEmailAsync(credenciales.Email)!.Returns(Task.FromResult<User>(usuario)); 

            signInManager.CheckPasswordSignInAsync(usuario, credenciales.Password, false)
                         .Returns(Microsoft.AspNetCore.Identity.SignInResult.Success);   

            // Prueba

            var respuesta = await controller.Login(credenciales);

            // Verificacion
           Assert.IsNotNull(respuesta.Value);
           Assert.IsNotNull(respuesta.Value!.Token);
        }
    }
}     