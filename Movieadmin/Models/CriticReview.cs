
namespace Movieadmin.Models
{
    public class CriticReview
    {
        public int Id { get; set; }

        public string Description { get; set; } = string.Empty;

        public int Rating { get; set; }

        public bool IsPublished { get; set; }

        public string CreatedBy { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; }
    }
}
