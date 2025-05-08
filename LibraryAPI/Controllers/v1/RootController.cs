using LibraryAPI.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryAPI.Controllers.v1
{
    [ApiController]
    [Route("api/v1")]
    [Authorize]
    public class RootController: ControllerBase
    {
        private readonly IAuthorizationService authorizationService;

        public RootController(IAuthorizationService authorizationService)
        {
            this.authorizationService = authorizationService;
        }

        [HttpGet(Name = "GetRootV1")]
        [AllowAnonymous]
        public async Task<IEnumerable<DatasHATEOASDTO>> Get()
        {
            var datasHATEOASDTO = new List<DatasHATEOASDTO>();

            var isAdmin = await authorizationService.AuthorizeAsync(User, "esadmin");

            // Actions that anyone can do

            datasHATEOASDTO.Add(new DatasHATEOASDTO(Link: Url.Link("GetRootV1", new { })!,
                Description: "self", Method: "GET"));

            datasHATEOASDTO.Add(new DatasHATEOASDTO(Link: Url.Link("GetAuthorsV1", new { })!,
                Description: "authors-get", Method: "GET"));

            datasHATEOASDTO.Add(new DatasHATEOASDTO(Link: Url.Link("GetCommentsV1", new { })!,
                Description: "comments-get", Method: "GET"));

            datasHATEOASDTO.Add(new DatasHATEOASDTO(Link: Url.Link("RetrieveCommentV1", new { })!,
                Description: "comment-retrieve", Method: "GET"));

            datasHATEOASDTO.Add(new DatasHATEOASDTO(Link: Url.Link("CreateCommentV1", new { })!,
                Description: "comment-create", Method: "POST"));

            datasHATEOASDTO.Add(new DatasHATEOASDTO(Link: Url.Link("PatchCommentV1", new { })!,
                Description: "comment-patch", Method: "PATCH"));

            datasHATEOASDTO.Add(new DatasHATEOASDTO(Link: Url.Link("DeleteCommentV1", new { })!,
                Description: "comment-patch", Method: "DELETE"));

            datasHATEOASDTO.Add(new DatasHATEOASDTO(Link: Url.Link("LoginUserV1", new { })!,
                Description: "users-login", Method: "POST"));

            datasHATEOASDTO.Add(new DatasHATEOASDTO(Link: Url.Link("GetUsersV1", new { })!,
                Description: "users-get", Method: "GET"));
            

            if (User.Identity!.IsAuthenticated)
            {
                // Actions to Logged user
                datasHATEOASDTO.Add(new DatasHATEOASDTO(Link: Url.Link("UpdateUserV1", new { })!,
                                Description: "users-update", Method: "PUT"));

                datasHATEOASDTO.Add(new DatasHATEOASDTO(Link: Url.Link("RenewTokenV1", new { })!,
                                Description: "users-renew-token", Method: "GET"));
            }


            // Actions that only user with esadmin claim can do
            if (isAdmin.Succeeded)
            {
                datasHATEOASDTO.Add(new DatasHATEOASDTO(Link: Url.Link("CreateAuthorV1", new { })!,
                Description: "author-create", Method: "POST"));

                datasHATEOASDTO.Add(new DatasHATEOASDTO(Link: Url.Link("CreateAuthorsV1", new { })!,
                    Description: "authors-create", Method: "POST"));

                datasHATEOASDTO.Add(new DatasHATEOASDTO(Link: Url.Link("CreateBookV1", new { })!,
                    Description: "book-create", Method: "POST"));

                datasHATEOASDTO.Add(new DatasHATEOASDTO(Link: Url.Link("CreateUsersV1", new { })!,
               Description: "users-create", Method: "GET"));

                datasHATEOASDTO.Add(new DatasHATEOASDTO(Link: Url.Link("AddAdminV1", new { })!,
                Description: "users-add-admin", Method: "GET"));

                datasHATEOASDTO.Add(new DatasHATEOASDTO(Link: Url.Link("RemoveAdminV1", new { })!,
                    Description: "users-remove-admin", Method: "GET"));

            }

            return datasHATEOASDTO;
        }
    }
}
