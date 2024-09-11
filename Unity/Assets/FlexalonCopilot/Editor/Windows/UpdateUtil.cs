#if UNITY_TMPRO && UNITY_UI

using UnityEditor;
using UnityEngine;
using System.IO;

namespace FlexalonCopilot.Editor
{
    internal static class UpdateUtil
    {
        public static async void UpdateFromUrl(string url)
        {
            var tmpFile = Path.GetTempFileName();

            bool downloadSuccess = false;
            try
            {
                await Api.Instance.DownloadFileAsync(url, tmpFile);
                downloadSuccess = true;
            }
            catch (System.Exception e)
            {
                Log.Error($"Failed to download update: {e.Message}");
            }

            if (downloadSuccess)
            {
                // Avoid re-compiling until we're done
                EditorApplication.LockReloadAssemblies();

                // Check if meta files are using LF or CRLF
                var packageJsonPath = AssetDatabase.GUIDToAssetPath(WindowUtil.PackageJsonGuid);
                var packageJsonPathMeta = packageJsonPath + ".meta";
                bool crlf = File.ReadAllText(packageJsonPathMeta).Contains("\r\n");

                // Delete the old package
                var packagePath = Path.GetDirectoryName(packageJsonPath);
                Directory.Delete(packagePath, true);

                // Import the package to the Assets folder
                AssetDatabase.ImportPackage(tmpFile, false);

                var newPackagePath = Path.GetDirectoryName(AssetDatabase.GUIDToAssetPath(WindowUtil.PackageJsonGuid));

                // If using CRLF for meta files, replace all LF with CRLF
                if (crlf)
                {
                    var files = Directory.GetFiles(newPackagePath, "*.meta", SearchOption.AllDirectories);
                    foreach (var file in files)
                    {
                        var text = File.ReadAllText(file);
                        text = text.Replace("\n", "\r\n");
                        File.WriteAllText(file, text);
                    }
                }

                // Move assets to the new location
                if (newPackagePath != packagePath)
                {
                    Directory.Move(newPackagePath, packagePath);
                }

                // Re-compile
                EditorApplication.UnlockReloadAssemblies();
            }

            File.Delete(tmpFile);
        }
    }
}

#endif