using CircuitPythonBackupService;
using CircuitPythonBackupService.WorkerStrategies;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        //services.AddHostedService<CodePyRapidWorker>();
        // services.AddHostedService<GitCodePyWorker>();
        //services.AddHostedService<LibFolderWorker>();
    })
    .Build();

host.Run();
