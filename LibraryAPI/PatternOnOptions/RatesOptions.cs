using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.PatternOnOptions
{
    public class RatesOptions
    {
        public const string Section = "rates";

        [Required]
        public decimal Day { get; set; }

        [Required]
        public decimal Night { get; set; }
    }
}
