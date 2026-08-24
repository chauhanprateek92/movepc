using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Move
{
    internal static class Program
    {
        private const string SingleInstanceMutexName = "f45b30b9-9e65-4d33-a2bc-d6ba6a7500bd";
        private const int MoveIntervalMilliseconds = 60000;

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
                    NudgeCursor();
                    Thread.Sleep(MoveIntervalMilliseconds);
                }
            }
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

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);
    }
}
