# Mechanic

A CLI tool for managing Bethesda Game Studios modding projects, helping you track and manage source files and their corresponding game files.

## Overview

Mechanic is a .NET-based command-line tool designed to streamline the workflow for modders working on Bethesda games like Skyrim Special Edition, Skyrim, Fallout 4, and Starfield. It helps you:

- Track source files (textures, scripts, etc.) and their corresponding game files
- Monitor file changes and automatically sync them to your game directory
- Generate Papyrus project files with Pyro support
- Verify that tracked files exist and are up to date

## Supported Games

- Skyrim Special Edition
- Skyrim
- Fallout 4
- Starfield

## Features

- **Project Initialization**: Set up new modding projects with game-specific configurations
- **File Tracking**: Track source files and their corresponding game files
- **File Watching**: Automatically monitor changes to tracked files
- **File Validation**: Check that tracked files exist and are synchronized
- **Pyro Integration**: Generate Papyrus project files for script compilation
- **Interactive CLI**: User-friendly command-line interface with fuzzy file matching

## Requirements

- .NET 9.0 or later
- Windows (for Steam/registry integration)

## Installation

Clone the repository and build the project:

```bash
git clone https://github.com/yak3d/mechanic.git
cd Mechanic
dotnet build
```

## Usage

### Initialize a Project

Create a new Mechanic project in your current directory:

```bash
mechanic init
```

This creates a `mechanic.json` file that stores your project configuration.

### Configure Project Settings

Modify project settings like Pyro usage:

```bash
mechanic configure
```

### Track Files

Add source files to track:

```bash
mechanic file src add
```

Add game files to track:

```bash
mechanic file game add
```

List all tracked files:

```bash
mechanic file ls
# or
mechanic file list
```

Remove files from tracking:

```bash
# Interactive mode
mechanic file src rm
mechanic file game rm

# By ID
mechanic file src rm --id <file-id>

# By path
mechanic file game rm --path "textures/project/metal01.dds"
```

### Check File Status

Verify that tracked files exist and are up to date:

```bash
mechanic check
```

### Watch for Changes

Monitor tracked files and automatically sync changes:

```bash
mechanic watch
```

## Project Structure

- `Mechanic.CLI`: Command-line interface and user-facing commands
- `Mechanic.Core`: Core business logic, services, and models
- `Mechanic.BuildTasks`: MSBuild tasks for code generation
- `Mechanic.CLI.Tests`: Tests for CLI components
- `Mechanic.Core.Tests`: Tests for core functionality

## Configuration

The `mechanic.json` file stores your project configuration:

```json
{
  "$schema": "https://raw.githubusercontent.com/yak3d/mechanic/refs/heads/main/Mechanic.Core/ProjectFileSchema.json",
  "id": "com.example.MyMod",
  "namespace": "MyMod",
  "game": {
    "path": "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Skyrim Special Edition",
    "name": "SkyrimSpecialEdition"
  },
  "projectSettings": {
    "usePyro": false
  },
  "sourceFiles": [],
  "gameFiles": []
}
```

## Development

### Building

```bash
dotnet build
```

### Running Tests

```bash
dotnet test
```

Or with coverage:

```powershell
.\scripts\RunTestsWithCoverage.ps1
```

## License

See [LICENSE](LICENSE) file for details.

## Contributing

Contributions are welcome! Please feel free to submit issues or pull requests.
