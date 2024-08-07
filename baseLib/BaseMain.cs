﻿
using System;
using System.Runtime.InteropServices;

namespace csharp_lib.baseLib
{
    public static class BaseMain
    {
        const uint ENABLE_QUICK_EDIT = 0x0040;
        // STD_INPUT_HANDLE (DWORD): -10 is the standard input device.
        const int STD_INPUT_HANDLE = -10;
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetStdHandle(int nStdHandle);
        [DllImport("kernel32.dll")]
        static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);
        [DllImport("kernel32.dll")]
        static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);
        [DllImport("User32.dll")]
        static extern bool ShowWindow(IntPtr hConsoleHandle, int type);
        [DllImport("User32.dll")]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        public static System.Threading.Mutex  mutex;
        public static bool Go(string specailEventName=null)
        {
            string titleName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            if(specailEventName!=null)
                titleName = specailEventName;
            bool createNew;
            mutex = new System.Threading.Mutex(true, titleName, out createNew);
            if (!createNew)
            {
                Console.WriteLine("only a single instance can run in a time");
                throw new Exception("only a single instance can run in a time");
            }         
            IntPtr consoleHandle = GetStdHandle(STD_INPUT_HANDLE);
            // get current console mode
            uint consoleMode;
            if (!GetConsoleMode(consoleHandle, out consoleMode))
            {
                // ERROR: Unable to get console mode.
                return false;
            }
            Console.Title = titleName;
            // Clear the quick edit bit in the mode flags
            consoleMode &= ~ENABLE_QUICK_EDIT;
            // set the new mode
            if (!SetConsoleMode(consoleHandle, consoleMode))
            {
                // ERROR: Unable to set console mode
                return false;
            }
            IntPtr pwnd = new IntPtr(0);
            IntPtr et = new IntPtr(0);
            System.Threading.Thread.Sleep(10);
            pwnd = FindWindow(null, Console.Title);
            ShowWindow(pwnd, 2);
            return true;
        }

    }    
}
