# TrayFolder

[English](README.md) | [简体中文](README.zh-CN.md)

<img src="docs/images/icon.png" width="64" height="64" alt="TrayFolder 应用图标" />

TrayFolder 是一款 Windows 系统托盘应用，用来还原便捷的“将文件夹放到任务栏”体验。你可以通过现代化的多级菜单，快速访问常用文件夹及其中的内容。

## 功能特性

- **系统托盘集成**：通过托盘图标访问已配置的文件夹。
- **多级目录浏览**：使用层级菜单浏览文件和子文件夹。
- **快速打开**：双击文件夹即可在文件资源管理器中打开。
- **原生右键菜单**：右键单击条目，即可打开与 Windows 文件资源管理器一致的上下文菜单。
- **单文件发布**：Windows x64 版本打包为单个 `TrayFolder.exe` 文件。

## 软件截图

![TrayFolder 预览](docs/images/preview.gif)

## 快速开始

1. 下载最新的 Windows x64 版本。
2. 如果电脑尚未安装运行环境，请安装 [.NET 9 Desktop Runtime](https://dotnet.microsoft.com/zh-cn/download/dotnet/9.0)。
3. 运行 `TrayFolder.exe`。
4. 右键单击托盘图标，选择需要快速访问的文件夹。

## 让托盘图标始终可见

Windows 默认可能会将 TrayFolder 图标收进托盘的折叠菜单。若要让它始终显示：

1. 单击系统托盘中的**显示隐藏的图标**箭头（`^`）。
2. 将 **TrayFolder** 图标从折叠菜单拖到托盘主区域。

## 开源许可

[MIT License](LICENSE)
