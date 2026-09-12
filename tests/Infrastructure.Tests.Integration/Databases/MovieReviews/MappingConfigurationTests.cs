namespace CleanMinimalApi.Infrastructure.Tests.Integration.Databases.MovieReviews;

using Xunit;

[Collection("MovieReviews")]
public class MappingConfigurationTests(MovieReviewsDataFixture fixture)
{
    [Fact]
    public async Task ShouldReturnMappedAuthorMovieAndReviewValues()
    {
        var authors = await fixture.Repository.GetAuthors(CancellationToken.None);
        var movies = await fixture.Repository.GetMovies(CancellationToken.None);
        var reviews = await fixture.Repository.GetReviews(CancellationToken.None);

        Assert.NotEmpty(authors);
        Assert.NotEmpty(movies);
        Assert.NotEmpty(reviews);

        Assert.NotNull(authors[0].Reviews);
        Assert.NotNull(movies[0].Reviews);
        Assert.NotNull(reviews[0].ReviewAuthor);
        Assert.NotNull(reviews[0].ReviewedMovie);
    }
}
