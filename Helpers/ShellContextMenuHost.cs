using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace TrayFolder.Helpers
{
    internal static class ShellContextMenuHost
    {
        private const string HostResourceName = "TrayFolder.Embedded.TrayFolder.ShellHost.exe";
        private const string HostConfigResourceName = "TrayFolder.Embedded.TrayFolder.ShellHost.exe.config";
        private static readonly object ExtractionLock = new object();
        private static string? _extractedHostPath;

        public static async Task ShowAsync(string path, int x, int y)
        {
            string hostPath = EnsureHostExtracted();
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = hostPath,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            startInfo.ArgumentList.Add(path);
            startInfo.ArgumentList.Add(x.ToString(CultureInfo.InvariantCulture));
            startInfo.ArgumentList.Add(y.ToString(CultureInfo.InvariantCulture));

            using Process? hostProcess = Process.Start(startInfo);
            if (hostProcess != null)
            {
                await hostProcess.WaitForExitAsync();
            }
        }

        private static string EnsureHostExtracted()
        {
            lock (ExtractionLock)
            {
                if (_extractedHostPath != null && File.Exists(_extractedHostPath))
                {
                    return _extractedHostPath;
                }

                byte[] hostBytes = ReadEmbeddedResource(HostResourceName);
                byte[] configBytes = ReadEmbeddedResource(HostConfigResourceName);
                byte[] combinedBytes = new byte[hostBytes.Length + configBytes.Length];
                Buffer.BlockCopy(hostBytes, 0, combinedBytes, 0, hostBytes.Length);
                Buffer.BlockCopy(configBytes, 0, combinedBytes, hostBytes.Length, configBytes.Length);

                string contentHash = Convert.ToHexString(SHA256.HashData(combinedBytes)).Substring(0, 16);
                string hostDirectory = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "TrayFolder",
                    "ShellHost",
                    contentHash);

                Directory.CreateDirectory(hostDirectory);
                string hostPath = Path.Combine(hostDirectory, "TrayFolder.ShellHost.exe");
                string configPath = hostPath + ".config";
                WriteEmbeddedFile(hostPath, hostBytes);
                WriteEmbeddedFile(configPath, configBytes);

                _extractedHostPath = hostPath;
                return hostPath;
            }
        }

        private static byte[] ReadEmbeddedResource(string resourceName)
        {
            Assembly assembly = typeof(ShellContextMenuHost).Assembly;
            using Stream resourceStream = assembly.GetManifestResourceStream(resourceName)
                ?? throw new InvalidOperationException($"Missing embedded resource: {resourceName}");
            using MemoryStream buffer = new MemoryStream();
            resourceStream.CopyTo(buffer);
            return buffer.ToArray();
        }

        private static void WriteEmbeddedFile(string destinationPath, byte[] content)
        {
            if (File.Exists(destinationPath) && File.ReadAllBytes(destinationPath).SequenceEqual(content))
            {
                return;
            }

            string temporaryPath = destinationPath + "." + Environment.ProcessId + ".tmp";
            File.WriteAllBytes(temporaryPath, content);

            try
            {
                File.Move(temporaryPath, destinationPath, true);
            }
            catch (IOException) when (File.Exists(destinationPath) && File.ReadAllBytes(destinationPath).SequenceEqual(content))
            {
                File.Delete(temporaryPath);
            }
        }
    }
}
