using CleanMinimalApi.Application;
using CleanMinimalApi.Infrastructure;
using CleanMinimalApi.Worker;

var host = Host.CreateApplicationBuilder(args);

host.Services.AddInfrastructure(host.Configuration);
host.Services.AddApplication();
host.Services.AddHostedService<QueuedBackgroundWorker>();

await host.Build().RunAsync();
