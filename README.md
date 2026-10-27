# Checkers Game

A C# implementation of Checkers, a strategy board game for two players with diagonal moves and mandatory captures by jumping over opponent pieces. Play from the console, a WPF desktop app, or online against another player.

## Table of contents

- [Features](#features)
- [Project structure](#project-structure)
- [Getting started](#getting-started)
- [Usage](#usage)
- [Online multiplayer](#online-multiplayer-wpf)
- [Running tests](#running-tests)
- [Game rules](#game-rules)
- [Getting help](#getting-help)
- [Third-party notices](#third-party-notices)
- [Contributing](#contributing)

## Features

- Console and WPF (Windows) applications
- Online multiplayer through a SignalR server that validates every move
- Light and dark themes
- Localization (English, Polish)

## Project structure

| Project | Description |
|---------|-------------|
| `Checkers.Console` | Console application |
| `Checkers.WPF` | WPF desktop application (Windows only) |
| `Checkers.Server` | ASP.NET Core SignalR server for online play |
| `Checkers.Tools` | Shared game logic, resources and utilities |
| `Checkers.UnitTests` | NUnit unit tests |
| `Documentation` | Design mockups and images |

## Getting started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Windows to build and run `Checkers.WPF` (it targets `net10.0-windows`)
- Optional: Visual Studio 2022 or newer, or JetBrains Rider

### Build

```powershell
git clone https://github.com/Stani1k/Checkers-Game.git
cd Checkers-Game
dotnet build Checkers-Game.sln
```

## Usage

```powershell
# Console app
dotnet run --project Checkers.Console

# WPF app (Windows)
dotnet run --project Checkers.WPF
```

## Online multiplayer (WPF)

Two players on different computers can play via `Checkers.Server`, a small ASP.NET Core SignalR relay. The server validates every move and forwards it to the opponent.

1. Start the server on a machine both players can reach: `dotnet run --project Checkers.Server --urls http://0.0.0.0:5000` (open the port in the firewall / host it on a public server; use HTTPS behind a reverse proxy for internet play).
2. In the WPF app choose **Play Online**, enter the server address (e.g. `http://my-server:5000`) and your name.
3. Player 1 clicks **Create Game** and shares the 5-character code (plays the light pieces and moves first). Player 2 enters the code and clicks **Join Game**.

Leaving a game or losing the connection forfeits it. Games live in server memory only; there is no reconnect.

## Running tests

```powershell
dotnet test Checkers.UnitTests
```

## Game rules

New to the game? See [Checkers on Wikipedia](https://en.wikipedia.org/wiki/Checkers).

## Getting help

Found a bug or have a question? [Open an issue](https://github.com/Stani1k/Checkers-Game/issues/new/choose).

## Third-party notices

The licenses of the libraries this project uses are listed in [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md). After changing NuGet dependencies, regenerate it with `powershell -File scripts\Update-ThirdPartyNotices.ps1`.

## Contributing

Contributions are welcome. Open an issue using the feature request or task template to discuss a change, then submit a pull request.
