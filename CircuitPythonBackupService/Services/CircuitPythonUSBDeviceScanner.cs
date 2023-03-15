using CircuitPythonBackupService.Models;

namespace CircuitPythonBackupService.Services;

public class CircuitPythonUSBDeviceScanner
{
    private readonly ILogger<CircuitPythonUSBDeviceScanner> logger;
    private readonly HardwareInfoSingleton hardwareInfoService;

    public CircuitPythonUSBDeviceScanner(
        ILogger<CircuitPythonUSBDeviceScanner> logger,
        HardwareInfoSingleton hardwareInfoService)
    {
        this.logger = logger;
        this.hardwareInfoService = hardwareInfoService;
    }

    public List<CircuitPythonDriveInformation> FindCircuitPythonDrives()
    {
        this.hardwareInfoService.hardwareInfo.RefreshDriveList();
        this.logger.LogInformation("Refreshed drive information");

        var circuitPyDrive = this.hardwareInfoService.hardwareInfo.DriveList
            .Where(d =>
                d.PartitionList.Any(p =>
                    p.VolumeList.Any(v =>
                        v.VolumeName.Equals("CIRCUITPY", StringComparison.OrdinalIgnoreCase))));

        var drives = new List<CircuitPythonDriveInformation>();
        foreach (var drive in circuitPyDrive)
        {
            this.logger.LogInformation("Drive found with volume name of 'CIRCUITPY' and serial {circuitPyDriveSerial}", drive.SerialNumber);
            var circuitPyLogicalVolume =
                drive.PartitionList
                .SelectMany(x => x.VolumeList)
                .SingleOrDefault(y => y.VolumeName.Equals("CIRCUITPY", StringComparison.OrdinalIgnoreCase));

            if (circuitPyLogicalVolume is null)
            {
                this.logger.LogError("Expected to find a single logical volume with volume name 'CIRCUITPY' for the drive.");
            }
            else
            {
                this.logger.LogInformation("Found logical volume with volume name 'CIRCUITPY' for the disk with Volume Serial Number {VolumeSerialNumber}.", circuitPyLogicalVolume.VolumeSerialNumber);
                drives.Add(new CircuitPythonDriveInformation
                {
                    Name = circuitPyLogicalVolume.Name,
                    SerialNumber = drive.SerialNumber
                });
            }
        }

        return drives;
    }
}