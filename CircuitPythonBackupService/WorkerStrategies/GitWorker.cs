using CircuitPythonBackupService.CommandLineParser;
using CircuitPythonBackupService.Models;
using Hardware.Info;
using LibGit2Sharp;
using Microsoft.Extensions.Options;
using System.Diagnostics.Eventing.Reader;
using System.Reflection;

namespace CircuitPythonBackupService
{
    public class GitWorker : BackgroundService
    {
        private readonly ILogger<GitWorker> logger;
        private readonly AllOptions allOptions;
        private readonly IHardwareInfo hardwareInfo;

        public GitWorker(
            ILogger<GitWorker> logger,
            HardwareInfoSingleton hardwareInfoService,
            AllOptions allOptions)
        {
            this.logger = logger;
            this.allOptions = allOptions;
            this.hardwareInfo = hardwareInfoService.hardwareInfo;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                this.logger.LogInformation("GitWorker running at: {Time}", DateTimeOffset.Now);

                this.hardwareInfo.RefreshDriveList();
                this.logger.LogInformation("Refreshed drive information");

                var circuitPyDrive = this.hardwareInfo.DriveList
                    .Where(d =>
                        d.PartitionList.Any(p =>
                            p.VolumeList.Any(v =>
                                v.VolumeName.Equals("CIRCUITPY", StringComparison.OrdinalIgnoreCase))));

                foreach (var drive in circuitPyDrive)
                {
                    this.logger.LogInformation("Drive found with volume name of 'CIRCUITPY' and serial {circuitPyDriveSerial}", drive.SerialNumber);
                    var circuitPyLogicalVolume = 
                        drive.PartitionList
                        .SelectMany(x => x.VolumeList)
                        .SingleOrDefault(y => y.VolumeName.Equals("CIRCUITPY", StringComparison.OrdinalIgnoreCase));

                    if(circuitPyLogicalVolume is null)
                    {
                        this.logger.LogError("Expected to find a single logical volume with volume name 'CIRCUITPY' for the drive.");
                        return;
                    }

                    this.logger.LogInformation("Found logical volume with volume name 'CIRCUITPY' for the disk with Volume Serial Number {VolumeSerialNumber}.", circuitPyLogicalVolume.VolumeSerialNumber);

                    PerformCodePyGit(circuitPyLogicalVolume.Name, drive.SerialNumber);
                }

                await Task.Delay(1000 * 10, stoppingToken);
            }
        }
        
        private void PerformCodePyGit(string driveLetter, string driveSerialNumber)
        {
            var now = DateTime.UtcNow;
            
            var codePyDestinationGitRepoPath = 
                Path.Join(this.allOptions.CodePyGitWorkerRepoDirectory, driveSerialNumber);

            var filesToStage = this.allOptions.CodePyGitWorkerFiles
                .Select(x => new FileToStage
                { 
                    FileName = x,
                    FullPath = Path.Join(driveLetter, x),
                    DestintionGitFullPath = Path.Join(codePyDestinationGitRepoPath, x),
                });

            this.logger.LogInformation("Files to backup to your git folder {@FilesToStage}", filesToStage);

            foreach (var fileToStage in filesToStage)
            {
                if (!File.Exists(fileToStage.FullPath))
                {
                    this.logger.LogError("File not found {FilePath}, done with backup until next interval.", fileToStage);
                    return;
                }
            }

            if(this.allOptions.CodePyGitWorkerRepoCreateDirectory)
            {
                this.logger.LogInformation("Creating {RepoPath} if not exists.", codePyDestinationGitRepoPath);
                Directory.CreateDirectory(codePyDestinationGitRepoPath);
            }

            if(!Repository.IsValid(codePyDestinationGitRepoPath))
            {
                this.logger.LogInformation("Git repo not found at {RepoPath}.", codePyDestinationGitRepoPath);
                Repository.Init(codePyDestinationGitRepoPath);
                this.logger.LogInformation("Init new repo complete at {RepoPath}.", codePyDestinationGitRepoPath);
            }
            else
            {
                this.logger.LogInformation("Valid git repo detected at {RepoPath}.", codePyDestinationGitRepoPath);
            }

            var identity = new Identity("Backup Program", "BackupProgram@example.com");
            var authorAndCommitter = new Signature(identity, now);

            using var repo = new Repository(
                codePyDestinationGitRepoPath,
                new RepositoryOptions { Identity = identity });
            
            // Verify repo has nothing on the index
            var status = repo.RetrieveStatus();
            LogStatus(status);
            if (status.Untracked.Any() &&
                status.Added.Any() &&
                status.Modified.Any() &&
                status.Removed.Any())
            {
                this.logger
                    .LogWarning("Working directory not clean... there can be no files that are untracked, added, modified or removed.");
                return;
            }

            foreach (var fileToStage in filesToStage)
            {
                File.Copy(
                    fileToStage.FullPath,
                    fileToStage.DestintionGitFullPath,
                    true);
                
                this.logger.LogInformation("Copied file {FileToStageFileName} to repo {RepoPath}.", fileToStage.FileName, codePyDestinationGitRepoPath);
            }

            status = repo.RetrieveStatus();
            LogStatus(status);
            if (!status.Modified.Any() && !status.Untracked.Any())
            {
                this.logger.LogInformation("Git does not see anything modified or untracked, done with backup until next interval.");
                return;
            }

            foreach (var fileToStage in filesToStage)
            {
                Commands.Stage(repo, fileToStage.DestintionGitFullPath);
                this.logger.LogInformation("Staged {FileToStage}.", fileToStage.DestintionGitFullPath);
            }

            Commit commit = repo.Commit(
                "CIRCUITPY backup commit.",
                authorAndCommitter,
                authorAndCommitter);
            
            this.logger.LogInformation("Committed {filesToStageCount} to index.", filesToStage.Count());
            
            this.logger.LogInformation("Total commits so far in repo: {CommitCount}", repo.Commits.Count());
        }

        private void LogStatus(RepositoryStatus status)
        {
            this.logger.LogInformation("Status: +{Added} ~{Staged} -{Removed} | +{Untracked} ~{Modified} -{Missing} | i{Ignored}",
                         status.Added.Count(),
                         status.Staged.Count(),
                         status.Removed.Count(),
                         status.Untracked.Count(),
                         status.Modified.Count(),
                         status.Missing.Count(),
                         status.Ignored.Count());
        }
    }
}
