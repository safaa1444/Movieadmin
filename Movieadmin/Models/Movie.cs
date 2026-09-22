
namespace Movieadmin.Models
{
    public class Movie
    {
        public int Id { get; set; }

        public string Title { get; set; } = "";

        public string Synopsis { get; set; } = "";

        public string Genre { get; set; } = ""; 

        public string Rating { get; set; } = "";

        public int RuntimeHours { get; set; }

        public int RuntimeMinutes { get; set; }

        public DateTime ReleaseDate { get; set; }
    }
}