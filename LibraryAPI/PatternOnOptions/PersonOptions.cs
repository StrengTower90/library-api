using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.PatternOnOptions
{
    public class PersonOptions
    {
        public const string Section = "section_01";

        [Required]
        public required string Name { get; set; }
        
        [Required]
        public int Age { get; set; }
    }
}
