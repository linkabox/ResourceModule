#if UNITY_IOS

using System;
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;
using UnityEditor.iOS.Xcode;
using UnityEngine;
using System.Diagnostics;

public class iOSBuildHelper
{

    public static void ExecuteCommand(string command)
    {
        var escapedArgs = command.Replace("\"", "\\\"");

        Process proc = new System.Diagnostics.Process();
        proc.StartInfo.FileName = "/bin/bash";
        proc.StartInfo.Arguments = "-c \"" + escapedArgs + "\"";
        proc.StartInfo.UseShellExecute = false;
        proc.StartInfo.RedirectStandardOutput = true;
        proc.StartInfo.RedirectStandardError = true;
        proc.Start();

        var err = proc.StandardError.ReadLine();
        if (!string.IsNullOrEmpty(err))
            UnityEngine.Debug.LogError(err);

        var output = proc.StandardOutput.ReadLine();
        if (!string.IsNullOrEmpty(output))
            UnityEngine.Debug.LogWarning(output);
        proc.WaitForExit();
    }

    [PostProcessBuild]
    public static void OnPostProcessBuild(BuildTarget buildTarget, string pathToBuiltProject)
    {

        // merge plist
        var plistPath = Path.Combine(pathToBuiltProject, "Info.plist");
        var onePath = Path.Combine(Application.dataPath, "Plugins/iOS/vendors/one/Info.plist");
        ExecuteCommand(string.Format("/usr/libexec/PlistBuddy -x -c 'Merge {0}' {1}", onePath, plistPath));

        // write sdkconfig
        var sdkPath = Path.Combine(pathToBuiltProject, "ejoysdk");
        var jsonPath = Path.Combine(sdkPath, "sdkconfig.json");
        ExecuteCommand("mkdir -p " + sdkPath);
        string json = @"{""sdks"":[{""class"":""SDKProxyOne""}],""meta"":{}, ""tag"": ""cocoisland""}";
        var sr = File.CreateText(jsonPath);
        sr.WriteLine(json);
        sr.Close();

        //xcode setting
        string projectPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
        PBXProject pbxProject = new PBXProject();
        pbxProject.ReadFromString(File.ReadAllText(projectPath));
        string target = pbxProject.TargetGuidByName("Unity-iPhone");

        var guid = pbxProject.AddFolderReference("ejoysdk", "ejoysdk");
        pbxProject.AddFileToBuild(target, guid);

        pbxProject.SetBuildProperty(target, "ENABLE_BITCODE", "NO");
        pbxProject.AddBuildProperty(target, "OTHER_LDFLAGS", "-ObjC");
        pbxProject.AddBuildProperty(target, "OTHER_LDFLAGS", "-lz");
        pbxProject.AddFrameworkToProject(target, "WebKit.framework", false);

        File.WriteAllText(projectPath, pbxProject.WriteToString());
    }
}

#endif
