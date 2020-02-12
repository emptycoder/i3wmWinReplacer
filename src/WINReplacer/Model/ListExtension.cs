using System.Collections.Generic;
using WINReplacer;

namespace ListExtension
{
    public static class ListExtension
    {
        public static void RemoveByName(this List<App> apps, string name)
        {
            for (int i = 0; i < apps.Count; i++)
            {
                if (apps[i].name == name)
                {
                    apps.RemoveAt(i);
                    return;
                }
            }
        }
    }
}
