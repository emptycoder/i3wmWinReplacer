using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management;

namespace WINReplacer
{
    abstract class ProcessWatcher
    {
        protected readonly string ConfigPath;
        ManagementEventWatcher StartWatch;
        protected IndexedList firstLevelProcessHashes;
        protected IndexedList secondLevelProcessHashes; //For addiction process, but if u use portable versions of progs it will be impact
        protected Dictionary<string, App> processExeNames;

        public struct ProcessHash : IEquatable<ProcessHash>
        {
            public string Name;
            public string Path;
            public DateTime LiveHashTime;

            public ProcessHash(string Name, string Path, DateTime LiveHashTime)
            {
                this.Name = Name;
                this.Path = Path;
                this.LiveHashTime = LiveHashTime;
            }

            public bool Equals(ProcessHash other)
            {
                return this.Name == other.Name;
            }
        }

        public ProcessWatcher(string ConfigPath)
        {
            this.ConfigPath = ConfigPath;

            this.firstLevelProcessHashes = new IndexedList();
            this.secondLevelProcessHashes = ConfigLoader.LoadSecondHashConfig(ConfigPath);
            this.processExeNames = new Dictionary<string, App>();

            StartWatch = new ManagementEventWatcher(new WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace"));
            StartWatch.EventArrived += new EventArrivedEventHandler(startWatch_ProcessCreated);
            StartWatch.Start();
        }

        protected List<App> FindByPattern(string pattern)//TODO: reworking answer + alghoritm
        {
            List<App> apps = firstLevelProcessHashes.TryToFind(pattern);
            List<App> get = secondLevelProcessHashes.TryToFind(pattern);

            if (apps == null)
            {
                apps = get;
            }
            else if (get != null)
            {
                apps.AddRange(get);
            }

            if (apps == null)
            {
                apps = new List<App>();
                foreach (KeyValuePair<string, App> app in processExeNames)
                {
                    if (Path.GetFileNameWithoutExtension(app.Key).Contains(pattern))
                    {
                        apps.Add(app.Value);
                    }
                }
                if (apps.Count == 0) { apps = null; }
            }

            return apps;
        }

        private void startWatch_ProcessCreated(object sender, EventArrivedEventArgs e)
        {
            try
            {
                string fullpath = Process.GetProcessById(Convert.ToInt32(e.NewEvent.Properties["ProcessID"].Value)).MainModule.FileName;
                if (this.processExeNames.TryGetValue(fullpath, out App app))
                {
                    app.last_start = DateTime.Now;
                }
                else
                {
                    string product_name = FileVersionInfo.GetVersionInfo(fullpath).ProductName;
                    this.secondLevelProcessHashes.TryToAdd(product_name == null? Path.GetFileNameWithoutExtension(fullpath).ToLower() : product_name.ToLower(), fullpath, ref app);
                    this.processExeNames.Add(fullpath, app);
                }

                Console.WriteLine($"Added: {fullpath}");
            }
            catch { }
        }

        protected void DisposeProcessWatcher()
        {
            StartWatch.Stop();
            StartWatch.Dispose();
        }
    }
}
