using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cocoa
{
    class ChocolateyHost : IDisposable
    {
        public string ChocoPath;
        public string ChocoBinPath;
        public string ChocoExePath;

        private static Process Chocolatey { get; set; }
        private static Thread StdInThread { get; set; }
        private static bool IsForceKeepTask { get; set; }

        public ChocolateyHost()
        {
            ChocoPath = Environment.GetEnvironmentVariables().OfType<DictionaryEntry>().Where(x => x.Key.ToString() == @"ChocolateyInstall")?.ElementAt(0).Value.ToString();
            ChocoBinPath = Path.Combine(ChocoPath, @"bin");
            ChocoExePath = Path.Combine(ChocoBinPath, @"choco.exe");

            if (!File.Exists(ChocoExePath))
            {
                Console.WriteLine(@"'choco.exe' could not be found.");
                return;
            }
        }

        public void Exec(string args = null)
        {
            var psi = new ProcessStartInfo
            {
                FileName = ChocoExePath,
                Arguments = args?.ToString(),

                CreateNoWindow = true,
                UseShellExecute = false,

                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
            };

            Chocolatey = Process.Start(psi);
            Chocolatey.WaitForExit();

            Console.Write(Chocolatey.StandardOutput.ReadToEnd());
        }

        public void ExecInteractive(string args = null)
        {
            var psi = new ProcessStartInfo
            {
                FileName = ChocoExePath,
                Arguments = args?.ToString(),

                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
            };

            Chocolatey = Process.Start(psi);
            Task.Run(() => ReadLineLoop());
            Chocolatey.EnableRaisingEvents = true;
            Chocolatey.BeginOutputReadLine();
            Chocolatey.BeginErrorReadLine();

            Chocolatey.Exited += (sender, e) => Dispose();
            Chocolatey.OutputDataReceived += (sender, e) => 
            {
                if (e.Data != null)
                {
                    Console.WriteLine(e.Data);
                }
            };
            Chocolatey.ErrorDataReceived += (sender, e) => 
            {
                if (e.Data != null)
                {
                    Console.WriteLine(e.Data);
                }
            };

            TaskKeeper();
        }

        private void TaskKeeper()
        {
            IsForceKeepTask = true;
            while (IsForceKeepTask)
            {
                Thread.Sleep(1);
            }
        }

        private void ReadLineLoop()
        {
            StdInThread = Thread.CurrentThread;

            while (true)
            {
                var stdin = Console.ReadLine();
                Chocolatey.StandardInput.WriteLine(stdin);
            }
        }

        public void InjectionStdin(string stdin)
        {
            WriteStdIn(stdin.ToString());
        }

        private void WriteStdIn(string command)
        {
            Chocolatey.StandardInput.WriteLine(command);
            Chocolatey.StandardInput.WriteLine();
        }

        public void Dispose()
        {
            StdInThread?.Abort();
            if (Chocolatey != null)
            {
                Chocolatey.Dispose();
                Chocolatey = null;
            }
            IsForceKeepTask = false;
        }
    }
}
