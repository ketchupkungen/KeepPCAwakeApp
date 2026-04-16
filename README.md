# KeepPCAwakeApp

KeepPCAwakeApp is a lightweight desktop utility built with **C#**, **.NET**, and **WinUI 3**.

Its purpose is simply to prevent PC from falling asleep, without being annoying for user.

## Current status

This repository currently contains the very first shell of the application:
- a simple WinUI3 window
- a toggle button for enabling or disabling keep-awake mode
- basic Windows keep-awake behavior through native OS calls
- a foundation for future UI and platform improvements

## Why I am making this app

Many keep-awake tools are either too noisy, too technical, too visually dated... and mainly because I want to :)

KeepPCAwakeApp aims to be:
- simple
- visually clean
- not annoying for user

## Planned goals

The long-term goals for KeepPCAwakeApp include:

- **System theme awareness**  
  Follow the user's current OS light/dark theme where possible.

- **Native-feeling appearance**  
  Resemble the look and feel of windows.

- **Optional timed awake mode**  
  Let the user specify how long the machine should be kept awake.

- **System tray support**  
  Allow the app to hide to the tray instead of fully exiting.

- **Platform-aware architecture**  
  Keep the code organized in a way that could support Windows first and potentially macOS later through a different UI stack.


## How to run

### Requirements
- Windows
- .NET SDK 8.0 or later
- Visual Studio 2022 or later with desktop development tools installed

### Run in Visual Studio
1. Clone the repository.
2. Open the solution file.
3. Set `KeepPCAwakeApp` as the startup project.
4. Press `F5` to run.

### Run from terminal
```bash
dotnet build
dotnet run --project KeepPCAwakeApp
