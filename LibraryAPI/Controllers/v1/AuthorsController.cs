using AutoMapper;
using LibraryAPI.Data;
using LibraryAPI.DTOs;
using LibraryAPI.Entities;
using LibraryAPI.Services;
using LibraryAPI.Services.v1;
using LibraryAPI.Utilities;
using LibraryAPI.Utilities.V1;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Linq.Dynamic.Core;

namespace LibraryAPI.Controllers.v1
{
    [ApiController]
    [Route("api/v1/authors")]
    [Authorize(Policy = "esadmin")]
    [FilterAddHeaders("controllers", "authors")]
    public class AuthorsController: ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IFileStorage fileStorage;
        private readonly ILogger<AuthorsController> logger;
        private readonly IOutputCacheStore outputCacheStore;
        private readonly IServiceAuthors serviceAuthorsV1;
        private const string container = "authors";
        private const string cache = "authors-obtener";

        public AuthorsController(ApplicationDbContext context, IMapper mapper, 
            IFileStorage fileStorage, ILogger<AuthorsController> logger, 
            IOutputCacheStore outputCacheStore, IServiceAuthors serviceAuthorsV1)
        {
            this.context = context;
            this.mapper = mapper;
            this.fileStorage = fileStorage;
            this.logger = logger;
            this.outputCacheStore = outputCacheStore;
            this.serviceAuthorsV1 = serviceAuthorsV1;
        }

        /* and either we can use both /list-all-autores or inherit /api/autores */
        [HttpGet(Name = "GetAuthorsV1")] // /api/authors
        [AllowAnonymous]
        //[OutputCache(Tags = [cache])] // With this attribute apply cache on this action
        [ServiceFilter<MyActionFilter>()]// Action Filters
        [FilterAddHeaders("actions", "obtain-authors")] // Attribute filters
        [ServiceFilter<HATEOASAuthorsAttribute>()]
        public async Task<IEnumerable<AuthorDTO>> Get(
            [FromQuery] PaginationDTO paginationDTO,
            [FromQuery] bool includeHATEOAS = false)
        {
            //throw new NotImplementedException();
            //return await context.Authors.ToListAsync();
            /* When Manually mapp the Entity props to Dto Model 
            var authorsDTO = authors.Select(author =>
                                            new AuthorDTO
                                            {
                                                Id = author.Id,
                                                FullName = $"{author.Names} {author.LastNames}"  String interpolation
                                            });
            */
            //var authors = await context.Authors.ToListAsync();
            // return new List<AuthorDTO>();
            // var newPaginationDTO = new PaginationDTO(1,1);
            // return await serviceAuthorsV1.Get(newPaginationDTO);
            return await serviceAuthorsV1.Get(paginationDTO);
        }

