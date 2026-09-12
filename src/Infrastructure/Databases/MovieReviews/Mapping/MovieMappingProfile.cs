namespace CleanMinimalApi.Infrastructure.Databases.MovieReviews.Mapping;

using ApplicationMovies = Application.Movies.Entities;
using InfrastructureMovie = Models.Movie;

internal static class MovieMappingProfile
{
    public static ApplicationMovies.Movie ToApplicationMovie(InfrastructureMovie? movie)
    {
        if (movie is null)
        {
            return null!;
        }

        return new ApplicationMovies.Movie(movie.Id, movie.Title);
    }
}
