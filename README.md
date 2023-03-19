# Circuit Python USB Drive Auto Git Backup

Finds circuitpy drives, backs up code.py, and commits to git.

Appends the serial to the git folder in case of multiple drives

## Features

### Git worker

- Runs every few seconds
- Checks for a CIRCUITPY drive
- If found, grabs the drive letter and serial number of your device
- Creates a folder, creates a git repo in the folder
- Copies your code.py (and whatever else you want) to the folder
- Commits the changes to git

### /Lib Worker

- Runs every few minutes
- Checks for a CIRCUITPY drive
- If found, grabs the drive letter and serial number of your device
- Creates a folder using your serial number
- Zips up your /lib folder using the current timestamp for the folder name
- Copies the zip to the folder

## Usage

```
--use-lib-interval-worker                 (Default: true) Use the simple lib interval worker. This worker zips up the entire lib folder, and saves it with a timestamp in the filename.

--lib-interval-worker-interval-seconds    (Default: 300) How often to scan for drives and perform a /lib backup.

--lib-interval-worker-archive-path        (Default: D:\Source\ESP32-S3-Archive) Path to save the lib.zip in.

--lib-interval-worker-create              (Default: false) Create the archive folder if it does not exist.

--use-codepy-git-worker                   Use the git backup backup worker.

--codepy-git-worker-interval-seconds      (Default: 10) How often to scan for drives and perform a git commit.

--git-worker-repo-path                    (Default: D:\Source\ESP32-S3-Archive-VCS) The location of your git repo.

--git-worker-repo-create                  (Default: false) Create the repo directory if it does not exist.

--git-worker-repo-files                   (Default: code.py index.html fake.file) The files to stage, defaults to only code.py.

--help                                    Display this help screen.

--version                                 Display version information.
```  

## TODO

- Release as a dotnet tool on nuget, so anyone with dotnet core installed could just do `dotnet tool install -g circuitpy-git-backup`
- Build using os matrix in CICD instead of running a single build on windows and producing cross-platform binaries

## License

MIT