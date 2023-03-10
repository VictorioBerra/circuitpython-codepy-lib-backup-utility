namespace CircuitPythonBackupService;

using System.IO;

public class LibFolderWorker : BackgroundService
{
    private readonly ILogger<LibFolderWorker> _logger;
    private readonly string archivePath;

    public LibFolderWorker(ILogger<LibFolderWorker> logger)
    {
        _logger = logger;

        archivePath = @"D:\Source\ESP32-S3-Archive";
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("LibFolderWorker running at: {Time}", DateTimeOffset.Now);

            foreach (var drive in DriveInfo.GetDrives())
            {
                if (drive.VolumeLabel.Equals("CIRCUITPY", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation("CIRCUITPY drive found at {Drive}", drive.Name);

                    PerformLibArchival(drive.Name);
                }
            }

            await Task.Delay(1000 * 60 * 5, stoppingToken);
        }
    }

    private void PerformLibArchival(string driveName)
    {
        _logger.LogInformation("Performing archival of lib folder");

        var libFolder = Path.Join(driveName, "lib");

        // Zip up folder
        var fastZip = new ICSharpCode.SharpZipLib.Zip.FastZip();
        fastZip.CreateZip(
            Path.Join(archivePath, $"lib-{DateTime.Now.ToString("yyyyMMddHHmmss")}.zip"),
            libFolder,
            true,
            null);
    }
}