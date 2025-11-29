using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Drawing;

using System.Diagnostics;
using System.Windows.Input;
using TrayFolder.Helpers;
using TrayFolder.Services;

namespace TrayFolder.Models
{
    public class FileSystemItem
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public bool IsFolder { get; set; }
        public ImageSource SysIcon { get; set; }
        public ObservableCollection<FileSystemItem> Children { get; set; }
        public ICommand OpenCommand { get; set; }

        public string TargetFolderPath { get; set; }

        private bool _isExpanded;
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                _isExpanded = value;
                if (_isExpanded && IsFolder && Children.Count == 1 && Children[0].Name == "Loading...")
                {
                    LoadChildren();
                }
            }
        }

        public FileSystemItem()
        {
            Children = new ObservableCollection<FileSystemItem>();
        }

        public FileSystemItem(string path)
        {
            Path = path;
            Name = System.IO.Path.GetFileName(path);
            if (string.IsNullOrEmpty(Name)) Name = path; // Handle root drives
            
            // Check if it's a shortcut to a folder
            if (System.IO.Path.GetExtension(path).ToLower() == ".lnk")
            {
                string target = ShortcutHelper.ResolveShortcut(path);
                if (!string.IsNullOrEmpty(target) && Directory.Exists(target))
                {
                    TargetFolderPath = target;
                    IsFolder = true;
                    Name = System.IO.Path.GetFileNameWithoutExtension(path);
                }
                else
                {
                    IsFolder = false;
                }
            }
            else
            {
                IsFolder = Directory.Exists(path);
            }
            
            try 
            {
                SysIcon = IconHelper.GetIcon(path, IsFolder);
            }
            catch { }

            Children = new ObservableCollection<FileSystemItem>();
            OpenCommand = new RelayCommand(ExecuteOpen);

            if (IsFolder)
            {
                // Add a dummy item to enable expansion if it's a folder
                Children.Add(new FileSystemItem { Name = "Loading..." });
            }
        }

        private void ExecuteOpen(object obj)
        {
            if (IsFolder) return; // Folders expand, files open
            try
            {
                Process.Start(new ProcessStartInfo(Path) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                // Show error?
                Debug.WriteLine($"Error opening file: {ex.Message}");
            }
        }

        public void LoadChildren()
        {
            if (!IsFolder) return;

            try
            {
                Children.Clear();
                string path = !string.IsNullOrEmpty(TargetFolderPath) ? TargetFolderPath : Path;
                var dirInfo = new DirectoryInfo(path);
                
                foreach (var directory in dirInfo.GetDirectories())
                {
                    Children.Add(new FileSystemItem(directory.FullName));
                }

                foreach (var file in dirInfo.GetFiles())
                {
                    Children.Add(new FileSystemItem(file.FullName));
                }
            }
            catch (Exception)
            {
                // Handle access exceptions or others
                Children.Add(new FileSystemItem { Name = "Access Denied" });
            }
        }
    }
}
