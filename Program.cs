using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace LyXel
{
    internal static class Program
    {
        // P/Invoke para el Job Object de Windows
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr CreateJobObject(IntPtr lpJobAttributes, string? lpName);

        [DllImport("kernel32.dll")]
        private static extern bool SetInformationJobObject(IntPtr hJob, int JobObjectInfoClass,
            ref JOBOBJECT_EXTENDED_LIMIT_INFORMATION lpJobObjectInfo, int cbJobObjectInfoLength);

        [DllImport("kernel32.dll")]
        private static extern bool AssignProcessToJobObject(IntPtr hJob, IntPtr hProcess);

        [StructLayout(LayoutKind.Sequential)]
        private struct JOBOBJECT_BASIC_LIMIT_INFORMATION
        {
            public long PerProcessUserTimeLimit;
            public long PerJobUserTimeLimit;
            public uint LimitFlags;
            public UIntPtr MinimumWorkingSetSize;
            public UIntPtr MaximumWorkingSetSize;
            public uint ActiveProcessLimit;
            public UIntPtr Affinity;
            public uint PriorityClass;
            public uint SchedulingClass;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct IO_COUNTERS
        {
            public ulong ReadOperationCount;
            public ulong WriteOperationCount;
            public ulong OtherOperationCount;
            public ulong ReadTransferCount;
            public ulong WriteTransferCount;
            public ulong OtherTransferCount;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct JOBOBJECT_EXTENDED_LIMIT_INFORMATION
        {
            public JOBOBJECT_BASIC_LIMIT_INFORMATION BasicLimitInformation;
            public IO_COUNTERS IoInfo;
            public UIntPtr ProcessMemoryLimit;
            public UIntPtr JobMemoryLimit;
            public UIntPtr PeakProcessMemoryUsed;
            public UIntPtr PeakJobMemoryUsed;
        }

        private const uint JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE = 0x2000;
        private const int  JobObjectExtendedLimitInformation   = 9;

        private static IntPtr _hJob = IntPtr.Zero;

        /// <summary>Asigna un proceso hijo al Job Object para que muera junto con la app.</summary>
        internal static void AsignarAlJob(IntPtr hProcess)
        {
            if (_hJob != IntPtr.Zero)
                AssignProcessToJobObject(_hJob, hProcess);
        }

        // Entry point
        [STAThread]
        static void Main()
        {
            // El Job Object hace que si la app se cierra de golpe, scrcpy y adb también mueren
            _hJob = CreateJobObject(IntPtr.Zero, null);
            if (_hJob != IntPtr.Zero)
            {
                var info = new JOBOBJECT_EXTENDED_LIMIT_INFORMATION();
                info.BasicLimitInformation.LimitFlags = JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE;
                SetInformationJobObject(_hJob, JobObjectExtendedLimitInformation,
                    ref info, Marshal.SizeOf<JOBOBJECT_EXTENDED_LIMIT_INFORMATION>());

                AssignProcessToJobObject(_hJob, System.Diagnostics.Process.GetCurrentProcess().Handle);
            }

            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                try
                {
                    // Intentar reset de DPI y resolución en crash
                    // Ejecutar síncronamente con timeout de 3 segundos
                    var adbPath = Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        "bin", "adb", "adb.exe");

                    if (File.Exists(adbPath))
                    {
                        // Reset DPI
                        var pDpi = new Process
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                FileName = adbPath,
                                Arguments = "shell wm density reset",
                                UseShellExecute = false,
                                CreateNoWindow = true
                            }
                        };
                        pDpi.Start();
                        pDpi.WaitForExit(3000);

                        // Reset resolución
                        var pRes = new Process
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                FileName = adbPath,
                                Arguments = "shell wm size reset",
                                UseShellExecute = false,
                                CreateNoWindow = true
                            }
                        };
                        pRes.Start();
                        pRes.WaitForExit(3000);
                    }
                }
                catch { }
            };

            Application.SetUnhandledExceptionMode(
                UnhandledExceptionMode.CatchException);

            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
        }
    }
}
