namespace LibraryAPI.Entities
{
    public class Error
    {
        public Guid Id { get; set; }
        public required string MessageError { get; set; }
        public string? StrackTrace { get; set; }
        public DateTime Date { get; set; }
    }
}
