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
            int startTime = Environment.TickCount;
            List<App> apps = secondLevelProcessHashes.TryToFind(pattern);
            List<App> get = firstLevelProcessHashes.TryToFind(pattern);

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
            Console.WriteLine($"Find by pattern time: {Environment.TickCount - startTime}");

            return apps;
        }

        private void startWatch_ProcessCreated(object sender, EventArrivedEventArgs e)
        {
            try
            {
                string fullpath = Process.GetProcessById(Convert.ToInt32(e.NewEvent.Properties["ProcessID"].Value)).MainModule.FileName;
                if (fullpath.ToLower().StartsWith("c:\\windows\\")) { return; }
                string productName = FileVersionInfo.GetVersionInfo(fullpath).ProductName;
                int startTime = Environment.TickCount;
                productName = productName == null ? Path.GetFileNameWithoutExtension(fullpath).ToLower() : productName.ToLower();

                if (this.processExeNames.TryGetValue(fullpath, out App app) || (app = firstLevelProcessHashes.TryToGetApp(productName)) != null)
                {
                    Logger.log.Info($"Update time: {fullpath}, {productName}");
                    app.lastStart = DateTime.Now;
                }
                else
                {
                    Logger.log.Info($"Added to secondLevelHash: {fullpath}, {productName}");
                    this.secondLevelProcessHashes.TryToAdd(productName, fullpath, ref app);
                    this.processExeNames.Add(fullpath, app);
                }

                Console.WriteLine($"Process created time: {Environment.TickCount - startTime}");
            }
            catch { } //If process ended earlier
        }

        protected void DisposeProcessWatcher()
        {
            StartWatch.Stop();
            StartWatch.Dispose();
        }
    }
}
