using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace NeoKyoto.EditorTools
{
    /// <summary>
    /// Build entry points usable from the menu or from `-batchmode -executeMethod`,
    /// so releases are reproducible instead of depending on Editor UI state.
    /// </summary>
    public static class BuildScript
    {
        private const string Scene = "Assets/Scenes/NeoKyoto.unity";

        [MenuItem("Neo-Kyoto/Build WebGL")]
        public static void BuildWebGL()
        {
            Run(BuildTarget.WebGL, "Builds/WebGL");
        }

        [MenuItem("Neo-Kyoto/Build Windows")]
        public static void BuildWindows()
        {
            Run(BuildTarget.StandaloneWindows64, "Builds/Windows/NeoKyoto.exe");
        }

        private static void Run(BuildTarget target, string outputPath)
        {
            var options = new BuildPlayerOptions
            {
                scenes = new[] { Scene },
                target = target,
                locationPathName = outputPath,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

            string message = string.Format(
                "{0} build {1} — {2:0.0} MB in {3:0}s -> {4}",
                target, summary.result, summary.totalSize / (1024f * 1024f),
                summary.totalTime.TotalSeconds, outputPath);

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log(message);
                StripDoNotShip(Path.GetDirectoryName(outputPath) == null ? outputPath : outputPath);
            }
            else
            {
                Debug.LogError(message);
                // Fail the process so batch/CI runs do not report a false success.
                if (Application.isBatchMode) EditorApplication.Exit(1);
            }
        }

        /// <summary>Removes the Burst debug folder Unity emits but tells you not to ship.</summary>
        private static void StripDoNotShip(string buildPath)
        {
            try
            {
                string root = Directory.Exists(buildPath) ? buildPath : Path.GetDirectoryName(buildPath);
                if (string.IsNullOrEmpty(root) || !Directory.Exists(root)) return;

                foreach (var dir in Directory.GetDirectories(root, "*_DoNotShip*", SearchOption.AllDirectories))
                {
                    Directory.Delete(dir, true);
                    Debug.Log("Removed " + dir);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("Could not strip DoNotShip folders: " + e.Message);
            }
        }
    }
}
