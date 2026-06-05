using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
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
        private static readonly ConcurrentQueue<LogMsg> _msgQueue = new ConcurrentQueue<LogMsg>();
        protected static readonly AutoResetEvent _msgARE = new AutoResetEvent(false);
        private static readonly object _workerLock = new object();
        private static Task? _workerTask;
        private static volatile bool _shutdownRequested = false;
        private string _className;
        private MyLogger()
        {
            EnsureWorkerStarted();
        }
        private object olock = new object();
        int curLogLevel = 0;
        private static void EnsureWorkerStarted()
        {
            if (_workerTask != null)
                return;

            lock (_workerLock)
            {
                if (_workerTask != null)
                    return;

                _shutdownRequested = false;
                _workerTask = Task.Factory.StartNew(MonitorMsg, TaskCreationOptions.LongRunning);
            }
        }
        public static MyLogger GetLogger(string className)
        {
            return GetLogger(className, HeiFei_20220103.Config.GetInstance().logLevel);
        }
        public static void shutdown()
        {
            Task? workerTask = null;
            lock (loggmap)
            {
                foreach(var log in loggmap)
                {
                    log.Value.Dispose();
                }
                loggmap.Clear();
            }
            lock (_workerLock)
            {
                _shutdownRequested = true;
                _msgARE.Set();
                workerTask = _workerTask;
                _workerTask = null;
            }
            workerTask?.Wait(TimeSpan.FromSeconds(2));
        }
        public static MyLogger GetLogger(string className,int loggerLevel)
        {
            lock (loggmap)
            {
                if (!loggmap.ContainsKey(className))
                {
                    var loo = new MyLogger();
                    loo._className = className;
                    loo.curLogLevel = loggerLevel;
                    loggmap.Add(className, loo);



                    var exePath = Process.GetCurrentProcess().MainModule.FileName;

                    FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(exePath);
                    Console.WriteLine($"fileVersionInfo  {fileVersionInfo}");
                    string version = fileVersionInfo.ProductVersion;
                    loo.Error($"======start===={version}=================");
                    loo.Error($"======exePath===={exePath}=================");
                }
                return loggmap[className];
            }
        }
        protected bool disposedValue = false; // To detect redundant calls
        public virtual void Dispose()
        {
            disposedValue = true;
        }

        static void MonitorMsg()
        {
            while (!_shutdownRequested)
            {
                try
                {
                    _msgARE.WaitOne(TimeSpan.FromSeconds(10));
                    FlushQueue();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"RecvSync error. while {e.Message}");
                }
            }

            FlushQueue();
        }
        static void FlushQueue()
        {
            while (_msgQueue.TryDequeue(out var data))
            {
                Console.WriteLine(data.content);
                data.logger.WriteLogs("logs", data.threadID, data.type, data.content);
            }
        }
        public void WriteLogs(string dirName,int threadID, string type, string content)
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
                        sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")
                        + $" {threadID} "
                        //+ (_className ?? "") 
                         + type[0] + " : " + content);
                        sw.Close();
                    }
                }
            }
        }

        class LogMsg
        {
            public MyLogger logger = null!;
            public string type;
            public string content;
            public  int threadID=Thread.CurrentThread.ManagedThreadId;
        }
        private void Log(string type, string content)
        {
            if (disposedValue || _shutdownRequested) return;
            _msgQueue.Enqueue(new LogMsg { logger = this, type = type, content = content });
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
