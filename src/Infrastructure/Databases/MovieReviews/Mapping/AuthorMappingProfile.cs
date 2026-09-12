namespace CleanMinimalApi.Infrastructure.Databases.MovieReviews.Mapping;

using ApplicationAuthors = Application.Authors.Entities;
using InfrastructureAuthor = Models.Author;

internal static class AuthorMappingProfile
{
    public static ApplicationAuthors.Author ToApplicationAuthor(InfrastructureAuthor? author)
    {
        if (author is null)
        {
            return null!;
        }

        return new ApplicationAuthors.Author(author.Id, author.FirstName, author.LastName);
    }
}
