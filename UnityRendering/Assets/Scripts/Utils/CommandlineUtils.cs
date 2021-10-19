
public static class CommandlineUtils
{
    public static string ExecuteCommandline(string command, bool useCommandlineWindow = false)
    {
        string commandOutput = string.Empty;
        System.Diagnostics.Process process = new System.Diagnostics.Process();
        process.StartInfo.FileName = "cmd.exe";
        process.StartInfo.Arguments = "/c " + command;

        if (!useCommandlineWindow)
        {
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.OutputDataReceived += (a, b) => commandOutput += "\n" + b.Data;
            process.ErrorDataReceived += (a, b) => commandOutput += "\n" + b.Data;
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
        }
        else
        {
            process.StartInfo.UseShellExecute = true;
            process.Start();
            process.WaitForExit();

            // Fail and Success indicators used only for command window tools as string concatenation is
            // jumbling words when adding fail and success to commandOutput after using ReadLine processes
            commandOutput += process.ExitCode < 0 ? "Fail\n" : "Success\n";
        }

        return commandOutput;
    }

    public static void KillCurrentProcess()
    {
        System.Diagnostics.Process.GetCurrentProcess().Kill();
    }
}
