using System.Diagnostics;
using System.Reflection.PortableExecutable;
using System.Text.Json;
using LegacyLauncher.Models;
using LegacyLauncher.Utilities;

namespace LegacyLauncher {
    public class Program {
        static void Main(string[] args) {
            Console.Title = "LegacyFN Launcher";
            bool bUseOwnServer = args.Contains("--localserver");

            string WorkingDirectory = Directory.GetCurrentDirectory();

            string CobaltPath = Path.Join(WorkingDirectory, "Cobalt.dll");
            string CobaltLocalPath = Path.Join(WorkingDirectory, "CobaltLocal.dll");

            if (!File.Exists(CobaltPath)) {
                Console.WriteLine("Downloading Cobalt.dll...");
                //TODO: Install Cobalt
            }

            if (!File.Exists(CobaltLocalPath)) {
                Console.WriteLine("Downloading CobaltLocal.dll...");
                //TODO: Install CobaltLocal
            }

            string ConfigPath = Path.Join(WorkingDirectory, "config.json");
            Config config = new Config();
            if (File.Exists(ConfigPath)) {
                string jsonString = File.ReadAllText(ConfigPath);
                config = JsonSerializer.Deserialize<Config>(jsonString);
            } else {
                string? username = null;
                while (username == null) {
                    Console.Write("Enter E-mail: ");
                    username = Console.ReadLine();
                }

                string? password = null;
                while (password == null) {
                    Console.Write("Enter Password: ");
                    password = Console.ReadLine();
                }

                config.Email = username;
                config.Password = password;

                string jsonString = JsonSerializer.Serialize(config);
                File.WriteAllText(ConfigPath, jsonString);
            }

            string launchArgs = $"-epicapp=Fortnite -epicenv=Prod -epiclocale=en-us -epicportal -skippatchcheck -NOSSLPINNING -nobe -fromfl=eac -fltoken=7a848a93a74ba68876c36C1c -AUTH_LOGIN={config.Email} -AUTH_PASSWORD={config.Password} -AUTH_TYPE=epic";

            string BinariesPath = Path.Join(WorkingDirectory, "FortniteGame\\Binaries\\Win64");
            string ShippingPath = Path.Join(BinariesPath, "FortniteClient-Win64-Shipping.exe");
            string FNLauncherPath = Path.Join(BinariesPath, "FortniteLauncher.exe");
            string EACShippingPath = Path.Join(BinariesPath, "FortniteClient-Win64-Shipping_EAC.exe");

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

            ShippingProcess.WaitForExit();
            if (FNLauncherProcess != null) FNLauncherProcess.Kill();
            if (EACShippingProcess != null) EACShippingProcess.Kill();
        }
    }
}