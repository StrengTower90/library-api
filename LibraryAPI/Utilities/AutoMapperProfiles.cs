using AutoMapper;
using LibraryAPI.DTOs;
using LibraryAPI.Entities;
using System.Globalization;

namespace LibraryAPI.Utilities
{
    public class AutoMapperProfiles: Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<Author, AuthorDTO>()
                .ForMember(dto => dto.FullName,
                    config => config.MapFrom(author => MappAuthorNamesAndLasNames(author)));

            CreateMap<Author, AuthorWithBooksDTO>()
                .ForMember(dto => dto.FullName,
                    config => config.MapFrom(author => MappAuthorNamesAndLasNames(author)));

            CreateMap<AuthorCreationDTO, Author>();
            CreateMap<AuthorCreationDTOWithPhoto, Author>()
                    .ForMember(ent => ent.Photo, config => config.Ignore());

            /* ReverseMap indicate that the Mapp could be from rigth to left and left to rigth as needed */
            CreateMap<Author, AuthorPatchDTO>().ReverseMap();

            CreateMap<AuthorBook, BookDTO>()
                .ForMember(dto => dto.Id, config => config.MapFrom(ent => ent.BookId))
                .ForMember(dto => dto.Title, config => config.MapFrom(ent => ent.Book!.Title));

            CreateMap<Book, BookDTO>();

            CreateMap<BookCreationDTO, Book>()
                .ForMember(ent => ent.Authors, config => 
                    config.MapFrom(dto => dto.AuthorIds.Select(id => new AuthorBook { AuthorId = id })));

            CreateMap<Book, BookWithAuthorDTO>();

            CreateMap<AuthorBook, AuthorDTO>()
                   .ForMember(dto => dto.Id, config => config.MapFrom(ent => ent.AuthorId))
                   .ForMember(dto => dto.FullName, 
                    config => config.MapFrom(ent => MappAuthorNamesAndLasNames(ent.Author!)));

            CreateMap<BookCreationDTO, AuthorBook>()
                .ForMember(ent => ent.Book,
                    config => config.MapFrom(dto => new Book { Title = dto.Title }));

            //CreateMap<Book, BookWithAuthorDTO>()
            //    .ForMember(dto => dto.AuthorName, config =>
            //        config.MapFrom(ent => MappAuthorNamesAndLasNames(ent.Author!)));

            CreateMap<CommentCreationDTO, Comment>();
            CreateMap<Comment, CommentDTO>()
                 .ForMember(dto => dto.UserEmail, config => config.MapFrom(ent => ent.User!.Email));               
            CreateMap<CommentPatchDTO, Comment>().ReverseMap();

            CreateMap<User, UserDTO>();
        }

        private string MappAuthorNamesAndLasNames(Author author) => $"{author.Names} {author.LastNames}";
    }
}
