#if UNITY_STANDALONE_WIN

using System.Diagnostics;
using UnityEngine;
using System;
using System.Text.RegularExpressions;
using System.Linq;

class Firewall {
    private static bool RunAsAdministrator(string app, string args = ""){
        ProcessStartInfo processStartInfo = new ProcessStartInfo(app);
        processStartInfo.Arguments = args;
        processStartInfo.Verb = "runas";
        try {
            Process.Start(processStartInfo).WaitForExit();
            return true;
        }
        catch(Exception){
            return false;
        }
    }

    private static string GetProcessOutput(string app, string args = ""){
        ProcessStartInfo processStartInfo = new ProcessStartInfo(app);
        processStartInfo.Arguments = args;
        processStartInfo.UseShellExecute = false;
        processStartInfo.CreateNoWindow = true;
        processStartInfo.RedirectStandardOutput = true;
        Process process = Process.Start(processStartInfo);
        string stdout = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        return stdout;
    }

    private static string RuleName {
        get {
            return System.Environment.GetCommandLineArgs()[0].Split('\\').Last().ToLower();
        }
    }
    private static string Program {
        get {
            return System.Environment.GetCommandLineArgs()[0];
        }
    }

    public static bool UpdateAppRule(){
        string arg = string.Format(
            "advfirewall firewall set rule name=\"{0}\" new action=allow profile=\"Domain,Public,Private\"",
            RuleName
        );
        return RunAsAdministrator("netsh", arg);
    }

    public static bool DoesAppRestrictionExist(){
        string firewallRules = GetAppRules();
        return firewallRules.Split('\n').Any(line => Regex.IsMatch(line, @"(?:Action:\s*Block)")) ||
            firewallRules.Split('\n').Any(
                line => line.StartsWith("Profiles:") && !line.Contains("Domain,Private,Public")
            );
    }

    private static string GetAppRules(){
        string arg = string.Format("advfirewall firewall show rule name=\"{0}\"", RuleName);
        return GetProcessOutput("netsh", arg);
    }
}
#endif // UNITY_STANDALONE_WIN