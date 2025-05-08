using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using LibraryAPI.Entities;
using LibraryAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using NSubstitute;

namespace LibraryAPITests.UnitTest.Services
{
    [TestClass]
    public class UserServicesTests
    {
        private UserManager<User> userManager = null!;
        private IHttpContextAccessor contextAccessor = null!;
        private ServicesUser usersService = null!;

        [TestInitialize]
        public void Setup()
        {
            userManager = Substitute.For<UserManager<User>>(
                Substitute.For<IUserStore<User>>(), null, null, null, null, null, null, null, null);

            contextAccessor = Substitute.For<IHttpContextAccessor>();
            usersService = new ServicesUser(userManager, contextAccessor);
        }

        [TestMethod]
        public async Task ObtenerUsuario_RetornaNulo_CuandoNoHayClaimEmail()
        {
            // Preparacion
            var httpContext = new DefaultHttpContext();
             /* With the next method we are providing the expected HttpContext for the 
            int order testing */
            contextAccessor.HttpContext.Returns(httpContext);

            // Prueba
            var usuario = await usersService.GetUser();

            // Verificacion
            Assert.IsNull(usuario);
        }

        [TestMethod]
        public async Task ObtenerUsuario_RetornaUsuario_CuandoHayClaimEmail()
        {
            // Preparacion
            var email = "prueba@hotmail.com";
            var expectedUser = new User { Email = email };

            userManager.FindByEmailAsync(email)!.Returns(Task.FromResult(expectedUser));

            // Cadena de instanciaciones
            var claims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim("email", email)
            }));

            var httpContext = new DefaultHttpContext() { User = claims };
             /* With the next method we are providing the expected HttpContext for the 
            int order testing */
            contextAccessor.HttpContext.Returns(httpContext);

            // Prueba
            var usuario = await usersService.GetUser();

            // Verificacion
            Assert.IsNotNull(usuario);
            Assert.AreEqual(expected: email, actual: usuario.Email);
        }

                [TestMethod]
        public async Task ObtenerUsuario_RetornaNulo_CuandoUsuarioNoExiste()
        {
            // Preparacion
            var email = "prueba@hotmail.com";
            var expectedUser = new User { Email = email };

            userManager.FindByEmailAsync(email)!.Returns(Task.FromResult<User>(null!));

            // Cadena de instanciaciones
            var claims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim("email", email)
            }));

            var httpContext = new DefaultHttpContext() { User = claims };
             /* With the next method we are providing the expected HttpContext for the 
            int order testing */
            contextAccessor.HttpContext.Returns(httpContext);

            // Prueba
            var usuario = await usersService.GetUser();

            // Verificacion
            Assert.IsNull(usuario);
        }
    }
}