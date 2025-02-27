using System.Diagnostics;
using System.Net;
using System.Reflection.PortableExecutable;
using System.Text.Json;
using LegacyLauncher.Models;
using LegacyLauncher.Utilities;

namespace LegacyLauncher {
    public class Program {
        static void Main(string[] args) {
            Console.Title = "LegacyFN Launcher";

            string WorkingDirectory = Directory.GetCurrentDirectory();
            
            string CobaltPath = Path.Join(WorkingDirectory, "Cobalt.dll");
            string CobaltLocalPath = Path.Join(WorkingDirectory, "CobaltLocal.dll");
            string RebootPath = Path.Join(WorkingDirectory, "Reboot.dll");

            // In reality I shouldn't use WebClient
            if (!File.Exists(CobaltPath)) {
                Console.WriteLine("Downloading Cobalt DLL...");
                using (WebClient client = new WebClient())
                    client.DownloadFile("https://dl.dropboxusercontent.com/scl/fi/3enxve9ansk0yehf2z799/Cobalt.dll?rlkey=z1geqcjlf4ml5671jbukoqp7g&st=pjtofo40&dl=0", CobaltPath);
            }

            if (!File.Exists(CobaltLocalPath)) {
                Console.WriteLine("Downloading CobaltLocal DLL...");
                using (WebClient client = new WebClient())
                    client.DownloadFile("https://dl.dropboxusercontent.com/scl/fi/4l1s9t9eo014kgc9vrvsf/CobaltLocal.dll?rlkey=ain4yzn4blfb4r34cr1i7wf01&st=xg6myikt&dl=0", CobaltLocalPath);
            }

            if (!File.Exists(RebootPath)) {
                Console.WriteLine("Downloading Reboot DLL...");
                using (WebClient client = new WebClient())
                    client.DownloadFile("https://github.com/ProjectLegacyFN/LegacyReboot/releases/download/1.0/Reboot.dll", RebootPath);
            }

            ArgParser Arguments = new ArgParser(args);

            string ConfigPath = Arguments.ConfigPath != null ? Arguments.ConfigPath : Path.Join(WorkingDirectory, "config.json");
            Config config;
            if (File.Exists(ConfigPath)) {
                string jText = File.ReadAllText(ConfigPath);
                config = JsonSerializer.Deserialize<Config>(jText);
            } else {

                string? _Username = null;
                while (_Username == null) {
                    Console.Write("Enter E-mail: ");
                    _Username = Console.ReadLine();
                }

                string? _Password = null;
                while (_Password == null) {
                    Console.Write("Enter Password: ");
                    _Password = Console.ReadLine();
                }

                string _FortnitePath;
                if (Directory.Exists(Path.Join(WorkingDirectory, "FortniteGame")) && Directory.Exists(Path.Join(WorkingDirectory, "Engine")))
                    _FortnitePath = WorkingDirectory;
                else {
                    string? path = null;
                    while (path == null) {
                        Console.Write("Enter Fortnite Path: ");
                        path = Console.ReadLine();
                    }

                    _FortnitePath = path;
                }

                config = new Config(_Username, _Password, _FortnitePath);
                string jsonString = JsonSerializer.Serialize(config);
                File.WriteAllText(ConfigPath, jsonString);
            }
            
            string FortnitePath = Arguments.FortnitePath != null ? Arguments.FortnitePath : config.FortnitePath;
            string BinariesPath = Path.Join(FortnitePath, "FortniteGame\\Binaries\\Win64");
            string ShippingPath = Path.Join(BinariesPath, "FortniteClient-Win64-Shipping.exe");
            string FNLauncherPath = Path.Join(BinariesPath, "FortniteLauncher.exe");
            string EACShippingPath = Path.Join(BinariesPath, "FortniteClient-Win64-Shipping_EAC.exe");

            if (File.Exists(BinariesPath)) {
                Console.WriteLine("Could not find Fortnite path, do you have the right path selected?");
                Thread.Sleep(3000);
                return;
            }

            // Putting together launch args for Fortnite
            string Email = Arguments.Email != null ? Arguments.Email : config.Email;
            string Password = Arguments.Password != null ? Arguments.Password : config.Password;

            string launchArgs = $"-epicapp=Fortnite -epicenv=Prod -epiclocale=en-us -epicportal -skippatchcheck -NOSSLPINNING -nobe -fromfl=eac -fltoken=7a848a93a74ba68876c36C1c -AUTH_LOGIN={Email} -AUTH_PASSWORD={Password} -AUTH_TYPE=epic";
            Process? FNLauncherProcess  = null;
            Process? EACShippingProcess = null;
            if (File.Exists(FNLauncherPath)) {
                FNLauncherProcess = new Process {
                    StartInfo = {
                        FileName = FNLauncherPath,
                        UseShellExecute = false,
                        Arguments = launchArgs
                    }
                };
                FNLauncherProcess.Start();
                foreach (ProcessThread thread in FNLauncherProcess.Threads) {
                    Win32.SuspendThread(Win32.OpenThread(0x0002, false, thread.Id));
                }
            }

            if (File.Exists(EACShippingPath)) {
                EACShippingProcess = new Process {
                    StartInfo = {
                        FileName = EACShippingPath,
                        UseShellExecute = false,
                        Arguments = launchArgs
                    }
                };
                EACShippingProcess.Start();
                foreach (ProcessThread thread in EACShippingProcess.Threads) {
                    Win32.SuspendThread(Win32.OpenThread(0x0002, false, thread.Id));
                }
            }

            Process ShippingProcess = new Process {
                StartInfo = {
                    FileName = ShippingPath,
                    UseShellExecute = false,
                    Arguments = launchArgs
                }
            };
            ShippingProcess.Start();

            // Inject DLLs
            if (!Arguments.bUseLocalHostServer)
                Injector.Inject(CobaltPath, ShippingProcess.Id);
            else
                Injector.Inject(CobaltLocalPath, ShippingProcess.Id);

            if (Arguments.bRunAsGameServer) {
                new Thread(delegate() {
                    Console.Write("Press ENTER to inject Reboot!");
                    Console.ReadLine();
                    Injector.Inject(RebootPath, ShippingProcess.Id);
                }).Start();
            }

            ShippingProcess.WaitForExit();
            if (FNLauncherProcess != null) FNLauncherProcess.Kill();
            if (EACShippingProcess != null) EACShippingProcess.Kill();
        }
    }
}