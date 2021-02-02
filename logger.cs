using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GANGLIONSendSMS
{
   public static class logger
    {
       
            private static string appPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            private static string logFile = "sms_log.txt";

            public static void writeToLog(string msg, int logLevel)
            {
                if (logLevel > 0)
                {
                    try
                    {
                        string m = new StringBuilder().Append("[").Append(DateTime.Now).Append("]: ").Append(msg).AppendLine().ToString();
                        StreamWriter sw = File.AppendText(Path.Combine(appPath, logFile));
                        sw.Write(m);
                        sw.Close();
                    }
                    catch
                    {
                    }
                }
            }

            public static void writeToLogAlways(string msg)
            {
                try
                {
                    string m = new StringBuilder().Append("[").Append(DateTime.Now).Append("]: ").Append(msg).AppendLine().ToString();
                    StreamWriter sw = File.AppendText(Path.Combine(appPath, logFile));
                    sw.Write(m);
                    sw.Close();
                }
                catch
                {
                }
            }
        }
    }
