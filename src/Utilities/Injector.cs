using System.Text;

namespace LegacyLauncher.Utilities {
    public static class Injector {
        public static void Inject(string dllPath, int processId) {
            IntPtr hProcess = Win32.OpenProcess(Win32.PROCESS_CREATE_THREAD | Win32.PROCESS_QUERY_INFORMATION | Win32.PROCESS_VM_OPERATION | Win32.PROCESS_VM_WRITE | Win32.PROCESS_VM_READ, false, processId);

            if (hProcess == IntPtr.Zero)
            {
                Console.WriteLine("Failed to open process.");
                return;
            }

            IntPtr addr = Win32.VirtualAllocEx(hProcess, IntPtr.Zero, (uint)(dllPath.Length + 1), Win32.MEM_COMMIT | Win32.MEM_RESERVE, Win32.PAGE_READWRITE);

            if (addr == IntPtr.Zero)
            {
                Console.WriteLine("Failed to allocate memory.");
                Win32.CloseHandle(hProcess);
                return;
            }

            byte[] bytes = Encoding.ASCII.GetBytes(dllPath);

            if (!Win32.WriteProcessMemory(hProcess, addr, bytes, (uint)bytes.Length, out UIntPtr bytesWritten))
            {
                Console.WriteLine("Failed to write memory.");
                Win32.CloseHandle(hProcess);
                return;
            }

            IntPtr loadLibraryAddr = Win32.GetProcAddress(Win32.GetModuleHandle("kernel32.dll"), "LoadLibraryA");

            if (loadLibraryAddr == IntPtr.Zero)
            {
                Console.WriteLine("Failed to get LoadLibraryA address.");
                Win32.CloseHandle(hProcess);
                return;
            }

            IntPtr hThread = Win32.CreateRemoteThread(hProcess, IntPtr.Zero, 0, loadLibraryAddr, addr, 0, IntPtr.Zero);

            if (hThread == IntPtr.Zero)
            {
                Console.WriteLine("Failed to create remote thread.");
                Win32.CloseHandle(hProcess);
                return;
            }

            Win32.CloseHandle(hThread);
            Win32.CloseHandle(hProcess);
        }
    }
}