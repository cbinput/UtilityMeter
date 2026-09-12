namespace CleanMinimalApi.Infrastructure.Databases.MovieReviews.Mapping;

using ApplicationAuthor = Application.Authors.Entities.ReviewAuthor;
using ApplicationMovie = Application.Movies.Entities.ReviewedMovie;
using ApplicationReviews = Application.Reviews.Entities;
using InfrastructureReview = Models.Review;

internal static class ReviewMappingProfile
{
    public static ApplicationReviews.Review ToApplicationReview(InfrastructureReview? review)
    {
        if (review is null)
        {
            return null!;
        }

        return new ApplicationReviews.Review(
            review.Id,
            review.Stars,
            review.ReviewedMovie is null ? null! : new ApplicationMovie(review.ReviewedMovie.Id, review.ReviewedMovie.Title),
            review.ReviewAuthor is null ? null! : new ApplicationAuthor(review.ReviewAuthor.Id, review.ReviewAuthor.FirstName, review.ReviewAuthor.LastName));
    }
}
