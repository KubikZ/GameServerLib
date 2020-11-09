using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;

namespace GameServerLib.Tools
{
    /// <summary>
    /// Class for writing log files
    /// </summary>
    public static class Debug
    {
        static string filePath;
        static Queue<string> logsToWrite = new Queue<string>();

        private static void CreateDebugFile()
        {
            new Thread(LoggingThread).Start();
        }

        static void LoggingThread()
        {
            if (!Directory.Exists("Logs"))
                Directory.CreateDirectory("Logs");

            filePath = "Logs/" + FormatFileName() + ".log";

            while (true)
            {
                lock (logsToWrite)
                {
                    while (logsToWrite.Count > 0)
                    {

                        try
                        {
                            string line = GetTime(':') + ": " + logsToWrite.Dequeue();
                            File.AppendAllLines(filePath, new[] { line }, System.Text.Encoding.UTF8);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Failed to save to the debug file! Exception: " + e.Message);
                        }
                    }
                }

                Thread.Sleep(100);
            }
        }

        public static void Log(string s)
        {
            if (filePath != null)
            {
                Console.WriteLine(s);
                LogToFile(s);
            }
            else
            {
                CreateDebugFile();
                Log(s);
            }

        }
        private static void LogToFile(string s)
        {
            logsToWrite.Enqueue(s);
        }

        public static string FormatFileName()
        {
            return $"{GetDate()} {GetTime('.')}";
        }
        public static string GetTime(char separationChar)
        {
            return $"{DateTime.Now.Hour.ToString().PadLeft(2, '0')}{separationChar}" +
                    $"{DateTime.Now.Minute.ToString().PadLeft(2, '0')}{separationChar}" +
                    $"{DateTime.Now.Second.ToString().PadLeft(2, '0')}";
        }
        public static string GetDate()
        {
            return $"{DateTime.Today.Day.ToString().PadLeft(2, '0')}.{DateTime.Today.Month.ToString().PadLeft(2, '0')}.{DateTime.Today.Year}";
        }
    }
}