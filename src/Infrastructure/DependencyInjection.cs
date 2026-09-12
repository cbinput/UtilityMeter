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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration? configuration = null)
    {
        _ = services.AddDbContext<UtilityMeterDbContext>(options =>
            options.UseInMemoryDatabase($"UtilityMeter-{Guid.NewGuid()}"));

        _ = services.AddScoped<EntityFrameworkReadingsRepository>();
        _ = services.AddScoped<EntityFrameworkEvidenceRepository>();

        _ = services.AddScoped<IReadingsRepository>(x =>
            x.GetRequiredService<EntityFrameworkReadingsRepository>());
        _ = services.AddScoped<EntityFrameworkCondominiumReconciliationRepository>();
        _ = services.AddScoped<ICondominiumReconciliationRepository>(x =>
            x.GetRequiredService<EntityFrameworkCondominiumReconciliationRepository>());
        _ = services.AddScoped<IEvidenceRepository>(x =>
            x.GetRequiredService<EntityFrameworkEvidenceRepository>());

        _ = services.AddSingleton<IBackgroundJobQueue, InMemoryBackgroundJobQueue>();

        var useMinioStorage = configuration?["ObjectStorage:Provider"]
            ?.Equals("Minio", StringComparison.OrdinalIgnoreCase) == true;
        if (useMinioStorage)
        {
            var section = configuration!.GetSection("ObjectStorage:Minio");
            var minioOptions = new MinioStorageOptions
            {
                Endpoint = section["Endpoint"] ?? string.Empty,
                AccessKey = section["AccessKey"] ?? string.Empty,
                SecretKey = section["SecretKey"] ?? string.Empty,
                BucketName = section["BucketName"] ?? "utility-meter-evidence",
                UseSsl = bool.TryParse(section["UseSsl"], out var useSsl) && useSsl
            };
            _ = services.AddSingleton(Microsoft.Extensions.Options.Options.Create(minioOptions));
            _ = services.AddSingleton<IObjectStorage, MinioObjectStorage>();
        }
        else
        {
            _ = services.AddSingleton<IObjectStorage, InMemoryObjectStorage>();
        }

        _ = services.AddSingleton(TimeProvider.System);

        return services;
    }
}
