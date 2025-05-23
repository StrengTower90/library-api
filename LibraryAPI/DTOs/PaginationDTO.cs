﻿ namespace LibraryAPI.DTOs
{
    // when you establish record directive indicate that the class won't be inmuted once instanced
    public record PaginationDTO(int Page = 1, int RecordsPerPage = 10)
    {
        private const int MaxAmountRecordsPerPage = 50;

        public int Page { get; init; } = Math.Max(1, Page);
        public int RecordsPerPage { get; init; } = 
                Math.Clamp(RecordsPerPage, 1, MaxAmountRecordsPerPage);
    }
}
