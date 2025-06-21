# Route Tracker

A specialized tool designed for speedrunners to track their progress across multiple games.

## Overview

Route Tracker reads game memory in real-time to track collectibles, missions, and progress during speedruns. This tool is specifically designed for speedrunners following pre-determined routes.

## Game Support

- **Assassin's Creed 4: Black Flag** (Near Completion)

See [Planned Stuff.md](Planned Stuff.md) for information about future game support.

## Features

### Assassin's Creed 4: Black Flag
- **Real-time Collection Tracking**
  - Viewpoints, Mayan Stones, Chests, Animus Fragments
  - Assassin Contracts, Naval Missions, Letters, Manuscripts & Music Sheets
  - Taverns and other collectibles

- **Mission Progress**
  - Story Mission completion detection
  - Templar Hunt tracking
  - Legendary Ship battles

- **Upgrade Tracking**
  - Weapons purchases
  - Ship upgrades
  - Hero equipment upgrades

### Important Notes

- **Speedrun Focus**: This tool is designed for speedrunners following specific routes. Casual players must follow routes exactly from the beginning.
- **Route Files**: Routes are customizable by editing local files.
- **Release Status**: Version 1.0 planned for release within the next few months.

## How It Works

Route Tracker reads game memory to access progression data without modifying game files or gameplay, making it tournament-legal for speedruns where such tools are permitted.

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
- Routes stored in editable files for customization

## Credits

- Memory address discovery and pointer tracking assistance by **Ero** (Discord)
- Code development assistance by **GitHub Copilot**

## Developer's Note

While this project uses AI assistance, all ideas, requirements, and direction come directly from me. I've worked with tools like GitHub Copilot to implement my vision for this project.

The extensive comments throughout the code serve as reminders when I return after breaks from development. They help me quickly understand what each component does without having to relearn the entire codebase.

This project has been a significant learning experience. Future game implementations will be coded manually as I continue to develop my programming skills.

## Legal Notice

This tool does not modify any game files or game memory. It only reads data already accessible within the games. Use at your own discretion as terms of service for online components may vary.

**Disclaimer**: A significant portion of the code in this project was generated with AI assistance through GitHub Copilot, with manual edits and adjustments to ensure functionality. Some code components, particularly the pointer-reading functionality, were contributed by community members like Ero.