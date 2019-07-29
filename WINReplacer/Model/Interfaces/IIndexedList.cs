using System;
using System.Collections.Generic;

namespace WINReplacer
{
    interface IIndexedList
    {
        bool TryToAdd(string name, string path, ref App output);
        bool TryToAdd(string name, string path, DateTime last_start, ref App output);
        List<App> TryToFind(string pattern);
        void TryToDelete(string name);
        App TryToGetApp(string name);
    }
}
