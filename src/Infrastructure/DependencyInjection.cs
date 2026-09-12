namespace CleanMinimalApi.Infrastructure;

using System;
using Application.BackgroundJobs;
using Application.Evidence;
using Application.Readings;
using Application.Reports;
using Application.Storage;
using Databases.UtilityMeter;
using Infrastructure.BackgroundJobs;
using Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        _ = services.AddDbContext<UtilityMeterDbContext>(options =>
            options.UseInMemoryDatabase($"UtilityMeter-{Guid.NewGuid()}"), ServiceLifetime.Singleton);

        _ = services.AddSingleton<EntityFrameworkReadingsRepository>();
        _ = services.AddSingleton<EntityFrameworkEvidenceRepository>();

        _ = services.AddSingleton<IReadingsRepository>(x =>
            x.GetRequiredService<EntityFrameworkReadingsRepository>());
        _ = services.AddSingleton<EntityFrameworkCondominiumReconciliationRepository>();
        _ = services.AddSingleton<ICondominiumReconciliationRepository>(x =>
            x.GetRequiredService<EntityFrameworkCondominiumReconciliationRepository>());
        _ = services.AddSingleton<IEvidenceRepository>(x =>
            x.GetRequiredService<EntityFrameworkEvidenceRepository>());

        _ = services.AddSingleton<IBackgroundJobQueue, InMemoryBackgroundJobQueue>();
        _ = services.AddSingleton<IObjectStorage, InMemoryObjectStorage>();
        _ = services.AddSingleton(TimeProvider.System);

        return services;
    }
}
