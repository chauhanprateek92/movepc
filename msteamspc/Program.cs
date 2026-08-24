using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace msteamspc
{
    internal static class Program
    {
        private const string SingleInstanceMutexName = "f45b30b9-9e65-4d33-a2bc-d6ba6a7500bd";
        private const int IdleThresholdMilliseconds = 60000;
        private const int PollIntervalMilliseconds = 1000;

        [STAThread]
        private static void Main()
        {
            bool createdNew;
            using (var mutex = new Mutex(true, SingleInstanceMutexName, out createdNew))
            {
                if (!createdNew)
                {
                    return;
                }

                while (true)
                {
                    if (GetIdleTimeMilliseconds() >= IdleThresholdMilliseconds)
                    {
                        NudgeCursor();
                    }

                    Thread.Sleep(PollIntervalMilliseconds);
                }
            }
        }

        private static int GetIdleTimeMilliseconds()
        {
            var lastInputInfo = new LASTINPUTINFO
            {
                cbSize = (uint)Marshal.SizeOf(typeof(LASTINPUTINFO))
            };

            if (!GetLastInputInfo(ref lastInputInfo))
            {
                return 0;
            }

            return Environment.TickCount - (int)lastInputInfo.dwTime;
        }

        private static void NudgeCursor()
        {
            MoveCursorBy(1, 0);
            Thread.Sleep(50);
            MoveCursorBy(-1, 0);
        }

        private static void MoveCursorBy(int dx, int dy)
        {
            var input = new INPUT
            {
                type = InputTypeMouse,
                u = new InputUnion
                {
                    mi = new MOUSEINPUT
                    {
                        dx = dx,
                        dy = dy,
                        mouseData = 0,
                        dwFlags = MouseEventFMove,
                        time = 0,
                        dwExtraInfo = IntPtr.Zero
                    }
                }
            };

            SendInput(1, new[] { input }, Marshal.SizeOf(typeof(INPUT)));
        }

        private const int InputTypeMouse = 0;
        private const uint MouseEventFMove = 0x0001;

        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public int type;
            public InputUnion u;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct InputUnion
        {
            [FieldOffset(0)] public MOUSEINPUT mi;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public int mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct LASTINPUTINFO
        {
            public uint cbSize;
            public uint dwTime;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        [DllImport("user32.dll")]
        private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);
    }
}
