using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class Build
{
    [MenuItem("SimpleWebXR/Build/Examples")]
    public static void BuildAll()
    {
        ClearBuildFolder();

        var sceneFolder = Path.Combine(Application.dataPath, "SimpleWebXR", "Scenes");

        var scenes = Directory.GetFiles(sceneFolder, "*.unity");

        foreach (var scene in scenes)
        {
            var projectName = Path.GetFileNameWithoutExtension(scene);
            var locationPathName = Path.Combine(BuildPath, projectName);

            if (!Directory.Exists(locationPathName)) Directory.CreateDirectory(locationPathName);

            if (scene.Contains("Hololens2"))
            {
                var generationPath = Path.Combine(UWPTempPath, projectName);
                BuildPlayerOptions opts = new BuildPlayerOptions
                {
                    scenes = new string[] { scene },
                    targetGroup = BuildTargetGroup.WSA,
                    target = BuildTarget.WSAPlayer,
                    locationPathName = generationPath
                };

                BuildPipeline.BuildPlayer(opts);

                var generationFolder = new DirectoryInfo(generationPath);

                var slnPath = generationFolder.GetFiles("*.sln").FirstOrDefault()?.FullName;

                if (slnPath == null) throw new Exception($"No sln file in folder {generationFolder.FullName}");

                string pathToMSBuild;
                string error;
                using (var process = new Process())
                {
                    if (0 != process.Run(@"C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe", @"-latest -prerelease -products * -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe", Application.dataPath, out pathToMSBuild, out error))
                    {
                        throw new Exception($"Unable to locate MSBuild.exe : {error}");
                    }
                }

                if (string.IsNullOrEmpty(pathToMSBuild)) throw new Exception("Unable to access MSBuild individual component, please install it.");

                string msBuildOutput;
                using (var process = new Process())
                {
                    if (0 != process.Run(pathToMSBuild, $"/t:Build /p:Configuration=Master /p:Platform=ARM /verbosity:m \"{slnPath}\"", Application.dataPath, out msBuildOutput, out error))
                    {
                        throw new Exception("MSBuild compilation failed", new Exception(msBuildOutput));
                    }
                }

                var appxFolder = generationFolder.GetDirectories()
                     .FirstOrDefault((x) => string.Equals(x.Name, "AppPackages", StringComparison.InvariantCultureIgnoreCase)) // dossier AppPackages
                     ?.GetDirectories()?.FirstOrDefault() // dossier du nom du package 
                     ?.GetDirectories()?.FirstOrDefault(); // dossier du nom de la plateforme

                if (appxFolder == null || !appxFolder.Exists) throw new Exception($"Output folders in AppPackages don't exist for project {generationFolder.FullName}");

                var appxFile = appxFolder.GetFiles("*.appx")?.FirstOrDefault();
                if (appxFile == null) appxFile = appxFolder.GetFiles("*.msix")?.FirstOrDefault();

                var cerFile = appxFolder.GetFiles("*.cer")?.FirstOrDefault();
                var dependencieFile = appxFolder.GetDirectories("Dependencies")?.FirstOrDefault()?.GetDirectories(EditorUserBuildSettings.wsaArchitecture)?.FirstOrDefault()?.GetFiles("*.appx")?.FirstOrDefault();


                if (appxFile == null || cerFile == null || dependencieFile == null) throw new Exception($"Appx file not found for project {generationFolder.FullName}");

                var targetFolder = new DirectoryInfo(Path.Combine(BuildPath, projectName));
                if (targetFolder.Exists) targetFolder.Delete(true);
                targetFolder.Create();

                appxFile.CopyTo(Path.Combine(targetFolder.FullName, appxFile.Name));
                cerFile.CopyTo(Path.Combine(targetFolder.FullName, cerFile.Name));
                dependencieFile.CopyTo(Path.Combine(targetFolder.FullName, dependencieFile.Name));
            }
            else
            {
                BuildPlayerOptions opts = new BuildPlayerOptions
                {
                    scenes = new string[] { scene },
                    targetGroup = BuildTargetGroup.WebGL,
                    target = BuildTarget.WebGL,
                    locationPathName = locationPathName
                };

                BuildPipeline.BuildPlayer(opts);
            }
        }

        BuildUnityPackages();
    }

    public static string BuildPath => Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Builds"));

    public static string UWPTempPath => Path.GetFullPath(Path.Combine(Application.dataPath, "..", "UWPTempPath"));

    public static void ClearBuildFolder()
    {
        if (Directory.Exists(BuildPath)) Directory.Delete(BuildPath, true);
        Directory.CreateDirectory(BuildPath);
        if (Directory.Exists(UWPTempPath)) Directory.Delete(UWPTempPath, true);
        Directory.CreateDirectory(UWPTempPath);
    }

    private static int Run(this Process process, string application,
    string arguments, string workingDirectory, out string output,
    out string errors)
    {
        process.StartInfo = new ProcessStartInfo
        {
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            FileName = application,
            Arguments = arguments,
            WorkingDirectory = workingDirectory
        };

        // Use the following event to read both output and errors output.
        var outputBuilder = new StringBuilder();
        var errorsBuilder = new StringBuilder();
        process.OutputDataReceived += (_, args) => outputBuilder.AppendLine(args.Data);
        process.ErrorDataReceived += (_, args) => errorsBuilder.AppendLine(args.Data);

        // Start the process and wait for it to exit.
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();

        output = outputBuilder.ToString().TrimEnd();
        errors = errorsBuilder.ToString().TrimEnd();
        return process.ExitCode;
    }

    private static readonly string[] ScenesForStore = new string[] { "PaintExample" };

    [MenuItem("SimpleWebXR/Build/Asset store")]
    public static void BuildAssetStore()
    {
        var storeDirectory = Path.Combine(Application.dataPath, "~", "AssetStore", "SimpleWebXR");

        if (Directory.Exists(storeDirectory)) Directory.Delete(storeDirectory, true);
        Directory.CreateDirectory(storeDirectory);

        CopyDirectory(new DirectoryInfo(Path.Combine(Application.dataPath, "SimpleWebXR", "Plugins")), new DirectoryInfo(storeDirectory));

        var storeScenesDirectory = Directory.CreateDirectory(Path.Combine(storeDirectory, "Examples", "Scenes"));

        foreach (var scene in ScenesForStore)
        {
            foreach (var folder in new string[] { "Materials", "Prefabs", "Scripts" })
            {
                CopyDirectory(new DirectoryInfo(Path.Combine(Application.dataPath, "SimpleWebXR", folder, scene)), new DirectoryInfo(Path.Combine(storeDirectory, "Examples", folder)));
            }

            File.Copy(Path.Combine(Application.dataPath, "SimpleWebXR", "Scenes", scene + ".unity"), Path.Combine(storeScenesDirectory.FullName, scene + ".unity"));
            File.Copy(Path.Combine(Application.dataPath, "SimpleWebXR", "Scenes", scene + ".unity.meta"), Path.Combine(storeScenesDirectory.FullName, scene + ".unity.meta"));
        }

    }
    public static void CopyDirectory(DirectoryInfo sourceToCopy, DirectoryInfo copyIn, bool createRootDirectory = true)
    {
        if (!sourceToCopy.Exists) return;

        if (createRootDirectory) copyIn = copyIn.CreateSubdirectory(sourceToCopy.Name);
        foreach (DirectoryInfo dir in sourceToCopy.GetDirectories())
            CopyDirectory(dir, copyIn.CreateSubdirectory(dir.Name), false);
        foreach (FileInfo file in sourceToCopy.GetFiles())
            file.CopyTo(Path.Combine(copyIn.FullName, file.Name));
    }


    [MenuItem("SimpleWebXR/Build/Unity packages")]
    public static void BuildUnityPackages()
    {
        var assets = new List<String>();

        // Basics
        assets.Add("Assets/SimpleWebXR/Plugins");
        assets.Add("Assets/SimpleWebXR/Scripts/SimpleWebXR.cs");

        // Simulator
        assets.Add("Assets/SimpleWebXR/Scripts/Editor/SimpleWebXRSimulator.cs");

        // Examples
        assets.Add("Assets/SimpleWebXR/Examples/Materials/PaintExample");
        assets.Add("Assets/SimpleWebXR/Examples/Prefabs/PaintExample");
        assets.Add("Assets/SimpleWebXR/Examples/Scripts/PaintExample");
        assets.Add("Assets/SimpleWebXR/Examples/Scenes/PaintExample.unity");

        assets.Add("Assets/SimpleWebXR/Examples/Scenes/HandDetectionExample.unity");
        assets.Add("Assets/SimpleWebXR/Examples/Scripts/HandDetectionExample");

        var runtimeFrom = Path.Combine(Application.dataPath, "..", "..", "com.rufus31415.simplewebxr", "Runtime");
        var runtimeTo = Path.Combine(Application.dataPath, "SimpleWebXR", "Runtime");

        Directory.Move(runtimeFrom, runtimeTo);
        File.Move(runtimeFrom + ".meta", runtimeTo + ".meta");


        AssetDatabase.Refresh();

        assets.Add("Assets/SimpleWebXR/Runtime");

        // Export basic
        AssetDatabase.ExportPackage(assets.ToArray(), "Builds/SimpleWebXR.unitypackage", ExportPackageOptions.Recurse);

        // MRTK implementation
        assets.Add("Assets/SimpleWebXR/Scripts/MRTK");

        // Add all examples
        assets.Add("Assets/SimpleWebXR/Examples");

        // Export with MRTK
        AssetDatabase.ExportPackage(assets.ToArray(), "Builds/SimpleWebXR+MRTK.unitypackage", ExportPackageOptions.Recurse);

        Directory.Move(runtimeTo, runtimeFrom);
        File.Move(runtimeTo + ".meta", runtimeFrom + ".meta");

        AssetDatabase.Refresh();

    }
}

