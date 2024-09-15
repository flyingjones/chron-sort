# Image Sorter

A simply utility to sort a bunch of files chronologically based on file names and file meta data.

The tool can evaluate meta data of the following file types:

**Images**: ``jpg``, ``jpeg``, ``tif``, ``tiff``, ``wav``, ``png``, ``webp``

**Videos**: ``mp4``, ``mov``, ``qt``

And will use the last write time of a file as a fallback.

## How to use

Download the binary for your platform and execute it from a terminal.

### Quick Start

I want to sort all files in directory ``<source>``: ``ImageSorter <source> --move``

I want to sort all files from ``<source>`` and move the result to ``<destination>``: ``ImageSorter <source> --move --out <destination>``

I want to create a sorted copy of all files from ``<source>`` at ``<destination>``: ``ImageSorter <source> --out <destination>``

**Note**: Moving _on the same disk_ is significantly faster than copying.
Moving _between disks_ is slower and could lead to data loss so use with caution.
On Windows you can easily see if its on the same disk by checking if the first letter of the paths are the same (e.g. the ``C`` in ``C:\Users\..``).

If you are fine with using the file name based parsers in preference of the exif tag parsers, you can use ``--fast-scan``
which is significantly faster but may not be as accurate.

### Full Reference

(Print the help using ``ImageSorter --help``)

````
Description:
  Sorts files chronologically in the directory structure year/month
  Default sort configuration (change with --configure):
  ExifTag:DateTimeOriginal
  ExifTag:DateTimeDigitized
  ExifTag:DateTime
  QuickTimeMovieHeader:CreationTime
  QuickTimeMovieHeader:ModificationTime
  FileName:.*(?<year>20[0-9]{2}|19[0-9]{2})-(?<month>0[0-9]|1[0-9])-(?<day>0[0-9]|1[0-9]|2[0-9]|3[0-1]).*
  FileName:.*(?<year>20[0-9]{2}|19[0-9]{2})(?<month>0[0-9]|1[0-9])(?<day>0[0-9]|1[0-9]|2[0-9]|3[0-1]).*

Usage:
  ImageSorter <source path> [options]

Arguments:
  <source path>  The path of the source directory

Options:
  --dest, --out <dest>                                               The path of the destination directory (required if not --move)
  --move                                                             Move files instead of copy [default: False]
  --overwrite                                                        Overwrite files in destination [default: False]
  --dry-run                                                          Don't move or copy any files, just print the planned operations to a file [default: False]
  --format <format>                                                  Output directory structure (date format specifier separated by / ) [default: yyyy/MM]
  -c, --configure <configure>                                        Custom sort configuration. Parsers will be applied in order. Possible Formats:
                                                                     ExifTag:DateTimeOriginal                                       [Tries to use the exif tag 0x9003 to get a date]
                                                                     ExifTag:DateTimeDigitized                                      [Tries to use the exif tag 0x9004 to get a date]
                                                                     ExifTag:DateTime                                               [Tries to use the exif tag 0x0132 to get a date]
                                                                     QuickTimeMovieHeader:CreationTime                              [Tries to use the quick time movie header (mvhd) 'Creation time' to get a date]
                                                                     QuickTimeMovieHeader:ModificationTime                          [Tries to use the quick time movie header (mvhd) 'Modification time' to get a date]
                                                                     FileName:<Regex with named capture groups year month and day>  [Tries to parse the file name using a regular expression to get a date]
  --skip-parser-when-before <skip-parser-when-before>                Skip the result of a parser when the resulting date is earlier [default: 01/01/1900 00:00:00]
  --skip-parser-when-after <skip-parser-when-after>                  Skip the result of a parser when the resulting date is later [default: <current date + 1 year> 00:00:00]
  -t, --types <types>                                                Space seperated list of file endings to sort
  --from <from>                                                      Minimum date for files to sort
  --to <to>                                                          Maximum date for files to sort
  --fast-scan, --prefer-file-name-parsing                            Prefer FileName parsers over metadata-based parsers (which is significantly faster since parsing a file name which already is in memory doesn't use I/O)      
  --scan-parallel                                                    Perform the scan part in parallel [default: False]
  --log-level <Critical|Debug|Error|Information|None|Trace|Warning>  Log Level [default: Information]
  -v, --verbose                                                      Same as --log-level Trace
  --progress-at <progress-at>                                        Processed file count after which a progress update is printed [default: 1000]
  --version                                                          Show version information
  -?, -h, --help                                                     Show help and usage information
````

## Releases

Checkout the [release notes](ReleaseNotes/Releases.md)

## Build it yourself

The executable can be built using the command 
``dotnet publish -r <target> -c Release``

For x64 modern Windows: ``dotnet publish -r win-x64 -c Release``

For x64 linux: ``dotnet publish -r linux-x64 -c Release``

(You need to be on the correct platform for that).
