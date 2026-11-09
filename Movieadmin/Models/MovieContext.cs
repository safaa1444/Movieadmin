
using Microsoft.EntityFrameworkCore;

namespace Movieadmin.Models
{
    public class MovieContext : DbContext
    {
        public MovieContext(DbContextOptions<MovieContext> options)
            : base(options)
        {
        }

        public DbSet<Movie> Movies { get; set; }

        public DbSet<Genre> Genres { get; set; }

        public DbSet<AudienceRating> AudienceRatings { get; set; }

        public DbSet<MovieReview> MovieReviews { get; set; }
    }
}

