using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ControlWork_19._10
{
    class FileWatcher
    {
        DateTime changeTime;
        private string _filePath;
        public event Action<DateTime, string> Change;
        private readonly object _eventSync = new object();

        public FileWatcher(string path)
        {
            _filePath = path;
        }

        public void Start()
        {
            Thread t = new Thread(() =>
            {
                changeTime = File.GetLastWriteTime(_filePath);
                while (true)
                {
                    if (changeTime != File.GetLastWriteTime(_filePath))
                    {
                        changeTime = File.GetLastWriteTime(_filePath);
                        lock (_eventSync)
                        {
                            Change?.Invoke(changeTime, _filePath);
                        }
                    }
                }
            });
            t.Start();
        }
    }
    class Program
    {
        private static readonly object syncObj1 = new object();
        private static readonly object syncObj2 = new object();

        static void Main(string[] args)
        {
            var path = "test.txt";
            if (!File.Exists(path))
            {
                File.Create(path).Dispose();
            }


            var watcher = new FileWatcher(path);
            watcher.Change += Watcher_Change;
            watcher.Start();

            while (true)
            {
                Console.WriteLine("Do you want 1(y|n)?");
                string answer = Console.ReadLine().ToLower();

                if (answer == "y")
                {
                    lock (syncObj2)
                    {
                        File.WriteAllText(path, "1");
                    }
                }
            }
        }

        private static void Watcher_Change(DateTime changeTime, string path)
        {
            Thread thread = new Thread(() =>
            {
                lock (syncObj1)
                {
                    string contains = File.ReadAllText(path);
                    if (contains == "1")
                    {
                        Console.WriteLine("File changed: " + changeTime);
                        File.WriteAllText(path, "0");
                        Thread.Sleep(10000);
                        Console.WriteLine("File content changed to '0' ten seconds ago");
                    }
                }
            });
            thread.Start();
        }
    }
}
