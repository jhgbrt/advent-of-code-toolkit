## Advent of Code toolkit for dotnet

This project is part of the Net.Code.AdventOfCode.Toolkit and is
intended to help solving the advent of code puzzles in C#. 

This project is not part of the official adventofcode.com website.

## Getting started

### Installing the template

`dotnet new install Net.Code.AdventOfCode.Toolkit.Template:1.0.4`

(the latest version is 1.0.4 at the time of writing).

### Creating a new project 

This is similar to a standard console project:

`dotnet new aoc`

## Authentication

Many commands from the aoc toolkit require authenticated interaction with
adventofcode.com. To enable this, you need to set the AOC_SESSION variable, 
either as an environment variable or (recommended) as a dotnet user-secret.

This variable should be set to the session cookie header from a logged-in
session on adventofcode.com. So start by heading to https://adventofcode.com,
log in and open the browser devtools. Under 'network', request headers, you should
find cookies. There you will see a value 'session=xxxxxxxxxxx', with xxxxxxxxxxx a
long alphanumeric string. Copy the entire string (only the xxxxxxxxxxx part), and 
set it as a dotnet user secret:

```
dotnet user-secrets init
dotnet user-secrets set AOC_SESSION xxxxxxxxxxx //<- replace xxxxxxxxxxx by the actual session cookie value
```

## Getting started and general usage

The aoc template is a console application with a pre-provided Program.cs (and main method)
that calls into the toolkit library.

This means that you can use `dotnet run -- [aoc command]` in your terminal to run the different commands.

For example:
* `dotnet run -- --help` shows an overview of all available commands
* `dotnet run -- init` initializes todays puzzle 
* `dotnet run -- run` runs the code for todays puzzle

## Templates

### C# file for a puzzle

The Advent of code toolkit assumes it will find a file called 'AoC.cs' under 'Templates'. 
An example is already part of the template, but you can modify it to your own needs.

This template will be copied to a subfolder under YearYYYY\DayDD, with
YYYY and DD replaced by the actual year and day, respectively. This is where you 
add code to solve the actual puzzle for that day.

When looking for the code of a particular puzzle, the engine 
looks for a namespace that has the year (YYYY) and day (dayDD, case insensitive) 
in it. For example: MyAdventOfCode.Y2022.Day04 is a valid namespace.

Within that namespace, a class should exist that contains 2 parameterless 
instance methods called Part1() and Part2().

As long as you satisfy these requirements, you may modify the template AoC.cs class.

As long as such method returns null or empty string, the engine considers the
puzzle unsolved. A solved puzzle should obviously return the correct value. 

You can use any return type (string, int, long, BigInteger, ...), as long as the
`ToString()` representation of your return value returns the value as it should
be posted to adventofcode.com.

### csproj template file for export

The AoC toolkit has an `export` command, that allows to export your solution
to a stand-alone project (which will just work in most cases, but may require
some manual tweaking).

The csproj file for this export is taken from the aoc.csproj under templates.
A good default is provided, but you may change it to suit your needs.


