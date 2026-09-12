namespace CleanMinimalApi.Infrastructure.Databases.MovieReviews.Models;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
internal record Movie : Entity
{
    public string Title { get; init; } = string.Empty;

    public ICollection<Review> Reviews { get; init; } = [];
}
