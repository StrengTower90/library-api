using AutoMapper;
using LibraryAPI.Data;
using LibraryAPI.DTOs;
using LibraryAPI.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LibraryAPITests.Utilities
{
    public class TestsBase
    {
        protected readonly JsonSerializerOptions jsonSerializerOptions 
            = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        protected readonly Claim adminClaim = new Claim("esadmin", "1");

        protected ApplicationDbContext ConstruirContext(string nameDB)
        {
            var options = new  DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(nameDB).Options;

            var dbContext = new ApplicationDbContext(options);
            return dbContext;
        }

        protected IMapper ConfigurarAutoMapper()
        {
            var config = new MapperConfiguration(options => 
            {
                options.AddProfile(new AutoMapperProfiles());
            });

            return config.CreateMapper();
        }

       protected WebApplicationFactory<Program> ConstruirWebApplicationFactory(string nombreDB, 
            bool ignorarSeguridad = true)
        {
            var factory = new WebApplicationFactory<Program>();

            factory = factory.WithWebHostBuilder(builder => 
            {
                builder.ConfigureTestServices(services =>
                {
                    // this snipped code remove the SQL server provider that allow to use the new provider in memory
                    ServiceDescriptor descriptorDBContext = services.SingleOrDefault(
                        d => d.ServiceType == typeof(IDbContextOptionsConfiguration<ApplicationDbContext>))!;
                    
                    if (descriptorDBContext is not null)
                    {
                        services.Remove(descriptorDBContext);
                    }

                    services.AddDbContext<ApplicationDbContext>(opciones =>
                        opciones.UseInMemoryDatabase(nombreDB));

                    if (ignorarSeguridad)
                    {
                        services.AddSingleton<IAuthorizationHandler, AllowAnonymousHandler>();

                        services.AddControllers(opciones =>
                        {
                            opciones.Filters.Add(new UsuarioFalsoFiltro());
                        });
                    }
                });
            });

            return factory;
        }
       
       protected async Task<string> CrearUsuario(string nombreDB, WebApplicationFactory<Program> factory)
            => await CrearUsuario(nombreDB, factory, [], "ejemplo@hotmail.com");

       protected async Task<string> CrearUsuario(string nombreDB, WebApplicationFactory<Program> factory,
            IEnumerable<Claim> claims)
            => await CrearUsuario(nombreDB, factory, claims, "ejemplo@hotmail.com");

       protected async Task<string> CrearUsuario(string nombreDB, WebApplicationFactory<Program> factory, 
            IEnumerable<Claim> claims, string email)
       {
            var urlRegistro = "/api/v1/users/register";
            string token = string.Empty;
            token = await ObtenerToken(email, urlRegistro, factory);

            if (claims.Any())
            {
                var context = ConstruirContext(nombreDB);
                var usuario = await context.Users.Where(x => x.Email == email).FirstAsync();
                Assert.IsNotNull(usuario);

                var userClaims = claims.Select(x => new IdentityUserClaim<string>
                {
                    UserId = usuario.Id,
                    ClaimType = x.Type,
                    ClaimValue = x.Value
                });

                context.UserClaims.AddRange(userClaims);
                await context.SaveChangesAsync();
                var urlLogin = "/api/v1/users/login";
                token = await ObtenerToken(email, urlLogin, factory);
            }

            return token;
       }

       private async Task<string> ObtenerToken(string email, string url, 
            WebApplicationFactory<Program> factory)
       {
           var password = "aA123456!";
           var credenciales = new UserCredentialsDTO { Email = email, Password = password };
           var cliente = factory.CreateClient();
           var respuesta = await cliente.PostAsJsonAsync(url, credenciales);
           respuesta.EnsureSuccessStatusCode();

           var contenido = await respuesta.Content.ReadAsStringAsync();
           var respuestaAutenticacion = JsonSerializer.Deserialize<AuthenticationResponseDTO>(contenido, 
                jsonSerializerOptions)!;

           Assert.IsNotNull(respuestaAutenticacion.Token);

           return respuestaAutenticacion.Token;
       }
    }
}