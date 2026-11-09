
using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Movieadmin.Models;
using Movieadmin.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<BlobStorageService>();

var connectionString = builder.Configuration.GetConnectionString("MovieContext")
    ?? throw new InvalidOperationException(
        "Connection string 'MovieContext' not found.");

builder.Services.AddDbContext<MovieContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddControllersWithViews();

builder.Services.AddAuth0WebAppAuthentication(options =>
{
    options.Domain = builder.Configuration["Auth0:Domain"]!;
    options.ClientId = builder.Configuration["Auth0:ClientId"]!;
    options.ClientSecret = builder.Configuration["Auth0:ClientSecret"]!;
})
.WithAccessToken(options =>
{
    options.Audience = builder.Configuration["Auth0:Audience"];
});

// Configure Auth0 Roles
builder.Services.Configure<OpenIdConnectOptions>("Auth0", options =>
{
    options.TokenValidationParameters.RoleClaimType =
        "https://movieadmin.com/roles";
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Seed Genres and Audience Ratings
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider
        .GetRequiredService<MovieContext>();

    if (!context.Genres.Any())
    {
        context.Genres.AddRange(
            new Genre { Title = "Action" },
            new Genre { Title = "Comedy" },
            new Genre { Title = "Drama" },
            new Genre { Title = "Horror" },
            new Genre { Title = "Romance" },
            new Genre { Title = "Science Fiction" }
        );
    }

    if (!context.AudienceRatings.Any())
    {
        context.AudienceRatings.AddRange(
            new AudienceRating { Title = "G" },
            new AudienceRating { Title = "PG" },
            new AudienceRating { Title = "PG-13" },
            new AudienceRating { Title = "R" },
            new AudienceRating { Title = "NC-17" }
        );
    }

    context.SaveChanges();
}

app.Run();
