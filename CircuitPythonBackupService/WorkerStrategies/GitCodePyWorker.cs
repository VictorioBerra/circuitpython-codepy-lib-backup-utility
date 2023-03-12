using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace CircuitPythonBackupService
{
    public class GitCodePyWorker : BackgroundService
    {
        private readonly ILogger<GitCodePyWorker> _logger;
        private readonly string archivePath;

        public GitCodePyWorker(ILogger<GitCodePyWorker> logger)
        {
            _logger = logger;

            archivePath = @"D:\Source\ESP32-S3-Archive-VCS";
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

                        PerformCodePyGit(drive.Name);
                    }
                }

                await Task.Delay(5000, stoppingToken);
            }
        }
        
        private void PerformCodePyGit(string driveName)
        {
            var now = DateTime.UtcNow;

            var fileName = "code.py";
            var codePyFilePath = Path.Join(driveName, fileName);
            var codePyDestinationGitRepoPath = Path.Join(this.archivePath, "code.py");
            if (!File.Exists(codePyFilePath))
            {
                this._logger.LogError("code.py file not found");
            }

            var identity = new Identity("Backup Program", "BackupProgram@example.com");
            var authorAndCommitter = new Signature(identity, now);

            using var repo = new Repository(
                this.archivePath,
                new RepositoryOptions { Identity = identity });

            this._logger.LogInformation("Using git repo at {RepoPath}", repo.Info.Path);

            // Verify repo has nothing on the index
            var status = repo.RetrieveStatus();
            if (status.Untracked.Any() &&
                status.Added.Any() &&
                status.Modified.Any() &&
                status.Removed.Any())
            {
                this._logger.LogWarning("Working directory not clean... please commit or stash changes before running this program.");
                return;
            }

            File.Copy(codePyFilePath, codePyDestinationGitRepoPath, true);
            this._logger.LogInformation("Updated code.py file in archive repo.");

            status = repo.RetrieveStatus();
            if(!status.Modified.Any())
            {
                this._logger.LogInformation("Git does not see any changes.");
                return;
            }

            Commands.Stage(repo, codePyDestinationGitRepoPath);
            this._logger.LogInformation("Staged code.py to git index.");

            Commit commit = repo.Commit(
                "Backup of CodePy file.",
                authorAndCommitter,
                authorAndCommitter);
            this._logger.LogInformation("Committed to index.");
            
            this._logger.LogInformation("Commits so far in repo: {CommitCount}", repo.Commits.Count());
        }
    }
}
