
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Movieadmin.Models;

namespace Movieadmin.Controllers
{
    [Authorize(Roles = "critic")]
    public class ReviewsController : Controller
    {
        private readonly MovieContext _context;

        public ReviewsController(MovieContext context)
        {
            _context = context;
        }

        // Create Review - GET
        public async Task<IActionResult> Create(int movieId)
        {
            var movie = await _context.Movies.FindAsync(movieId);

            if (movie == null)
                return NotFound();

            ViewBag.MovieTitle = movie.Title;
            ViewBag.MovieId = movieId;

            return View();
        }

        // Create Review - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            int movieId,
            [Bind("Rating,ReviewText")] MovieReview review)
        {
            var movie = await _context.Movies.FindAsync(movieId);

            if (movie == null)
                return NotFound();

            string? reviewerId = GetReviewerId();

            if (string.IsNullOrEmpty(reviewerId))
                return Challenge();

            bool alreadyReviewed = await _context.MovieReviews
                .AnyAsync(r =>
                    r.MovieId == movieId &&
                    r.ReviewerId == reviewerId);

            if (alreadyReviewed)
                return BadRequest("You have already reviewed this movie.");

            if (!ModelState.IsValid)
            {
                ViewBag.MovieTitle = movie.Title;
                ViewBag.MovieId = movieId;
                return View(review);
            }

            review.MovieId = movieId;
            review.ReviewerId = reviewerId;
            review.IsPublished = false;
            review.CreatedAt = DateTime.UtcNow;

            _context.MovieReviews.Add(review);
            await _context.SaveChangesAsync();

            return RedirectToAction(
                "Details", "Movies", new { id = movieId });
        }

        // Edit Review - GET
        public async Task<IActionResult> Edit(int id)
        {
            var review = await _context.MovieReviews
                .Include(r => r.Movie)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (review == null)
                return NotFound();

            if (review.ReviewerId != GetReviewerId())
                return Forbid();

            if (review.IsPublished)
                return BadRequest("Published reviews cannot be edited.");

            ViewBag.MovieTitle = review.Movie.Title;

            return View(review);
        }

        // Edit Review - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,Rating,ReviewText")] MovieReview updatedReview)
        {
            var review = await _context.MovieReviews
                .FirstOrDefaultAsync(r => r.Id == id);

            if (review == null)
                return NotFound();

            if (review.ReviewerId != GetReviewerId())
                return Forbid();

            if (review.IsPublished)
                return BadRequest("Published reviews cannot be edited.");

            if (!ModelState.IsValid)
            {
                var movie = await _context.Movies.FindAsync(review.MovieId);
                ViewBag.MovieTitle = movie?.Title;
                return View(updatedReview);
            }

            review.Rating = updatedReview.Rating;
            review.ReviewText = updatedReview.ReviewText;

            await _context.SaveChangesAsync();

            return RedirectToAction(
                "Details", "Movies", new { id = review.MovieId });
        }

        // Delete Review
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var review = await _context.MovieReviews
                .FirstOrDefaultAsync(r => r.Id == id);

            if (review == null)
                return NotFound();

            if (review.ReviewerId != GetReviewerId())
                return Forbid();

            if (review.IsPublished)
                return BadRequest("Published reviews cannot be deleted.");

            int movieId = review.MovieId;

            _context.MovieReviews.Remove(review);
            await _context.SaveChangesAsync();

            return RedirectToAction(
                "Details", "Movies", new { id = movieId });
        }

        // Publish Review
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Publish(int id)
        {
            var review = await _context.MovieReviews
                .FirstOrDefaultAsync(r => r.Id == id);

            if (review == null)
                return NotFound();

            if (review.ReviewerId != GetReviewerId())
                return Forbid();

            if (review.IsPublished)
                return BadRequest("This review is already published.");

            review.IsPublished = true;

            await _context.SaveChangesAsync();

            return RedirectToAction(
                "Details", "Movies", new { id = review.MovieId });
        }

        // My Reviews
        public async Task<IActionResult> MyReviews()
        {
            string? reviewerId = GetReviewerId();

            if (string.IsNullOrEmpty(reviewerId))
                return Challenge();

            var reviews = await _context.MovieReviews
                .Include(r => r.Movie)
                .Where(r => r.ReviewerId == reviewerId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(reviews);
        }

        // Get Current Reviewer ID
        private string? GetReviewerId()
        {
            return User.FindFirst("sub")?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
