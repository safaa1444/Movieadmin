using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Movieadmin.Models;
using Movieadmin.Services;

namespace Movieadmin.Controllers
{
    public class MoviesController : Controller
    {
        private readonly MovieContext _context;
        private readonly BlobStorageService _blobStorageService;

        public MoviesController(
            MovieContext context,
            BlobStorageService blobStorageService)
        {
            _context = context;
            _blobStorageService = blobStorageService;
        }

        public async Task<IActionResult> Index()
        {
            var movies = await _context.Movies
                .Include(m => m.Genre)
                .Include(m => m.AudienceRating)
                .OrderByDescending(m => m.ReleaseDate)
                .ToListAsync();

            return View(movies);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var movie = await _context.Movies
                .Include(m => m.Genre)
                .Include(m => m.AudienceRating)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null)
                return NotFound();

            return View(movie);
        }

        public async Task<IActionResult> Image(int id)
        {
            var movie = await _context.Movies.FindAsync(id);

            if (movie == null ||
                string.IsNullOrWhiteSpace(movie.ImageUrl))
            {
                return NotFound();
            }

            try
            {
                var image = await _blobStorageService.DownloadImageAsync(
                    movie.ImageUrl);

                return File(
                    image.Content.ToArray(),
                    image.Details.ContentType);
            }
            catch (Azure.RequestFailedException)
            {
                return NotFound();
            }
        }

        public IActionResult Create()
        {
            PopulateDropdowns();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Title,Synopsis,GenreId,AudienceRatingId,RuntimeHours,RuntimeMinutes,ReleaseDate")]
            Movie movie,
            IFormFile? imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                ModelState.AddModelError(
                    "imageFile", "Please select an image.");
            }
            else if (!IsImage(imageFile))
            {
                ModelState.AddModelError(
                    "imageFile",
                    "Please select a JPG, PNG, GIF, or WebP image.");
            }

            if (!ModelState.IsValid)
            {
                PopulateDropdowns(movie.GenreId, movie.AudienceRatingId);
                return View(movie);
            }

            string extension =
                Path.GetExtension(imageFile!.FileName).ToLowerInvariant();

            string fileName = $"{Guid.NewGuid()}{extension}";

            using var stream = imageFile.OpenReadStream();

            movie.ImageUrl = await _blobStorageService.UploadImageAsync(
                stream,
                fileName,
                imageFile.ContentType);

            _context.Movies.Add(movie);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var movie = await _context.Movies.FindAsync(id);

            if (movie == null)
                return NotFound();

            PopulateDropdowns(movie.GenreId, movie.AudienceRatingId);

            return View(movie);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int? id,
            [Bind("Id,Title,Synopsis,GenreId,AudienceRatingId,RuntimeHours,RuntimeMinutes,ReleaseDate")]
            Movie movie,
            IFormFile? imageFile)
        {
            if (id != movie.Id)
                return NotFound();

            var existingMovie = await _context.Movies.FindAsync(id);

            if (existingMovie == null)
                return NotFound();

            if (imageFile != null &&
                imageFile.Length > 0 &&
                !IsImage(imageFile))
            {
                ModelState.AddModelError(
                    "imageFile",
                    "Please select a JPG, PNG, GIF, or WebP image.");
            }

            if (!ModelState.IsValid)
            {
                movie.ImageUrl = existingMovie.ImageUrl;

                PopulateDropdowns(movie.GenreId, movie.AudienceRatingId);

                return View(movie);
            }

            existingMovie.Title = movie.Title;
            existingMovie.Synopsis = movie.Synopsis;
            existingMovie.GenreId = movie.GenreId;
            existingMovie.AudienceRatingId = movie.AudienceRatingId;
            existingMovie.RuntimeHours = movie.RuntimeHours;
            existingMovie.RuntimeMinutes = movie.RuntimeMinutes;
            existingMovie.ReleaseDate = movie.ReleaseDate;

            if (imageFile != null && imageFile.Length > 0)
            {
                string extension =
                    Path.GetExtension(imageFile.FileName).ToLowerInvariant();

                string fileName = $"{Guid.NewGuid()}{extension}";

                using var stream = imageFile.OpenReadStream();

                existingMovie.ImageUrl =
                    await _blobStorageService.UploadImageAsync(
                        stream,
                        fileName,
                        imageFile.ContentType);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var movie = await _context.Movies
                .Include(m => m.Genre)
                .Include(m => m.AudienceRating)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null)
                return NotFound();

            return View(movie);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            var movie = await _context.Movies.FindAsync(id);

            if (movie != null)
            {
                _context.Movies.Remove(movie);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private void PopulateDropdowns(
            int? selectedGenreId = null,
            int? selectedAudienceRatingId = null)
        {
            ViewBag.GenreId = new SelectList(
                _context.Genres.OrderBy(g => g.Title),
                "Id",
                "Title",
                selectedGenreId);

            ViewBag.AudienceRatingId = new SelectList(
                _context.AudienceRatings.OrderBy(a => a.Title),
                "Id",
                "Title",
                selectedAudienceRatingId);
        }

        private static bool IsImage(IFormFile file)
        {
            string extension =
                Path.GetExtension(file.FileName).ToLowerInvariant();

            string[] allowedExtensions =
            {
                ".jpg", ".jpeg", ".png", ".gif", ".webp"
            };

            string[] allowedContentTypes =
            {
                "image/jpeg", "image/png", "image/gif", "image/webp"
            };

            return allowedExtensions.Contains(extension)
                && allowedContentTypes.Contains(
                    file.ContentType.ToLowerInvariant());
        }
    }
}