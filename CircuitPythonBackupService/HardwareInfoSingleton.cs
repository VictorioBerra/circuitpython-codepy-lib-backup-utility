using Hardware.Info;

namespace CircuitPythonBackupService
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