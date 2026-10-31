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

        public int GenreId { get; set; }

        public Genre Genre { get; set; } = null!;

        public int AudienceRatingId { get; set; }

        public AudienceRating AudienceRating { get; set; } = null!;

        [Range(0, 23)]
        public int RuntimeHours { get; set; }

        [Range(0, 59)]
        public int RuntimeMinutes { get; set; }

        [Display(Name = "Release Date")]
        [DataType(DataType.Date)]
        public DateTime ReleaseDate { get; set; }

        public string? ImageUrl { get; set; }
    }
}