using CircuitPythonBackupService;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<CodePyRapidWorker>();
        services.AddHostedService<LibFolderWorker>();
    })
    .Build();

host.Run();
