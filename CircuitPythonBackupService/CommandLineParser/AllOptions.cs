using CommandLine.Text;
using CommandLine;
using Newtonsoft.Json;

namespace CircuitPythonBackupService.CommandLineParser
{
    public class AllOptions
    {
        [Option("use-lib-interval-worker", Required = false, HelpText = "Use the simple lib interval worker.")]
        public bool UseLibIntervalWorker { get; set; } = true;

        [Option("lib-interval-worker-interval-seconds", Required = false, HelpText = "How often to scan for drives and perform a /lib backup.")]
        public int LibWorkerIntervalSeconds { get; set; } = 60 * 5;

        [Option("lib-interval-worker-archive-path", Required = false, HelpText = "How often to scan for drives and perform a /lib backup.")]
        public string LibWorkerArchivePath { get; set; } = @"D:\Source\ESP32-S3-Archive";

        [Option("lib-interval-worker-create", Required = false, HelpText = "Create the archive folder if it does not exist.")]
        public bool LibWorkerCreateDirectory { get; set; } = false;

        [Option("use-codepy-git-worker", Required = false, HelpText = "Use the git backup backup worker.")]
        public bool UseCodePyGitWorker { get; set; } = true;

        [Option("codepy-git-worker-interval-seconds", Required = false, HelpText = "How often to scan for drives and perform a git commit.")]
        public int CodePyGitWorkerIntervalSeconds { get; set; } = 10;

        [Option("git-repo-path", Required = false, HelpText = "The location of your git repo.")]
        public string CodePyGitWorkerRepoDirectory { get; set; } = @"D:\Source\ESP32-S3-Archive-VCS";

        [Option("git-repo-create", Required = false, HelpText = "Create the repo directory if it does not exist.")]
        public bool CodePyGitWorkerRepoCreateDirectory { get; set; } = false;

        [Option("git-repo-files", Required = false, HelpText = "The files to stage, defaults to only code.py.", Default = new[] { "code.py" })]
        public IEnumerable<string> CodePyGitWorkerFiles { get; set; } = null!;
    }
}