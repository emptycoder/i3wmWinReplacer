using System;
using System.Diagnostics;

namespace WINReplacer
{
    public sealed class App : IApp
    {
        public string app_path;
        public string name;
        public DateTime last_start;

        public App(string app_path, string name, DateTime last_start)
        {

            this.app_path = app_path;
            this.name = name;
            this.last_start = last_start;
        }

        public bool StartProcess()
        {
            last_start = DateTime.Now;
            return Process.Start(app_path) == null? false : true;
        }

        public override string ToString()
        {
            return $"{name} -> ({app_path}): {last_start}";
        }
    }
}
