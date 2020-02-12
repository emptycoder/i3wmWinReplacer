using System;
using System.Diagnostics;

namespace WINReplacer
{
    public sealed class App : IApp
    {
        public string appPath;
        public string args;
        public string name;
        public DateTime lastStart;

        public App() { }

        public App(string appPath, string name, DateTime lastStart)
        {
            this.appPath = appPath;
            this.name = name;
            this.lastStart = lastStart;
        }

        public App(string appPath, string name, string args, DateTime lastStart)
        {
            this.args = args;
            this.appPath = appPath;
            this.name = name;
            this.lastStart = lastStart;
        }

        public bool StartProcess()
        {
            lastStart = DateTime.Now;
            return Process.Start(appPath, args) == null? false : true;
        }

        public override string ToString()
        {
            return $"{name} -> ({appPath}): {lastStart}";
        }
    }
}
