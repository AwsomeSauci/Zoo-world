using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace ZooWorld.Editor
{
    public static class ZooBuild
    {
        public static void Web()
        {
            var directory = "Builds/Web";
            var arguments = Environment.GetCommandLineArgs();
            for (var i = 0; i < arguments.Length; i++)
            {
                if (arguments[i] != "--zoo-build-dir")
                    continue;
                if (i + 1 >= arguments.Length || string.IsNullOrWhiteSpace(arguments[i + 1]))
                    throw new ArgumentException("--zoo-build-dir requires an output directory.");
                directory = arguments[++i];
            }

            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.decompressionFallback = true;
            PlayerSettings.WebGL.threadsSupport = false;
            PlayerSettings.WebGL.template = "APPLICATION:Default";
            Directory.CreateDirectory(directory);

            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { "Assets/Scenes/Zoo.unity" },
                locationPathName = directory,
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            });
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException(
                    $"Web build failed: {report.summary.result}, {report.summary.totalErrors} errors.");
        }
    }
}
