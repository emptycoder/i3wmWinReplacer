using System;
using System.Collections.Generic;

namespace WINReplacer
{
    interface IIndexedList
    {
        bool TryToAdd(string name, string path, ref App output, string args = null);
        bool TryToAdd(string name, string path, DateTime last_start, ref App output, string args = null);
        List<App> TryToFind(string pattern);
        void TryToDelete(string name);
        App TryToGetApp(string name);
    }
}
