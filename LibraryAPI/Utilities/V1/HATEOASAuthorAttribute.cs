using LibraryAPI.DTOs;
using LibraryAPI.Services.v1;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LibraryAPI.Utilities.V1
{
    class HATEOASAuthorAttribute: HATEOASFilterAttribute
    {
        private readonly IGeneradorEnlaces generadorEnlaces;

        public HATEOASAuthorAttribute(IGeneradorEnlaces generadorEnlaces)
        {
            this.generadorEnlaces = generadorEnlaces;
        }

        public override async Task OnResultExecutionAsync
        (ResultExecutingContext context, ResultExecutionDelegate next)
        {
           var includeHATEOAS = ItMustToIncludeHATEOAS(context);

           if (!includeHATEOAS)
           {
                await next();
                return;
           }

           var result = context.Result as ObjectResult;
           var modelo = result!.Value as AuthorDTO ?? 
                throw new ArgumentNullException("An instance of AuthorDTO was expected");
            await generadorEnlaces.GenerarEnlaces(modelo);
            await next();
        }
    }
}