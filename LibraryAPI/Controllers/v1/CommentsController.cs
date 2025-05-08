using AutoMapper;
using Azure;
using LibraryAPI.Data;
using LibraryAPI.DTOs;
using LibraryAPI.Entities;
using LibraryAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;

namespace LibraryAPI.Controllers.v1
{
    [ApiController]
    [Route("api/v1/books/{bookId:int}/comments")]
    [Authorize]
    public class CommentsController: ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IServicesUser servicesUser;
        private readonly IOutputCacheStore outputCacheStore;
        private const string cache = "comentarios-obtener";

        public CommentsController(ApplicationDbContext context, IMapper mapper,
            IServicesUser servicesUser, IOutputCacheStore outputCacheStore)
        {
            this.context = context;
            this.mapper = mapper;
            this.servicesUser = servicesUser;
            this.outputCacheStore = outputCacheStore;
        }

        [HttpGet(Name = "GetCommentsV1")]
        [AllowAnonymous]
        [OutputCache(Tags = [cache])]
        public async Task<ActionResult<List<CommentDTO>>> Get(int bookId)
        {
            var isBookExist = await context.Books.AnyAsync(x => x.Id == bookId);

            if(!isBookExist)
            {
                return NotFound();
            };

            var comments = await context.Comments
                    .Include(x => x.User)
                    .Where(x => x.BookId == bookId)
                    .OrderByDescending(x => x.PublicationDate)
                    .ToListAsync();
            return mapper.Map<List<CommentDTO>>(comments);
        }

        [HttpGet("{id}", Name = "RetrieveCommentV1")]
        [AllowAnonymous]
        [OutputCache(Tags = [cache])]
        public async Task<ActionResult<CommentDTO>> Get(Guid id)
        {
            var comment = await context.Comments
                                    .Include(x => x.User)
                                    .FirstOrDefaultAsync(x => x.Id == id);

            if(comment is null)
            {
                return NotFound();
            }

            return mapper.Map<CommentDTO>(comment);
        }

        [HttpPost(Name = "CreateCommentV1")]
        public async Task<ActionResult> Post(int bookId, CommentCreationDTO commentCreationDto)
        {
            var isBookExist = await context.Books.AnyAsync(x => x.Id == bookId);

            if (!isBookExist)
            {
                return NotFound();
            }

            var user = await servicesUser.GetUser();

            if (user is null)
            {
                return NotFound();
            }

            var comment = mapper.Map<Comment>(commentCreationDto);
            comment.BookId = bookId;
            comment.PublicationDate = DateTime.UtcNow;
            comment.UserId = user.Id;
            context.Add(comment);
            await context.SaveChangesAsync();
            await outputCacheStore.EvictByTagAsync(cache, default);

            var commentDTO = mapper.Map<CommentDTO>(comment);

            return CreatedAtRoute("RetrieveCommentV1", new { id = comment.Id, bookId }, commentDTO);
        }

        [HttpPatch("{id}", Name = "PatchCommentV1")]
        public async Task<ActionResult> Patch(Guid id, int bookId, JsonPatchDocument<CommentPatchDTO> patchDoc)
        {
            if(patchDoc is null)
            {
                return BadRequest();
            }

            var isBookExist = await context.Books.AnyAsync(x => x.Id == bookId);

            if(!isBookExist)
            {
                return NotFound();
            }

            var user = await servicesUser.GetUser();

            if (user is null)
            {
                return NotFound(); // 404 No encontrado
            }

            var commentDB = await context.Comments.FirstOrDefaultAsync(x => x.Id == id);

            if (commentDB is null)
            {
                return NotFound();
            }

            if (commentDB.UserId != user.Id)
            {
                return Forbid(); // 403 Proivido
            }

            var commentPatchDTO = mapper.Map<CommentPatchDTO>(commentDB);

            patchDoc.ApplyTo(commentPatchDTO, ModelState);

            var isValid = TryValidateModel(commentPatchDTO);

            if (!isValid)
            {
                return ValidationProblem();
            }

            mapper.Map(commentPatchDTO, commentDB);

            await context.SaveChangesAsync();
            await outputCacheStore.EvictByTagAsync(cache, default);

            return NoContent(); // 204

        }

        [HttpDelete("{id}", Name = "DeleteCommentV1")]
        public async Task<ActionResult> Delete(Guid id, int bookId)
        {
            var isBookExist = await context.Books.AnyAsync(x => x.Id == bookId);

            if (!isBookExist)
            {
                return NotFound();
            }

            var user = await servicesUser.GetUser();

            if (user is null)
            {
                return NotFound();
            }

            //var deletedRecords = await context.Comments.Where(x => x.Id == id).ExecuteDeleteAsync();

            //if (deletedRecords == 0)
            //{
            //    return NotFound();
            //}

            var commentDB = await context.Comments.FirstOrDefaultAsync(x => x.Id == id);

            if (commentDB is null)
            {
                return NotFound();
            }

            if (commentDB.UserId != user.Id)
            {
                return Forbid();
            }

            /* For audit porpuses we gonna implement Logical Errase */
            //do it the next changes below
            commentDB.IsDeleted = true;
            context.Update(commentDB);

            // instead of physically delete record
            //context.Remove(commentDB);

            await context.SaveChangesAsync();
            await outputCacheStore.EvictByTagAsync(cache, default);

            return NoContent();
        }

    }
}
