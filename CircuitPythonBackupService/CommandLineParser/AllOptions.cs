using CommandLine.Text;
using CommandLine;

namespace CircuitPythonBackupService.CommandLineParser
{
    public class AllOptions
    {
        [Option("use-lib-interval-worker", Required = false, HelpText = "Use the simple lib interval worker. This worker zips up the entire lib folder, and saves it with a timestamp in the filename.")]
        public bool UseLibIntervalWorker { get; set; } = true;

        [Option("lib-interval-worker-interval-seconds", Required = false, HelpText = "How often to scan for drives and perform a /lib backup.")]
        public int LibWorkerIntervalSeconds { get; set; } = 60 * 5;

        [Option("lib-interval-worker-archive-path", Required = false, HelpText = "Path to save the lib.zip in.", Default = "D:\\Source\\ESP32-S3-Archive")]
        public string LibWorkerArchivePath { get; set; } = null!;

        [Option("lib-interval-worker-create", Required = false, HelpText = "Create the archive folder if it does not exist.")]
        public bool LibWorkerCreateDirectory { get; set; } = false;

        [Option("use-codepy-git-worker", Required = false, HelpText = "Use the git backup backup worker.")]
        public bool UseCodePyGitWorker { get; set; } = true;

        [Option("codepy-git-worker-interval-seconds", Required = false, HelpText = "How often to scan for drives and perform a git commit.")]
        public int CodePyGitWorkerIntervalSeconds { get; set; } = 10;

        [Option("git-worker-repo-path", Required = false, HelpText = "The location of your git repo.", Default = "D:\\Source\\ESP32-S3-Archive-VCS")]
        public string CodePyGitWorkerRepoDirectory { get; set; } = null!;

        [Option("git-worker-repo-create", Required = false, HelpText = "Create the repo directory if it does not exist.")]
        public bool CodePyGitWorkerRepoCreateDirectory { get; set; } = false;

        [Option("git-worker-repo-files", Required = false, HelpText = "The files to stage, defaults to only code.py.", Default = new[] { "code.py", "index.html", "fake.file" })]
        public IEnumerable<string> CodePyGitWorkerFiles { get; set; } = null!;
    }
}