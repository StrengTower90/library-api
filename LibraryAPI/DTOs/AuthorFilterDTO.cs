namespace LibraryAPI.DTOs
{
    public class AuthorFilterDTO
    {
        public int Page { get; set; } = 1;
        public int RecordsPerPage { get; set; } = 10;
        public PaginationDTO PaginationDTO 
        {
            get
            {
                return new PaginationDTO(Page, RecordsPerPage);
            } 
        }

        public string? Names { get; set; }
        public string? LastNames { get; set; }
        public bool? IsHasPhoto { get; set; }
        public bool? IsHasBooks { get; set; }
        public string? BookTitle { get; set; }
        public bool IncludeBooks { get; set; }
        public string? OrderField { get; set; }
        public bool IsAscendingOrder { get; set; }


    }
}
