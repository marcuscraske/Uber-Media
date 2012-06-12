/*
 * UBERMEAT FOSS
 * ****************************************************************************************
 * License:                 Creative Commons Attribution-ShareAlike 3.0 unported
 *                          http://creativecommons.org/licenses/by-sa/3.0/
 * 
 * Project:                 Uber Media
 * File:                    /Shared/Windows.cs
 * Author(s):               limpygnome						limpygnome@gmail.com
 *                          abatishchev                     http://stackoverflow.com/questions/102567/how-to-shutdown-the-computer-from-c-sharp
 *                          Stephen Wrighton                http://stackoverflow.com/questions/102567/how-to-shutdown-the-computer-from-c-sharp
 * To-do/bugs:              none
 * 
 * A class of useful static methods to control the application and Windows; this was created
 * because the "shutdown" process is not always reliable (due to permission issues)
 * and because Application.Restart() and Application.Exit() in Systems.Windows.Forms
 * are unreliable.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows.Forms;

namespace UberMediaServer
{
    public static class Windows
    {
        #region "Functions - External"
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal struct TokPriv1Luid { public int Count; public long Luid; public int Attr;}
        [DllImport("kernel32.dll", ExactSpelling = true)]
        internal static extern IntPtr GetCurrentProcess();
        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        internal static extern bool OpenProcessToken(IntPtr h, int acc, ref IntPtr phtok);
        [DllImport("advapi32.dll", SetLastError = true)]
        internal static extern bool LookupPrivilegeValue(string host, string name, ref long pluid);
        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        internal static extern bool AdjustTokenPrivileges(IntPtr htok, bool disall, ref TokPriv1Luid newst, int len, IntPtr prev, IntPtr relen);
        [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
        internal static extern bool ExitWindowsEx(int flg, int rea);
        #endregion

        #region "Constants"
        internal const int SE_PRIVILEGE_ENABLED = 0x00000002;
        internal const int TOKEN_QUERY = 0x00000008;
        internal const int TOKEN_ADJUST_PRIVILEGES = 0x00000020;
        internal const string SE_SHUTDOWN_NAME = "SeShutdownPrivilege";
        internal const int EWX_LOGOFF = 0x00000000;
        internal const int EWX_SHUTDOWN = 0x00000001;
        internal const int EWX_REBOOT = 0x00000002;
        internal const int EWX_FORCE = 0x00000004;
        internal const int EWX_POWEROFF = 0x00000008;
        internal const int EWX_FORCEIFHUNG = 0x00000010;
        #endregion

        #region "Methods - Public"
        public static bool shutdown(bool force)
        {
            if (force) return DoExitWin(EWX_SHUTDOWN | EWX_FORCE);
            else return DoExitWin(EWX_SHUTDOWN);
        }
        public static bool restart(bool force)
        {
            if (force) return DoExitWin(EWX_REBOOT | EWX_FORCE);
            else return DoExitWin(EWX_REBOOT);
        }
        public static bool logoff(bool force)
        {
            if (force) return DoExitWin(EWX_LOGOFF | EWX_FORCE);
            else return DoExitWin(EWX_LOGOFF);
        }
        public static void applicationRestart()
        {
            Process.Start(Application.ExecutablePath);
            applicationExit();
        }
        public static void applicationExit()
        {
            Environment.Exit(0);
        }
        #endregion

        #region "Methods - Private"
        private static bool DoExitWin(int flg)
        {
            bool success;
            TokPriv1Luid tp;
            IntPtr hproc = GetCurrentProcess();
            IntPtr htok = IntPtr.Zero;
            success = OpenProcessToken(hproc, TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, ref htok);
            if (!success) return false;
            tp.Count = 1;
            tp.Luid = 0;
            tp.Attr = SE_PRIVILEGE_ENABLED;
            success = LookupPrivilegeValue(null, SE_SHUTDOWN_NAME, ref tp.Luid);
            if (!success) return false;
            success = AdjustTokenPrivileges(htok, false, ref tp, 0, IntPtr.Zero, IntPtr.Zero);
            if (!success) return false;
            return ExitWindowsEx(flg, 0);
        }
        #endregion
    }
}