        [HttpGet("{id:int}", Name = "RetrieveAuthorV1")] // api/autores/id
        //All These attributes below helps makes endpoints in Swagger more descriptive 
        [AllowAnonymous]
        [EndpointSummary("Retrieve author by id")]
        [EndpointDescription("This endpoint gets an author by id, and its books, and return a 401 if the author was not found")]
        [ProducesResponseType<AuthorWithBooksDTO>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        //[OutputCache(Tags = [cache])]
        [ServiceFilter<HATEOASAuthorAttribute>()]
        public async Task<ActionResult<AuthorWithBooksDTO>> Get([Description("the author id")] int id)
        {
            var author = await context.Authors
                .Include(x => x.Books)
                    .ThenInclude(x => x.Book) // This method allow to navigate into the Books type(AuthorsBook) and obtain the Book object
                .FirstOrDefaultAsync(x => x.Id == id);

            if (author is null)
            {
                return NotFound();
            }

            var authorDTO = mapper.Map<AuthorWithBooksDTO>(author);

            // GenerateLinks(authorDTO);

            return authorDTO;
        }

        private void GenerateLinks(AuthorDTO authorDTO)
        {
            authorDTO.Links.Add(
                new DatasHATEOASDTO(
                    Link: Url.Link("RetrieveAuthorV1", new { id = authorDTO.Id })!,
                    Description: "self",
                    Method: "GET"));

            authorDTO.Links.Add(
                new DatasHATEOASDTO(
                    Link: Url.Link("UpdateAuthorV1", new { id = authorDTO.Id })!,
                    Description: "author-update",
                    Method: "PUT"));

            authorDTO.Links.Add(
                new DatasHATEOASDTO(
                    Link: Url.Link("PatchAuthorV1", new { id = authorDTO.Id })!,
                    Description: "author-patch",
                    Method: "PATCH"));

            authorDTO.Links.Add(
                new DatasHATEOASDTO(
                    Link: Url.Link("DeleteAuthorV1", new { id = authorDTO.Id })!,
                    Description: "author-delete",
                    Method: "DELETE"));
        }

        [HttpGet("filter", Name = "FilterAuthorsV1")]
        [AllowAnonymous]
        public async Task<ActionResult> Filter([FromQuery] AuthorFilterDTO authorFilterDTO)
        {
            var queryable = context.Authors.AsQueryable();

            if (!string.IsNullOrEmpty(authorFilterDTO.Names))
            {
                queryable = queryable.Where(x => x.Names.Contains(authorFilterDTO.Names));
            }

            if (!string.IsNullOrEmpty(authorFilterDTO.LastNames))
            {
                queryable = queryable.Where(x => x.LastNames.Contains(authorFilterDTO.LastNames));
            }

            if (authorFilterDTO.IncludeBooks)
            {
                queryable = queryable.Include(x => x.Books).ThenInclude(x => x.Book);
            }

            if (authorFilterDTO.IsHasPhoto.HasValue)
            {
                if (authorFilterDTO.IsHasPhoto.Value)
                {
                    queryable = queryable.Where(x => x.Photo != null);
                }
                else
                {
                    queryable = queryable.Where(x => x.Photo == null);
                }
            }

            if (authorFilterDTO.IsHasBooks.HasValue)
            {
                if (authorFilterDTO.IsHasBooks.Value)
                {
                    queryable = queryable.Where(x => x.Books.Any());
                }
                else
                {
                    queryable = queryable.Where(x => !x.Books.Any());
                }
            }

            if (!string.IsNullOrEmpty(authorFilterDTO.BookTitle))
            {
                queryable = queryable.Where(x =>
                    x.Books.Any(y => y.Book!.Title.Contains(authorFilterDTO.BookTitle)));
            }

            if (!string.IsNullOrEmpty(authorFilterDTO.OrderField))
            {
                var orderType = authorFilterDTO.IsAscendingOrder ? "ascending" : "descending";

                try
                {
                    queryable = queryable.OrderBy($"{authorFilterDTO.OrderField} {orderType}");
                }
                catch (Exception ex)
                {
                    queryable = queryable.OrderBy(x => x.Names);
                    logger.LogError(ex.Message, ex);
                }
            }
            else
            {
                queryable = queryable.OrderBy(x => x.Names);
            }

                var authors = await queryable
                        .Page(authorFilterDTO.PaginationDTO).ToListAsync();

            if (authorFilterDTO.IncludeBooks)
            {
                var authorsWithBooksDTO = mapper.Map<IEnumerable<AuthorWithBooksDTO>>(authors);
                return Ok(authorsWithBooksDTO);
            }
            else
            {
                var authorsDTO = mapper.Map<IEnumerable<AuthorDTO>>(authors);
                return Ok(authorsDTO);
            }
                
        }

        [HttpPost(Name = "CreateAuthorV1")]
        public async Task<ActionResult> Post(AuthorCreationDTO authorCreationDTO)
        {
            var author = mapper.Map<Author>(authorCreationDTO);
            context.Add(author);
            await context.SaveChangesAsync();
            await outputCacheStore.EvictByTagAsync(cache, default);
            var authorDTO = mapper.Map<AuthorDTO>(author);
            return CreatedAtRoute("RetrieveAuthorV1", new { id = author.Id }, authorDTO);
        }

        [HttpPost("with-photo", Name = "CreateAuthorWithPhotoV1")]
        public async Task<ActionResult> PostWithPhoto([FromForm] AuthorCreationDTOWithPhoto authorCreationDTOWithPhoto)
        {
            var author = mapper.Map<Author>(authorCreationDTOWithPhoto);

            if (authorCreationDTOWithPhoto.Photo is not null)
            {
                var url = await fileStorage.Storage(container,
                    authorCreationDTOWithPhoto.Photo);
                author.Photo = url;
            }

            context.Add(author);
            await context.SaveChangesAsync();
            await outputCacheStore.EvictByTagAsync(cache, default);
            var authorDTO = mapper.Map<AuthorDTO>(author);
            return CreatedAtRoute("RetrieveAuthorV1", new { id = author.Id }, authorDTO);
        }

        [HttpPut]

        [HttpPut("{id:int}", Name = "UpdateAuthorV1")]
        public async Task<ActionResult> Put(int id, 
            [FromForm] AuthorCreationDTOWithPhoto authorCreationDTO)
        {
            var isAuthorExist = await context.Authors.AnyAsync(x => x.Id == id);

            if (!isAuthorExist)
            {
                return NotFound();
            }

            var author = mapper.Map<Author>(authorCreationDTO);
            author.Id = id;

            if (authorCreationDTO.Photo is not null)
            {
                var currentPhoto = await context
                                         .Authors.Where(x => x.Id == id)
                                         .Select(x => x.Photo).FirstAsync();

                var url = await fileStorage.Edit(currentPhoto, container,
                    authorCreationDTO.Photo);
                author.Photo = url;
            }

            context.Update(author);
            await context.SaveChangesAsync();
            await outputCacheStore.EvictByTagAsync(cache, default);
            return NoContent();
        }

        [HttpPatch("{id:int}", Name = "PatchAuthorV1")]
        public async Task<ActionResult> Patch(int id, JsonPatchDocument<AuthorPatchDTO> patchDTO)
        {
            if(patchDTO is null)
            {
                return BadRequest();
            }

            var authorDB = await context.Authors.FirstOrDefaultAsync(x => x.Id == id);

            if(authorDB is null)
            {
                return NotFound();
            }
            // The Mapp is from Author Entity to AuthorPatchDTO
            var authorPatchDTO = mapper.Map<AuthorPatchDTO>(authorDB);

            patchDTO.ApplyTo(authorPatchDTO, ModelState);

            var isValid = TryValidateModel(authorPatchDTO);

            if(!isValid)
            {
                return ValidationProblem();
            }

            // The Mapp is from AuthorPatchDTO to Author 
            mapper.Map(authorPatchDTO, authorDB);

            await context.SaveChangesAsync();

            await outputCacheStore.EvictByTagAsync(cache, default);

            return NoContent();

        }


        [HttpDelete("{id:int}", Name = "DeleteAuthorV1")]
        public async Task<ActionResult> Delete(int id)
        {
            //var deletedRecords = await context.Authors.Where(x => x.Id == id).ExecuteDeleteAsync();

            //if (deletedRecords == 0)
            //{
            //    return NotFound();
            //}

            //return Ok();
            var author = await context.Authors.FirstOrDefaultAsync(x => x.Id == id);

            if (author is null)
            {
                return NotFound();
            }

            context.Remove(author);
            await context.SaveChangesAsync();
            await outputCacheStore.EvictByTagAsync(cache, default);
            await fileStorage.Delete(author.Photo, container);

            return NoContent();
        }
    }
}
