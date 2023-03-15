using CircuitPythonBackupService;
using CircuitPythonBackupService.WorkerStrategies;
using Serilog.Events;
using Serilog;
using CommandLine;
using Microsoft.Extensions.Options;
using CircuitPythonBackupService.CommandLineParser;
using CircuitPythonBackupService.Services;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting host");
    CreateHostBuilder(args).Build().Run();
    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}
static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureServices(services =>
        {
            services.AddSingleton<HardwareInfoSingleton>();
            services.AddSingleton<CircuitPythonUSBDeviceScanner>();

            Parser.Default.ParseArguments<AllOptions>(args)
               .WithParsed(parsedAllOptions =>
               {
                   services.AddSingleton(parsedAllOptions);

                   if (parsedAllOptions.UseLibIntervalWorker)
                   {
                       services.AddHostedService<LibFolderWorker>();
                   }

                   if (parsedAllOptions.UseCodePyGitWorker)
                   {
                       services.AddHostedService<GitWorker>();
                   }
               });
        })
        .UseSerilog((context, services, loggerConfiguration) => loggerConfiguration
            .ReadFrom.Configuration(context.Configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console());
