namespace CircuitPythonBackupService.WorkerStrategies;

using CircuitPythonBackupService.Models;
using JsonFlatFileDataStore;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Security.Cryptography;

public class CodePyRapidWorker : BackgroundService
{
    private readonly ILogger<CodePyRapidWorker> _logger;
    private readonly DataStore store;
    private readonly string archivePath;

    public CodePyRapidWorker(ILogger<CodePyRapidWorker> logger)
    {
        _logger = logger;

        archivePath = @"D:\Source\ESP32-S3-Archive";

        // Open database (create new if file doesn't exist)
        store = new DataStore(Path.Join(archivePath, "data.json"));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("CodePyRapidWorker running at: {Time}", DateTimeOffset.Now);

            foreach (var drive in DriveInfo.GetDrives())
            {
                if (drive.VolumeLabel.Equals("CIRCUITPY", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation("CIRCUITPY drive found at {Drive}", drive.Name);

                    _logger.LogInformation("Performing code.py backup check");

                    try
                    {
                        PerformCodePyArchival(drive.Name);
                    }
                    catch (IOException ioex)
                    {
                        // Not uncommon to see:
                        // The process cannot access the file 'E:\code.py' because it is being used by another process.
                        _logger.LogError(ioex, "IO Exception during processing. Logging and moving on.");
                    }
                }
            }

            await Task.Delay(5000, stoppingToken);
        }
    }

    private void PerformCodePyArchival(string driveName)
    {
        // Get the file
        var fileName = "code.py";
        var codePyFilePath = Path.Join(driveName, fileName);
        if (!File.Exists(codePyFilePath))
        {
            _logger.LogError("code.py file not found");
        }
        else
        {
            _logger.LogInformation("code.py file found at {Path}, calculating checksum", codePyFilePath);

            // Calc the checksum
            var checksum = CalculateChecksum(codePyFilePath);

            _logger.LogInformation("code.py checksum {Checksum}", checksum);

            var fileChecksumCollection = store.GetCollection<FileChecksum>();

            // Check if different
            var existingFileChecksum = fileChecksumCollection
                .AsQueryable()
                .SingleOrDefault(e => e.FileName == "code.py");

            if (existingFileChecksum is null)
            {
                _logger.LogInformation("code.py not found in cache, adding");

                // Add to database
                fileChecksumCollection.InsertOne(new FileChecksum
                {
                    Id = 1,
                    FileName = "code.py",
                    Checksum = checksum
                });

                ArchiveFile(codePyFilePath);
            }
            else
            {
                _logger.LogInformation("code.py found in cache, checking checksum");

                if (!checksum.Equals(existingFileChecksum.Checksum))
                {
                    _logger.LogInformation("File changed, archiving.");

                    ArchiveFile(codePyFilePath);

                    _logger.LogInformation("Updating cache.");

                    // Update the checksum in the database
                    fileChecksumCollection.UpdateOne(
                        existingFileChecksum.Id,
                        existingFileChecksum);
                }
                else
                {
                    _logger.LogInformation("File already in cache.");
                }
            }
        }
    }

    private void ArchiveFile(string codePyFilePath)
    {
        // Archive and add timestamp to file name
        var archiveFileName = Path.Join(archivePath, $"code-{DateTime.Now.ToString("yyyyMMddHHmmss")}.py");
        File.Copy(codePyFilePath, archiveFileName);
        _logger.LogInformation("Archived to cache {ArchivedFileName}", archiveFileName);
    }

    private string CalculateChecksum(string fileName)
    {
        using var md5 = MD5.Create();
        using var stream = File.OpenRead(fileName);
        byte[] checksum = md5.ComputeHash(stream);
        return BitConverter.ToString(checksum).Replace("-", string.Empty).ToLower();
    }
}