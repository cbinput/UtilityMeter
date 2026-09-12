namespace CleanMinimalApi.Infrastructure.Tests.Integration.Databases.MovieReviews;

using System;
using Extensions;
using Infrastructure.Databases.MovieReviews;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using Xunit;

[CollectionDefinition("MovieReviews")]
public class MovieReviewsCollectionFixture : ICollectionFixture<MovieReviewsDataFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}

public class MovieReviewsDataFixture : IDisposable
{
    internal MovieReviewsDbContext Context { get; set; } = null!;
    internal FakeTimeProvider TimeProvider { get; set; } = null!;
    internal EntityFrameworkMovieReviewsRepository Repository { get; set; } = null!;

    public MovieReviewsDataFixture()
    {
        var options = new DbContextOptionsBuilder<MovieReviewsDbContext>()
            .UseInMemoryDatabase($"TestMovies-{Guid.NewGuid()}")
            .Options;

        this.Context = new MovieReviewsDbContext(options);

        this.TimeProvider = new FakeTimeProvider();
        this.TimeProvider.SetUtcNow(new DateTime(2009, 12, 31, 23, 51, 01));

        this.Repository = new EntityFrameworkMovieReviewsRepository(this.Context, this.TimeProvider);

        _ = this.Context.Database.EnsureDeleted();
        _ = this.Context.Database.EnsureCreated();
        _ = this.Context.AddTestData();
    }

    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.Context.Dispose();
            this.Context = null!;
        }
    }
}
