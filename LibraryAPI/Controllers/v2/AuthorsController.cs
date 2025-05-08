using AutoMapper;
using LibraryAPI.Data;
using LibraryAPI.DTOs;
using LibraryAPI.Entities;
using LibraryAPI.Services;
using LibraryAPI.Services.v1;
using LibraryAPI.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Linq.Dynamic.Core;

namespace LibraryAPI.Controllers.v2
{
    [ApiController]
    [Route("api/v2/authors")]
    [Authorize(Policy = "esadmin")]
    [FilterAddHeaders("controllers", "authors")]
    public class AuthorsController: ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IFileStorage fileStorage;
        private readonly ILogger<AuthorsController> logger;
        private readonly IOutputCacheStore outputCacheStore;
        private readonly IServiceAuthors serviceAuthorV1;
        private const string container = "authors";
        private const string cache = "authors-obtener";

        public AuthorsController(ApplicationDbContext context, IMapper mapper, 
            IFileStorage fileStorage, ILogger<AuthorsController> logger, 
            IOutputCacheStore outputCacheStore, IServiceAuthors serviceAuthorV1)
        {
            this.context = context;
            this.mapper = mapper;
            this.fileStorage = fileStorage;
            this.logger = logger;
            this.outputCacheStore = outputCacheStore;
            this.serviceAuthorV1 = serviceAuthorV1;
        }

        /* and either we can use both /list-all-autores or inherit /api/autores */
        [HttpGet] // /api/authors
        [AllowAnonymous]
        //[OutputCache(Tags = [cache])] // With this attribute apply cache on this action
        [ServiceFilter<MyActionFilter>()]// Action Filters
        [FilterAddHeaders("actions", "obtain-authors")] // Attribute filters
        public async Task<IEnumerable<AuthorDTO>> Get([FromQuery] PaginationDTO paginationDTO)
        {
            return await serviceAuthorV1.Get(paginationDTO);
        }

        [HttpGet("{id:int}", Name = "RetrieveAuthorV2")] // api/autores/id
        //All These attributes below helps makes endpoints in Swagger more descriptive 
        [AllowAnonymous]
        [EndpointSummary("Retrieve author by id")]
        [EndpointDescription("This endpoint gets an author by id, and its books, and return a 401 if the author was not found")]
        [ProducesResponseType<AuthorWithBooksDTO>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        //[OutputCache(Tags = [cache])]
        public async Task<ActionResult<AuthorWithBooksDTO>> Get(
            [Description("the author id")] int id, bool includeBooks = false)
        {
            var queryable = context.Authors.AsQueryable();

            if (includeBooks)
            {
                queryable = queryable.Include(x => x.Books)
                    .ThenInclude(x => x.Book); // This method allow to navigate into the Books type(AuthorsBook) and obtain the Book object
            }

            var author = await queryable.FirstOrDefaultAsync(x => x.Id == id);

            if (author is null)
            {
                return NotFound();
            }

            var authorDTO = mapper.Map<AuthorWithBooksDTO>(author);

            return authorDTO;
        }

        [HttpGet("filter")]
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

        [HttpPost]
        public async Task<ActionResult> Post(AuthorCreationDTO authorCreationDTO)
        {
            var author = mapper.Map<Author>(authorCreationDTO);
            context.Add(author);
            await context.SaveChangesAsync();
            await outputCacheStore.EvictByTagAsync(cache, default);
            var authorDTO = mapper.Map<AuthorDTO>(author);
            return CreatedAtRoute("RetrieveAuthorV2", new { id = author.Id }, authorDTO);
        }

        [HttpPost("with-photo")]
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
            return CreatedAtRoute("RetrieveAuthorV2", new { id = author.Id }, authorDTO);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, 
            [FromForm] AuthorCreationDTOWithPhoto authorCreationDTOWithPhoto)
        {
            var isAuthorExist = await context.Authors.AnyAsync(x => x.Id == id);

            if (!isAuthorExist)
            {
                return NotFound();
            }

            var author = mapper.Map<Author>(authorCreationDTOWithPhoto);
            author.Id = id;

            if (authorCreationDTOWithPhoto.Photo is not null)
            {
                var currentPhoto = await context
                                         .Authors.Where(x => x.Id == id)
                                         .Select(x => x.Photo).FirstAsync();

                var url = await fileStorage.Edit(currentPhoto, container,
                    authorCreationDTOWithPhoto.Photo);
                author.Photo = url;
            }

            context.Update(author);
            await context.SaveChangesAsync();
            await outputCacheStore.EvictByTagAsync(cache, default);
            return NoContent();
        }

        [HttpPatch("{id:int}")]
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


        [HttpDelete("{id:int}")]
        public async Task<ActionResult<Author>> Delete(int id)
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
