namespace CleanMinimalApi.Infrastructure.Databases.MovieReviews;

using System;
using Application.Authors;
using Application.Common.Enums;
using Application.Common.Exceptions;
using Application.Movies;
using Application.Reviews;
using Extensions;
using Microsoft.EntityFrameworkCore;
using Models;
using ApplicationAuthor = Application.Authors.Entities.Author;
using ApplicationMovie = Application.Movies.Entities.Movie;
using ApplicationReview = Application.Reviews.Entities.Review;
using ApplicationReviewAuthor = Application.Authors.Entities.ReviewAuthor;
using ApplicationReviewedMovie = Application.Movies.Entities.ReviewedMovie;
using InfrastructureAuthor = Models.Author;
using InfrastructureMovie = Models.Movie;
using InfrastructureReview = Models.Review;

internal class EntityFrameworkMovieReviewsRepository : IAuthorsRepository, IMoviesRepository, IReviewsRepository
{
    private readonly MovieReviewsDbContext context;
    private readonly TimeProvider timeProvider;

    public EntityFrameworkMovieReviewsRepository(
        MovieReviewsDbContext context,
        TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(timeProvider);

        this.context = context;
        this.timeProvider = timeProvider;

        _ = this.context.Database.EnsureDeleted();
        _ = this.context.Database.EnsureCreated();
        _ = this.context.AddData();
    }

    #region Authors

    public virtual async Task<List<ApplicationAuthor>> GetAuthors(CancellationToken cancellationToken)
    {
        var authors = await this.context.Authors
            .Include(a => a.Reviews)
            .ThenInclude(r => r.ReviewedMovie)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return authors.Select(MapAuthor).ToList();
    }

    public virtual async Task<ApplicationAuthor> GetAuthorById(Guid id, CancellationToken cancellationToken)
    {
        var author = await this.context.Authors
            .Where(r => r.Id == id)
            .Include(a => a.Reviews)
            .ThenInclude(r => r.ReviewedMovie)
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        return author is null ? null! : MapAuthor(author);
    }

    public virtual async Task<bool> AuthorExists(Guid id, CancellationToken cancellationToken)
    {
        return await this.context.Authors.AsNoTracking().AnyAsync(a => a.Id == id, cancellationToken);
    }

    #endregion Authors

    #region Movies

    public virtual async Task<List<ApplicationMovie>> GetMovies(CancellationToken cancellationToken)
    {
        var result = await this.context.Movies
            .Include(m => m.Reviews)
            .ThenInclude(r => r.ReviewAuthor)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return result.Select(MapMovie).ToList();
    }

    public virtual async Task<ApplicationMovie> GetMovieById(Guid id, CancellationToken cancellationToken)
    {
        var result = await this.context.Movies
            .Where(r => r.Id == id)
            .Include(m => m.Reviews)
            .ThenInclude(r => r.ReviewAuthor)
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        return result is null ? null! : MapMovie(result);
    }

    public virtual async Task<bool> MovieExists(Guid id, CancellationToken cancellationToken)
    {
        return await this.context.Movies.AsNoTracking().AnyAsync(m => m.Id == id, cancellationToken);
    }

    #endregion Movies

    #region Reviews

    public async Task<ApplicationReview> CreateReview(
        Guid authorId,
        Guid movieId,
        int stars,
        CancellationToken cancellationToken)
    {
        var review = new InfrastructureReview
        {
            ReviewAuthorId = authorId,
            ReviewedMovieId = movieId,
            Stars = stars,
            DateCreated = this.timeProvider.GetUtcNow().UtcDateTime,
            DateModified = this.timeProvider.GetUtcNow().UtcDateTime
        };

        var id = this.context.Add(review).Entity.Id;

        _ = await this.context.SaveChangesAsync(cancellationToken);

        var result = await this.context.Reviews
            .Where(r => r.Id == id)
            .Include(r => r.ReviewAuthor)
            .Include(r => r.ReviewedMovie)
            .AsNoTracking()
            .FirstAsync(cancellationToken);

        return MapReview(result);
    }

    public async Task<bool> DeleteReview(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            _ = this.context.Remove(this.context.Reviews.Single(r => r.Id == id));
            _ = await this.context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }

    public async Task<List<ApplicationReview>> GetReviews(CancellationToken cancellationToken)
    {
        var result = await this.context.Reviews
            .Include(r => r.ReviewAuthor)
            .Include(r => r.ReviewedMovie)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return result.Select(MapReview).ToList();
    }

    public async Task<ApplicationReview> GetReviewById(Guid id, CancellationToken cancellationToken)
    {
        var result = await this.context.Reviews
            .Where(r => r.Id == id)
            .Include(r => r.ReviewAuthor)
            .Include(r => r.ReviewedMovie)
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        return result is null ? null! : MapReview(result);
    }

    public async Task<bool> ReviewExists(Guid id, CancellationToken cancellationToken)
    {
        return await this.context.Reviews.AnyAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<bool> UpdateReview(
        Guid id,
        Guid authorId,
        Guid movieId,
        int stars,
        CancellationToken cancellationToken)
    {
        try
        {
            var review = this.context.Reviews.FirstOrDefault(r => r.Id == id);

            if (review is null)
            {
                return false;
            }

            review.Stars = stars;
            review.ReviewAuthorId = authorId;
            review.ReviewedMovieId = movieId;
            review.DateModified = this.timeProvider.GetUtcNow().UtcDateTime;

            _ = this.context.Update(review);
            _ = await this.context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }

    #endregion Reviews

    private static ApplicationAuthor MapAuthor(InfrastructureAuthor? author)
    {
        if (author is null)
        {
            return null!;
        }

        return new ApplicationAuthor(
            author.Id,
            author.FirstName,
            author.LastName,
            author.Reviews?.Select(MapReview).ToList() ?? []);
    }

    private static ApplicationMovie MapMovie(InfrastructureMovie? movie)
    {
        if (movie is null)
        {
            return null!;
        }

        return new ApplicationMovie(
            movie.Id,
            movie.Title,
            movie.Reviews?.Select(MapReview).ToList() ?? []);
    }

    private static ApplicationReview MapReview(InfrastructureReview? review)
    {
        if (review is null)
        {
            return null!;
        }

        return new ApplicationReview(
            review.Id,
            review.Stars,
            MapReviewedMovie(review.ReviewedMovie),
            MapReviewAuthor(review.ReviewAuthor));
    }

    private static ApplicationReviewedMovie MapReviewedMovie(InfrastructureMovie? movie)
    {
        if (movie is null)
        {
            return null!;
        }

        return new ApplicationReviewedMovie(movie.Id, movie.Title);
    }

    private static ApplicationReviewAuthor MapReviewAuthor(InfrastructureAuthor? author)
    {
        if (author is null)
        {
            return null!;
        }

        return new ApplicationReviewAuthor(author.Id, author.FirstName, author.LastName);
    }
}
