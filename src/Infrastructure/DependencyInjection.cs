namespace CleanMinimalApi.Infrastructure;

using System;
using Application.Authors;
using Application.BackgroundJobs;
using Application.Movies;
using Application.Reviews;
using Application.Storage;
using Databases.MovieReviews;
using Infrastructure.BackgroundJobs;
using Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        _ = services.AddDbContext<MovieReviewsDbContext>(options =>
            options.UseInMemoryDatabase($"Movies-{Guid.NewGuid()}"), ServiceLifetime.Singleton);

        _ = services.AddSingleton<EntityFrameworkMovieReviewsRepository>();

        _ = services.AddSingleton<IAuthorsRepository>(p =>
            p.GetRequiredService<EntityFrameworkMovieReviewsRepository>());
        _ = services.AddSingleton<IMoviesRepository>(x =>
            x.GetRequiredService<EntityFrameworkMovieReviewsRepository>());
        _ = services.AddSingleton<IReviewsRepository>(x =>
            x.GetRequiredService<EntityFrameworkMovieReviewsRepository>());

        _ = services.AddSingleton<IBackgroundJobQueue, InMemoryBackgroundJobQueue>();
        _ = services.AddSingleton<IObjectStorage, InMemoryObjectStorage>();
        _ = services.AddSingleton(TimeProvider.System);

        return services;
    }
}
