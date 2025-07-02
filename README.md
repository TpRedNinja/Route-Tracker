# Route Tracker

A specialized tool designed for speedrunners to track their progress across multiple games with real-time memory reading and advanced route management features.

## Overview

Route Tracker reads game memory in real-time to track collectibles, missions, and progress during speedruns. This tool is specifically designed for speedrunners following pre-determined routes with comprehensive tracking and management capabilities.

## Game Support

- **Assassin's Creed 4: Black Flag** (Complete)
- **God of War (2018)** (Early Implementation)

See [Planned Stuff.md](Planned Stuff.md) for information about future game support.

## Features

### Core Features
- **Always On Top**: Keep the tracker visible while playing by toggling "Always On Top" in Settings
- **Automatic Game Detection**: Tool automatically detects supported games that are running
- **Real-time Memory Reading**: Live tracking of game statistics without modifying game files
- **Performance Optimizations**: Adaptive update rates and memory caching reduce resource usage
- **Progress Saving/Loading**: Save your progress and load it later with cycling backup system
- **Manual Entry Management**: Mark route entries as completed or skipped manually
- **Right-click Context Menu**: Quick access to save/load progress and route file management

### Advanced UI Features
- **Multiple Layout Modes**: 
  - Normal - Full interface with all controls
  - Compact - Reduced interface, hidden filters
  - Mini - Essential completion tracking only
  - Overlay - Minimal vertical layout perfect for streaming
- **Search and Filtering**: Real-time search through route entries with type-based filtering
- **Customizable Hotkeys**: Configure keyboard shortcuts to mark entries as completed or skipped
- **Separate Statistics Windows**: Dedicated windows for game stats and route completion stats
- **Text Wrapping**: Better readability for long route descriptions
- **Dark Theme**: Consistent dark theme throughout the application

### Connection and Game Management
- **Connection Window**: Streamlined game connection interface
- **Auto-Start Game Support**: Automatically launch and connect to configured games on startup
- **Game Directory Management**: Set custom game installation paths for each supported game
- **Connection Status Indicators**: Clear feedback on connection state

### Update Management
- **Automatic Update Check**: On startup, Route Tracker can automatically check for new releases on GitHub
- **Update Settings**: Enable or disable automatic update checks via Settings menu
- **In-App Update Download**: Download and install updates directly from the app
- **Developer Mode**: Passcode-protected dev mode that disables update checks for development

### Enhanced Data Management
- **Cycling Autosave System**: Multiple numbered backup files with automatic rotation
- **Settings Backup**: Automatic backup of all settings to AppData with restore functionality
- **Progress Recovery**: Fallback system to load from backup files if main save is corrupted
- **Settings Restoration**: First-run detection with option to restore from previous installation

### Assassin's Creed 4: Black Flag
- **Real-time Collection Tracking**
  - Viewpoints, Mayan Stones, Chests, Animus Fragments
  - Assassin Contracts, Naval Missions, Letters, Manuscripts & Music Sheets
  - Taverns and other collectibles

- **Mission Progress**
  - Story Mission completion detection
  - Templar Hunt tracking
  - Legendary Ship battles
  - Treasure Map collection
  - Modern Day mission detection

- **Upgrade Tracking**
  - Ship upgrades with resource expenditure detection
  - Hero equipment upgrades
  - Animal skin upgrades with checkpoint system

- **Game State Detection**
  - Loading screen detection
  - Main menu detection
  - Automatic save/load during state transitions

### God of War (2018)
- **Basic Tracking Implementation**
  - little to no support
  - full support coming after v1.0 releases

### Recent Improvements

- **Enhanced UI Layout System**: Four distinct layout modes for different use cases
- **Improved Connection Management**: Streamlined connection window with better game detection
- **Advanced Backup Systems**: Both progress and settings now have robust backup mechanisms
- **Memory Optimization**: Caching system reduces redundant memory reads
- **UI Update Throttling**: Prevents excessive redraws for better performance
- **Developer Tools**: Protected developer mode for testing and development

### Important Notes

- **Speedrun Focus**: This tool is designed for speedrunners following specific routes
- **Route Files**: Routes are fully customizable by editing local TSV files
- **Memory Safety**: Only reads game memory, never modifies game files or data
- **Tournament Legal**: Memory reading approach is legal for tournaments where such tools are permitted

## How It Works

Route Tracker uses Windows API calls to read game memory and access progression data without modifying game files or gameplay. The application features a sophisticated memory caching system and adaptive update frequencies for optimal performance.

## System Requirements

- Windows 10 or later (Windows 6.1+ supported)
- .NET 8 Runtime
- Supported games (installed separately)
- Administrator privileges (required for memory reading)

## Contributing

If you're interested in contributing to this project, please reach out via Discord:
- **Discord**: NotTpRedNinja

Areas where help is particularly welcome:
- Memory address discovery for new games
- Route optimization and creation
- Additional game support implementation
- UI/UX improvements
- Performance optimizations

## Technical Details

- Built with C# 12.0 and .NET 8
- Uses ProcessMemory64 library for memory access
- Routes stored in editable TSV files for easy customization
- Memory caching system with configurable cache duration
- Adaptive timer system for performance optimization
- JSON-based settings and backup system
- **Auto-update system**: Checks GitHub releases API for updates

## Credits

- Guidance on memory-reading method implementation by **Ero** (Discord)
- Code development assistance through **GitHub Copilot**
- All memory addresses and pointers discovered by **NotTpRedNinja**

## Developer's Note

This project was developed through a combination of custom implementation and assistance from GitHub Copilot. I personally discovered all memory addresses and pointers, designed the overall architecture, and wrote/modified significant portions of the code.

The extensive comments throughout the code serve as documentation and reminders when returning to development after breaks. They help quickly understand component functionality without having to relearn the entire codebase.

This project represents my journey in software development, combining game knowledge with programming concepts to create a comprehensive tool for the speedrunning community.

## Legal Notice

This tool does not modify any game files or game memory. It only reads data already accessible within the games. Use at your own discretion as terms of service for online components may vary.

**Disclaimer**: This project was developed with AI assistance through GitHub Copilot alongside significant custom implementation and game memory research by the author.