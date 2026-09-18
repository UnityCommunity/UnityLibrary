// builds single standalone exe from your Unity build (using 7-Zip SFX module)
// Note: Change those hardcoded 7zip paths for you
// Note: Download SDK package for the 7zSD.sfx files https://www.7-zip.org/download.html

using UnityEditor;
using UnityEditor.Callbacks;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace UnityLibrary.BuildTools
{
    public static class SingleExePostProcessor
    {
        private const string SevenZipExe = @"D:\Program Files\7-Zip\7z.exe";
        private const string SevenZipSfx = @"D:\sdk\lzma2603\bin\7zSD.sfx";

        [PostProcessBuild(1000)]
        public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
        {
            if (target != BuildTarget.StandaloneWindows64)
                return;

            string buildExe = Path.GetFullPath(pathToBuiltProject);
            string buildDirectory = Path.GetDirectoryName(buildExe);
            string exeName = Path.GetFileName(buildExe);
            string gameName = Path.GetFileNameWithoutExtension(buildExe);

            if (buildDirectory == null)
                return;

            if (!File.Exists(SevenZipExe))
            {
                UnityEngine.Debug.LogError("7-Zip not found: " + SevenZipExe);
                return;
            }

            if (!File.Exists(SevenZipSfx))
            {
                UnityEngine.Debug.LogError("7-Zip SFX module not found: " + SevenZipSfx);
                return;
            }

            string tempDirectory = Path.Combine(Path.GetTempPath(), "UnitySingleExe_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDirectory);

            string archivePath = Path.Combine(tempDirectory, "game.7z");
            string configPath = Path.Combine(tempDirectory, "config.txt");
            string outputExe = Path.Combine(Directory.GetParent(buildDirectory)?.FullName ?? buildDirectory, gameName + "_Standalone.exe");

            try
            {
                CreateArchive(buildDirectory, archivePath);

                string config = ";!@Install@!UTF-8!\r\n" + "Title=\"" + gameName + "\"\r\n" + "RunProgram=\"" + exeName + "\"\r\n" + "GUIMode=\"2\"\r\n" + ";!@InstallEnd@!\r\n";

                File.WriteAllText(configPath, config, new UTF8Encoding(false));

                using (FileStream output = File.Create(outputExe))
                {
                    AppendFile(output, SevenZipSfx);
                    AppendFile(output, configPath);
                    AppendFile(output, archivePath);
                }

                UnityEngine.Debug.Log("Single EXE created: " + outputExe);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }
            finally
            {
                try
                {
                    Directory.Delete(tempDirectory, true);
                }
                catch
                {
                }
            }
        }

        private static void CreateArchive(string buildDirectory, string archivePath)
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = SevenZipExe,
                WorkingDirectory = buildDirectory,
                Arguments = "a -t7z -mx=5 \"" + archivePath + "\" \"*\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using Process process = Process.Start(psi);

            if (process == null)
                throw new Exception("Could not start 7-Zip.");

            string stdout = process.StandardOutput.ReadToEnd();
            string stderr = process.StandardError.ReadToEnd();

            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new Exception("7-Zip failed.\n" + stdout + "\n" + stderr);
            }
        }

        private static void AppendFile(Stream destination, string file)
        {
            using FileStream source = File.OpenRead(file);
            source.CopyTo(destination);
        }
    }
}
