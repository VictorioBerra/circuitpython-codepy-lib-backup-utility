namespace CircuitPythonBackupService.WorkerStrategies;

using CircuitPythonBackupService.CommandLineParser;
using CircuitPythonBackupService.Services;
using Hardware.Info;
using System.IO;
using System.Net.Http.Headers;

public class LibFolderWorker : BackgroundService
{
    private readonly ILogger<LibFolderWorker> logger;
    private readonly CircuitPythonUSBDeviceScanner circuitPythonUSBDeviceScanner;
    private readonly AllOptions allOptions;

    public LibFolderWorker(
        ILogger<LibFolderWorker> logger,
        CircuitPythonUSBDeviceScanner circuitPythonUSBDeviceScanner,
        AllOptions allOptions)
    {
        this.logger = logger;
        this.circuitPythonUSBDeviceScanner = circuitPythonUSBDeviceScanner;
        this.allOptions = allOptions;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            this.logger.LogInformation("LibFolderWorker running at: {Time}", DateTimeOffset.Now);

            var drives = circuitPythonUSBDeviceScanner.FindCircuitPythonDrives();

            if (!drives.Any())
            {
                this.logger.LogInformation("No CircuitPython drives found, done with backup until next interval.");
            }
            else
            {
                foreach (var drive in drives)
                {
                    PerformLibArchival(drive.Name, drive.SerialNumber);
                }
            }

            await Task.Delay(1000 * this.allOptions.LibWorkerIntervalSeconds, stoppingToken);
        }
    }

    private void PerformLibArchival(string driveLetter, string serialNumber)
    {
        this.logger.LogInformation("Performing archival of lib folder for drive {DriveLetter} with serial number {SerialNumber}", driveLetter, serialNumber);

        var libFolder = Path.Join(driveLetter, "lib");
        var archivePath = Path.Join(this.allOptions.LibWorkerArchivePath, serialNumber, "lib");
        var destinationZip = Path.Join(archivePath, $"lib-{DateTime.Now.ToString("yyyyMMddHHmmss")}.zip");

        if (!Directory.Exists(this.allOptions.LibWorkerArchivePath))
        {
            if (this.allOptions.LibWorkerCreateDirectory)
            {
                Directory.CreateDirectory(this.allOptions.LibWorkerArchivePath);
            }
            else
            {
                this.logger.LogError("Archive path {ArchivePath} does not exist and LibWorkerCreateDirectory is false, skipping archival.", archivePath);
                throw new InvalidOperationException("Lib worker archive folder did not exist, and I was told not to create it.");
            }
        }

        Directory.CreateDirectory(archivePath);

        // Zip up folder
        var fastZip = new ICSharpCode.SharpZipLib.Zip.FastZip();
        fastZip.CreateZip(
            destinationZip,
            libFolder,
            true,
            null);

        this.logger.LogInformation("Archival of lib folder for drive {DriveLetter} with serial number {SerialNumber} complete.", driveLetter, serialNumber);
        this.logger.LogInformation(
            "Lib folder {LibFolder} archived to {DestinationZip}",
            libFolder,
            destinationZip);
    }
}