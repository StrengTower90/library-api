using LibraryAPI.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace LibraryAPI.Services.v1
{
    public class GeneradorEnlaces : IGeneradorEnlaces
    {
        private readonly LinkGenerator linkGenerator;
        private readonly IAuthorizationService authorizationService;
        private readonly IHttpContextAccessor httpContextAccessor;

        public GeneradorEnlaces(LinkGenerator linkGenerator,
            IAuthorizationService authorizationService, IHttpContextAccessor httpContextAccessor)
        {
            this.linkGenerator = linkGenerator;
            this.authorizationService = authorizationService;
            this.httpContextAccessor = httpContextAccessor;
        }

        public async Task GenerarEnlaces(AuthorDTO authorDTO)
        {
            var usuario = httpContextAccessor.HttpContext!.User;
            var isAdmin = await authorizationService.AuthorizeAsync(usuario, "esAdmin");
            GenerarEnlaces(authorDTO, isAdmin.Succeeded);
        }

        public async Task<CollectionOfResourcesDTO<AuthorDTO>> 
        GenerarEnlaces(List<AuthorDTO> authors)
        {
            var results = new CollectionOfResourcesDTO<AuthorDTO> { Values = authors };

            var usuario = httpContextAccessor.HttpContext!.User;
            var isAdmin = await authorizationService.AuthorizeAsync(usuario, "esadmin");

            foreach (var dto in authors)
            {
                GenerarEnlaces(dto, isAdmin.Succeeded); 
            }

            results.Links.Add(new DatasHATEOASDTO(
                Link: linkGenerator.GetUriByRouteValues(httpContextAccessor.HttpContext!,
                "GetAuthorsV1", new { })!,
                Description: "self",
                Method: "GET"));

            if (isAdmin.Succeeded)
            {
                results.Links.Add(new DatasHATEOASDTO(
                    Link: linkGenerator.GetUriByRouteValues(httpContextAccessor.HttpContext!,
                    "CreateAuthorV1", new { })!,
                    Description: "author-create",
                    Method: "POST"));

                results.Links.Add(new DatasHATEOASDTO(
                    Link: linkGenerator.GetUriByRouteValues(httpContextAccessor.HttpContext!,
                    "CreateAuthorWithPhotoV1", new { })!,
                    Description: "author-create-with-photo",
                    Method: "POST"));
            }

            return results;
        }

        private void GenerarEnlaces(AuthorDTO authorDTO, bool isAdmin)
        {
            authorDTO.Links.Add(
                new DatasHATEOASDTO(
                    Link: linkGenerator.GetUriByRouteValues(httpContextAccessor.HttpContext!,
                    "RetrieveAuthorV1", new { id = authorDTO.Id })!,
                    Description: "self",
                    Method: "GET"));

            if (isAdmin)
            {
                authorDTO.Links.Add(
                new DatasHATEOASDTO(
                    Link: linkGenerator.GetUriByRouteValues(httpContextAccessor.HttpContext!,
                    "UpdateAuthorV1", new { id = authorDTO.Id })!,
                    Description: "author-update",
                    Method: "PUT"));

                authorDTO.Links.Add(
                    new DatasHATEOASDTO(
                        Link: linkGenerator.GetUriByRouteValues(httpContextAccessor.HttpContext!,
                        "PatchAuthorV1", new { id = authorDTO.Id })!,
                        Description: "author-patch",
                        Method: "PATCH"));

                authorDTO.Links.Add(
                    new DatasHATEOASDTO(
                        Link: linkGenerator.GetUriByRouteValues(httpContextAccessor.HttpContext!,
                        "DeleteAuthorV1", new { id = authorDTO.Id })!,
                        Description: "author-delete",
                        Method: "DELETE"));
            }


        }
    }
}