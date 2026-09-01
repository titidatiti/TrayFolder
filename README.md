# TrayFolder

[English](README.md) | [简体中文](README.zh-CN.md)

<img src="docs/images/icon.png" width="64" height="64" alt="TrayFolder app icon" />

TrayFolder is a Windows system tray application that brings back the convenient "folder on the taskbar" experience. It lets you quickly access a favorite folder and its contents through a modern hierarchical menu.

## Features

- **System tray integration**: Access your configured folder from a tray icon.
- **Folder navigation**: Browse files and subfolders through hierarchical menus.
- **Quick open**: Double-click a folder to open it in File Explorer.
- **Native context menus**: Right-click an item to open its Windows File Explorer context menu.
- **Single-file distribution**: The Windows x64 release is packaged as one `TrayFolder.exe` file.

## Screenshot

![TrayFolder preview](docs/images/preview.gif)

## Getting Started

1. Download the latest Windows x64 release.
2. Install the [.NET 9 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/9.0) if it is not already available on your computer.
3. Run `TrayFolder.exe`.
4. Right-click the tray icon and select the folder you want to access.

## Keep the Tray Icon Visible

Windows may place the TrayFolder icon in the tray overflow menu by default. To keep it visible:

1. Click the **Show hidden icons** arrow (`^`) in the system tray.
2. Drag the **TrayFolder** icon from the overflow menu to the main tray area.

## License

[MIT License](LICENSE)
