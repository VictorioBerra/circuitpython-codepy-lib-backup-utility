namespace CircuitPythonBackupService.Models
{
    public class FileToStage
    {
        public required string FileName { get; set; }

        public required string FullPath { get; set; }

        public required string DestintionGitFullPath { get; set; }
    }
}