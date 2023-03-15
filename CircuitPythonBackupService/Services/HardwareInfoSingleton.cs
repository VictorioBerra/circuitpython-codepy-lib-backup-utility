using Hardware.Info;

namespace CircuitPythonBackupService.Services
{
    public class HardwareInfoSingleton
    {
        public readonly IHardwareInfo hardwareInfo;

        public HardwareInfoSingleton()
        {
            hardwareInfo = new HardwareInfo();
        }
    }
}