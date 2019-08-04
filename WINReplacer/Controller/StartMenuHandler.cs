using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace WINReplacer
{
    class StartMenuHandler : ProcessWatcher, IDisposable, IFinder
    {
        List<FileSystemWatcher> watchers = new List<FileSystemWatcher>();
        public StartMenuHandler(string ConfigPath, string[] StartMenuPathes) : base(ConfigPath)
        {
            IndexedList firstSaved = ConfigLoader.LoadFirstHashConfig(ConfigPath);
            //Scan Menu Folder
            foreach (string menu_path in StartMenuPathes)
            {
                foreach (string symlink_path in Directory.GetFiles(menu_path, "*.lnk", SearchOption.AllDirectories))
                {
                    string name = Path.GetFileNameWithoutExtension(symlink_path).ToLower();
                    string real_path = Symlink.GetRealPath(symlink_path);
                    if (Path.GetExtension(real_path) != ".exe") continue;
                    App output = firstSaved.TryToGetApp(name);
                    if (output != null)
                    {
                        firstLevelProcessHashes.TryToAdd(name, symlink_path, output.lastStart, ref output);
                    }
                    else
                    {
                        firstLevelProcessHashes.TryToAdd(name, symlink_path, ref output);
                    }
                    if (!processExeNames.ContainsKey(real_path))
                    {
                        processExeNames.Add(real_path, output);
                    }
                }
            }
            //Start watching StartUp folder
            foreach (string menu_path in StartMenuPathes)
            {
                FileSystemWatcher watcher = new FileSystemWatcher();
                watcher.Path = menu_path;
                watcher.IncludeSubdirectories = true;
                watcher.Filter = "*.lnk";
                watcher.Created += new FileSystemEventHandler(OnCreated);
                watcher.Deleted += new FileSystemEventHandler(OnDeleted);
                watcher.Renamed += new RenamedEventHandler(OnRenamed);
                watcher.EnableRaisingEvents = true;
                watchers.Add(watcher);
            }
        }

        private void OnRenamed(object sender, RenamedEventArgs e)
        {
            Console.WriteLine("RENAME: " + e.FullPath);
            string name = e.Name.ToLower();
            processExeNames[name].name = name;
        }

        private void OnCreated(object source, FileSystemEventArgs e)
        {
            Console.WriteLine("CREATE: " + e.FullPath);
            string real_path = Symlink.GetRealPath(e.FullPath);
            string product_name = FileVersionInfo.GetVersionInfo(real_path).ProductName;
            App app = null;
            if (this.firstLevelProcessHashes.TryToAdd(product_name == null? e.Name.ToLower() : product_name.ToLower(), real_path, ref app))
            {
                if (!processExeNames.ContainsKey(real_path))
                {
                    processExeNames.Add(real_path, app);
                }
            }
        }

        private void OnDeleted(object source, FileSystemEventArgs e)
        {
            Console.WriteLine("DELETE: " + e.FullPath);
            string real_path = Symlink.GetRealPath(e.FullPath);
            if (processExeNames.ContainsKey(real_path))
            {
                processExeNames.Remove(real_path);
            }
        }

        public List<App> FindByName(string name)
        {
            if (name.Length == 1) return null;
            name = name.ToLower();
            return FindByPattern(name);
        }

        public void Save()
        {
            ConfigLoader.SaveFirstHashConfig(ConfigPath, firstLevelProcessHashes);
            ConfigLoader.SaveSecondHashConfig(ConfigPath, secondLevelProcessHashes);
        }

        public void Dispose()
        {
            DisposeProcessWatcher();
            watchers.ForEach(watcher => watcher.Dispose());
        }
    }
}
