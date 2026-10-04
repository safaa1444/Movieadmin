using System.ComponentModel.DataAnnotations;

namespace Movieadmin.Models
{
    public class Movie
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = "";

        [Required]
        [StringLength(1000)]
        public string Synopsis { get; set; } = "";

        [Required]
        [StringLength(50)]
        public string Genre { get; set; } = "";

        [Required]
        [StringLength(10)]
        public string Rating { get; set; } = "";

        [Range(0, 23)]
        public int RuntimeHours { get; set; }

        [Range(0, 59)]
        public int RuntimeMinutes { get; set; }

        [Display(Name = "Release Date")]
        [DataType(DataType.Date)]
        public DateTime ReleaseDate { get; set; }
    }
}