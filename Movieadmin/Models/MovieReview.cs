
using System.ComponentModel.DataAnnotations;

namespace Movieadmin.Models
{
    public class MovieReview
    {
        public int Id { get; set; }

        public int MovieId { get; set; }

        public Movie Movie { get; set; } = null!;

        [Required]
        public string ReviewerId { get; set; } = "";

        [Range(0, 5)]
        public int Rating { get; set; }

        [Required]
        [StringLength(1000)]
        public string ReviewText { get; set; } = "";

        public bool IsPublished { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
