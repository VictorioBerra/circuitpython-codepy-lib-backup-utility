# Code.py and /lib folder backup

Nieve process to compare the code.py checksum with one in a little datastore, and then to back it up if its changed.

Even more Nieve process to just backup the lib folder as a zip on an interval.

A very simple but effective way to backup your code.py and lib folder and better than nothing.

## Usage

Uncomment the worker you want to use

### CodePyRapidWorker

This worker will backup code.py every on an interval if it has changed using MD5 checksum.

Saves into the destination folder using a simple timestamp on the filename.

### GitCodePyWorker

Uses git. Copies the file over to the repo, adds, and commits.

*Note:* Init a git repo in the destination folder first.

### LibFolderWorker

Backs up the lib folder as a zip file on an interval.

## Todo

- /lib to use the same checksum process as code.py?
- support more than just code.py
- everything configurable though CLI args (System.CommandLine)
	- Like which workflow/strategy to run
	- Workflow options

## License

MIT