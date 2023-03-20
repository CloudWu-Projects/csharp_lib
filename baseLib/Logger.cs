using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace csharp_lib.baseLib
{
    public class MyLogger : IDisposable
    {

        private static Dictionary<string, MyLogger> loggmap = new Dictionary<string, MyLogger>();
        private string _className;
        private MyLogger()
        {
            Task.Factory.StartNew(MonitorMsg, TaskCreationOptions.LongRunning);
        }
        private object olock = new object();
        int curLogLevel = 0;
        public static MyLogger GetLogger(string className,int customLogLevel=0)
        {
            if (!loggmap.ContainsKey(className))
            {
                var loo = new MyLogger();
                loo._className = className;
                loo.curLogLevel = customLogLevel;
                loggmap.Add(className, loo);
            }
            return loggmap[className];
        }
        protected bool disposedValue = false; // To detect redundant calls
        public virtual void Dispose()
        {
            disposedValue = true;
        }

        void MonitorMsg()
        {
            while (!disposedValue)
            {
                try
                {
                    _msgARE.WaitOne(TimeSpan.FromSeconds(10));
                    while (_msgQueue.TryDequeue(out var data))
                    {
                        Console.WriteLine(data.content);
                        WriteLogs("logs", data.type, data.content);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"RecvSync error. while {e.Message}");
                }
            }

            _msgQueue.Clear();
            _msgQueue = null;
        }
        public void WriteLogs(string dirName, string type, string content)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory;
            if (!string.IsNullOrEmpty(path))
            {
                lock (olock)
                {
                    path = AppDomain.CurrentDomain.BaseDirectory + dirName;
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    path = path + "\\" + _className + "_" + DateTime.Now.ToString("yyyyMMdd") + ".log";
                    if (!File.Exists(path))
                    {
                        FileStream fs = File.Create(path);
                        fs.Close();
                    }
                    if (File.Exists(path))
                    {
                        StreamWriter sw = new StreamWriter(path, true, System.Text.Encoding.Default);
                        sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ") + (_className ?? "") + " : " + type + " --> " + content);
                        sw.Close();
                    }
                }
            }
        }

        class LogMsg
        {
            public string type;
            public string content;
        }
        ConcurrentQueue<LogMsg> _msgQueue = new ConcurrentQueue<LogMsg>();
        protected readonly AutoResetEvent _msgARE = new AutoResetEvent(false);
        private void Log(string type, string content)
        {
            _msgQueue.Enqueue(new LogMsg { type = type, content = content });
            _msgARE.Set();
        }

        public void Debug(string content)
        {
            if (curLogLevel > 0) return;
            Log("Debug", content);
        }

        public void Info(string content)
        {
            if (curLogLevel > 1) return;
            Log("Info", content);
        }

        public void Warn(string content)
        {
             if (curLogLevel > 2) return;
            Log("Warn", content);
        }

        public void Error(string content)
        {
            Log("Error", content);
        }

        public void Fatal(string content)
        {
            Log("Fatal", content);
        }
    }
}
