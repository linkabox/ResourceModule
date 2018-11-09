using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public static class SystemExt
{
	public static int RunShell(string command, string args = "", string workDir = null)
	{
		int exitCode = -1;
		Process process = new Process();

		process.StartInfo.CreateNoWindow = false;
		process.StartInfo.UseShellExecute = true;
		process.StartInfo.FileName = command;
		process.StartInfo.Arguments = args;
		process.StartInfo.WorkingDirectory = !String.IsNullOrEmpty(workDir) ? workDir : Application.dataPath;

		try
		{
			process.Start();
			process.WaitForExit();
		}
		catch (Exception e)
		{
			Debug.LogError("Run error" + e.ToString());
			throw;
		}
		finally
		{
			exitCode = process.ExitCode;
			process.Dispose();
			process = null;
			if (exitCode == 0)
				Debug.LogFormat("RunShell:{0} {1} {2} \nExitCode:{3}", command, args, workDir, exitCode);
			else
				Debug.LogErrorFormat("RunShell:{0} {1} {2} \nExitCode:{3}", command, args, workDir, exitCode);
		}
		return exitCode;
	}
}
