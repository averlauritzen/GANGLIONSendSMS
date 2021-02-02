using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;

namespace GANGLIONSendSMS
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (IsAppAlreadyRunning() == false)
            {
                Application.EnableVisualStyles(); 
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1());
            }
            else
            {
                MessageBox.Show("GANGLIONSendSMS application is already running.");
            }
        }

        public static bool IsAppAlreadyRunning()
        {
            bool isAlreadyRunning = false;
            Process currentProcess = Process.GetCurrentProcess();
            Process[] processes = Process.GetProcesses();
            foreach (Process process in processes)
            {
                if (currentProcess.Id != process.Id)
                {
                    if (currentProcess.ProcessName == process.ProcessName)
                    {
                        isAlreadyRunning = true;
                    }
                }
            }
            return isAlreadyRunning;
        }
    }
}
