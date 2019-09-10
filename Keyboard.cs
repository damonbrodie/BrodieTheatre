using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;


namespace BrodieTheatre
{
    public partial class FormMain : Form
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        public static LowLevelKeyboardProc proc = HookCallback;
        private static IntPtr hookID = IntPtr.Zero;
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        public static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        public static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                KeysConverter kc = new KeysConverter();
                string keyCode = kc.ConvertToString(vkCode);
                switch (keyCode)
                {
                    case "F12":
                        formMain.BeginInvoke(new Action(() =>
                        {
                            Logging.writeLog("Keyboard:  Caught keypress 'F12'");
                            formMain.lightsToEnteringLevel();
                        }));
                        break;
                    case "F11":
                        formMain.BeginInvoke(new Action(() =>
                        {
                            Logging.writeLog("Keyboard:  Caught keypress 'F11'");
                            formMain.lightsOff();
                        }));
                        break;
                    case "F9":
                        formMain.BeginInvoke(new Action(() =>
                        {
                            Logging.writeLog("Keyboard:  Caught keypress 'F9'");
                            formMain.lightsToStoppedLevel();
                        }));
                        break;
                    case "F7":
                        formMain.BeginInvoke(new Action(() =>
                        {
                            Logging.writeLog("Keyboard:  Caught keypress 'F7'");
                            formMain.lightsToPlaybackLevel();
                        }));
                        break;
                    case "F5":
                        formMain.BeginInvoke(new Action(() =>
                        {
                            Logging.writeLog("Keyboard:  Caught keypress 'F5'");
                            formMain.projectorQueueChangeAspect((float)1.85);
                        }));
                        break;
                    case "F4":
                        formMain.BeginInvoke(new Action(() =>
                        {
                            Logging.writeLog("Keyboard:  Caught keypress 'F4'");
                            formMain.projectorQueueChangeAspect((float)2.0);
                        }));
                        break;
                 }
            }
            return CallNextHookEx(hookID, nCode, wParam, lParam);
        }
    }
}