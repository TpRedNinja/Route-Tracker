# Route Tracker

A specialized tool designed for speedrunners to track their progress across multiple games.

## Overview

Route Tracker is a standalone application that reads game memory in real-time to track collectibles, missions, and progress during speedruns. This tool is specifically designed for speedrunners who follow pre-determined routes, not for casual gameplay.

## Game Support Status

- **Assassin's Creed 4: Black Flag** (Work in Progress)

## Planned Support

- **God of War (2018)**
- **God of War Ragnarok**
- Additional games to be announced

## Features

### Assassin's Creed 4: Black Flag
- **Real-time Collection Tracking** (In Development)
  - Viewpoints, Mayan Stones, Chests, Animus Fragments
  - Assassin Contracts, Naval Missions, Letters, Manuscripts & Music Sheets
  - Taverns and other collectibles

- **Mission Progress** (Still Developing)
  - Story Mission completion detection
  - Templar Hunt tracking
  - Legendary Ship battles

### Important Notes

- **Speedrun Focus**: This tool is designed for speedrunners following specific routes. Casual players would need to follow the route exactly from the beginning.
- **Route Files**: The route is still a work in progress. Users can edit route files locally to customize their runs.
- **Release Status**: No official releases are currently planned for the immediate future as development continues.

## How It Works

The Route Tracker reads a game's memory to access progression data. It does this without modifying any game files or gameplay, making it tournament-legal for speedruns where such tools are permitted.

## System Requirements

- Windows 10 or later
- .NET 8 Runtime
- Supported games (installed separately)
- Administrator privileges (required for memory reading)

## Contributing

If you're interested in contributing to this project, please reach out via Discord:
- **Discord**: NotTpRedNinja

Areas where help is particularly welcome:
- Memory address discovery
- Route optimization
- Additional game support
- UI improvements

## Technical Details

- Built with C# and .NET 8
- Uses memory reading techniques to access game data
- Routes are stored in editable files that users can customize

## Credits

- Memory address discovery and pointer tracking assistance by **Ero** (Discord)
- Code development assistance by **GitHub Copilot**

## Legal Notice

This tool does not modify any game files or game memory. It only reads data that is already accessible within the games. However, use at your own discretion as terms of service for online components may vary.