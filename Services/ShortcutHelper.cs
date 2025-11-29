using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace TrayFolder.Services
{
    public static class ShortcutHelper
    {
        public static string ResolveShortcut(string shortcutPath)
        {
            if (!File.Exists(shortcutPath)) return null;

            try
            {
                Type shellType = Type.GetTypeFromProgID("WScript.Shell");
                object shell = Activator.CreateInstance(shellType);
                
                object shortcut = shellType.InvokeMember("CreateShortcut", BindingFlags.InvokeMethod, null, shell, new object[] { shortcutPath });
                
                Type shortcutType = shortcut.GetType();
                string targetPath = (string)shortcutType.InvokeMember("TargetPath", BindingFlags.GetProperty, null, shortcut, null);
                
                return targetPath;
            }
            catch
            {
                return null;
            }
        }
    }
}
