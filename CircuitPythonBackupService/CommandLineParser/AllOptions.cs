using CommandLine.Text;
using CommandLine;
using Newtonsoft.Json;

namespace CircuitPythonBackupService.CommandLineParser
{
    public class AllOptions
    {
        [Option("use-lib-interval-worker", Required = false, HelpText = "Use the simple lib interval worker.")]
        public bool UseLibIntervalWorker { get; set; }

        [Option("use-codepy-git-worker", Required = false, HelpText = "Use the git backup backup worker.")]
        public bool UseCodePyGitWorker { get; set; } = true;

        [Option("git-repo-path", Required = false, HelpText = "The location of your git repo.")]
        public string CodePyGitWorkerRepoDirectory { get; set; } = @"D:\Source\ESP32-S3-Archive-VCS";

        [Option("git-repo-create", Required = false, HelpText = "Create the repo if it does not exist.")]
        public bool CodePyGitWorkerRepoCreateDirectory { get; set; } = true;

        [Option("git-repo-files", Required = false, HelpText = "The files to stage, defaults to only code.py.", Default = new[] { "code.py" })]
        public IEnumerable<string> CodePyGitWorkerFiles { get; set; } = null!;
    }
}