# Route Tracker

A specialized tool designed for speedrunners to track their progress across multiple games.

## Overview

Route Tracker reads game memory in real-time to track collectibles, missions, and progress during speedruns. This tool is specifically designed for speedrunners following pre-determined routes.

## Game Support

- **Assassin's Creed 4: Black Flag** (Complete)
- **God of War (2018)** (Early Implementation)

See [Planned Stuff.md](Planned Stuff.md) for information about future game support.

## Features

### Core Features
- **Always On Top**: Keep the tracker visible while playing by toggling "Always On Top" in Settings
- **Progress Saving/Loading**: Save your progress and load it later to continue tracking where you left off
- **Text Wrapping**: Better readability for long route descriptions
- **Automatic Game Detection**: Tool automatically detects supported games that are running
- **Performance Optimizations**: Adaptive update rates reduce resource usage during idle periods
- **Customizable Hotkeys**: Configure keyboard shortcuts to mark route entries as completed or skipped
- **Manual Entry Management**: Ability to manually complete or skip route entries without game detection

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

- **Game State Detection**
  - Automatically detects loading screens and main menu
  - Preserves tracking data through game transitions

### God of War (2018)
- **Basic Tracking Implementation**
  - Initial support for basic stats and progression
  - More features coming soon

### Important Notes

- **Speedrun Focus**: This tool is designed for speedrunners following specific routes. Casual players must follow routes exactly from the beginning.
- **Route Files**: Routes are customizable by editing local files.
- **Release Status**: Version 1.0 released with full AC4 support; additional game support in progress.

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
- Memory caching system for improved performance

## Credits

- Guidance on memory-reading method implementation by **Ero** (Discord)
- Code development assistance through **GitHub Copilot**
- All memory addresses and pointers discovered by **NotTpRedNinja**

## Developer's Note

This project was developed through a combination of my own code and assistance from GitHub Copilot. I wrote some of the code not much but some or modify code when it was shit, discovered all memory addresses and pointers, and designed the overall architecture.

The extensive comments throughout the code serve as reminders when I return after breaks from development. They help me quickly understand what each component does without having to relearn the entire codebase.

This project represents my learning journey in software development, combining my game knowledge with programming concepts to create a useful tool for the speedrunning community.

## Legal Notice

This tool does not modify any game files or game memory. It only reads data already accessible within the games. Use at your own discretion as terms of service for online components may vary.

**Disclaimer**: This project was developed with some AI assistance through GitHub Copilot alongside significant custom implementation and game memory research by the author.