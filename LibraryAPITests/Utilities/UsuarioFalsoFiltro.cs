using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LibraryAPITests.Utilities
{
    public class UsuarioFalsoFiltro : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, 
            ActionExecutionDelegate next)
        {
            // Before the execution
            context.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
            {
                new Claim("claim", "ejemplo@hotmail.com")
            }, "prueba"));

            await next();

            // After the execution
        }
    }
}