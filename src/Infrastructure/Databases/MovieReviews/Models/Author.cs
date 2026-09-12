namespace CleanMinimalApi.Infrastructure.Databases.MovieReviews.Models;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
internal record Author : Entity
{
    public string FirstName { get; init; } = string.Empty;

    public string LastName { get; init; } = string.Empty;

    public ICollection<Review> Reviews { get; init; } = [];
}
