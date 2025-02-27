using System.Runtime.InteropServices.Marshalling;
using LegacyLauncher.Models;

namespace LegacyLauncher.Utilities {
    public class ArgParser {
        public bool bUseLocalHostServer = false;
        public bool bRunAsGameServer = false;
        public string? ConfigPath = null;

        public string? Email = null;
        public string? Password = null;
        public string? FortnitePath = null;

        public ArgParser(string[] args) {
            bUseLocalHostServer = args.Contains("--localhost");
            bRunAsGameServer = args.Contains("--gameserver");

            for (int i = 0; i < args.Length; i++) {
                string arg = args[i];

                if (arg == "--config" && args.Length > i)
                    ConfigPath = args[i + 1];

                if (arg == "--email" && args.Length > i)
                    Email = args[i + 1];

                if (arg == "--password" && args.Length > i)
                    Password = args[i + 1];

                if (arg == "--fortnite-path" && args.Length > i)
                    FortnitePath = args[i + 1];
            }
        }
    }
}