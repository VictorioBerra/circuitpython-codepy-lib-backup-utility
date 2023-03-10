namespace CircuitPythonBackupService.Models
{
    public class FileChecksum
    {
        public int Id { get; set; }

        public required string FileName { get; set; }

        public required string Checksum { get; set; }
    }
}