using System.Collections.Generic;

namespace WINReplacer
{
    interface IFinder
    {
        List<App> FindByName(string name);
        void Save();
    }
}
