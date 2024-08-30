# Image Sorter

A simply utility to sort a bunch of images chronologically.

## How to use

Download the binary for your platform and execute it from a terminal.

Print the help using ``ImageSorter --help``:

````
TBD Update me

Description:
  Sorts images chronologically in the directory structure year/month
  Will use the following clues to determine a date for each file in order:

  - Exif Tag DateTimeOriginal  (0x9003)
  - Exif Tag DateTimeDigitized (0x9004)
  - Exif Tag DateTime          (0x0132)
  - Regex for file name: .*(?<year>20[0-9]{2}|19[0-9]{2})-(?<month>0[0-9]|1[0-9])-(?<day>0[0-9]|1[0-9]|2[0-9]|3[0-1]).* : detects yyyy-mm-dd
  - Regex for file name: .*(?<year>20[0-9]{2}|19[0-9]{2})(?<month>0[0-9]|1[0-9])(?<day>0[0-9]|1[0-9]|2[0-9]|3[0-1]).*   : detects yyyymmdd
  - Last edit date from file system


Usage:
  ImageSorter <source path> [options]

Arguments:
  <source path>  The path of the source directory

Options:
  --dest <dest>                The path of the destination directory (required if not --move)
  --move                       Move files instead of copy [default: False]
  -t, --types <types>          Space seperated list of file endings to sort
  --overwrite                  Overwrite files in destination [default: False]
  --from <from>                Minimum date for files to sort
  --to <to>                    Maximum date for files to sort
  --scan-parallel              Perform the scan part in parallel [default: False]
  --progress-at <progress-at>  Processed file count after which a progress update is printed [default: 1000]
  --version                    Show version information
  -?, -h, --help               Show help and usage information
````

## Releases

Checkout the [release notes](ReleaseNotes/Releases.md)

## TODOs

- Better readme
- Add support for user provided regex for parsing file names
- Add support for more customization (after what should be sorted in which order)
