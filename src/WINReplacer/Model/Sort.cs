using System.Collections.Generic;
using System.Linq;

namespace WINReplacer
{
    public static class Sort
    {
        public static void AscSort(ref List<App> unsort, string pattern)
        {
            if (unsort.Count == 1) return;
            unsort = unsort.Where(app => isPatternContains(ref app, ref pattern)).OrderBy(app => app.lastStart).ToList();
        }

        public static void AscSort(ref List<App> unsort)
        {
            if (unsort.Count == 1) return;
            unsort = unsort.OrderBy(app => app.lastStart).ToList();
        }

        private static bool isPatternContains(ref App app, ref string pattern)
        {
            int i;
            //TODO: reworking to start of word + spell check + pattern change
            var words = app.name.Split(' ');
            foreach (string word in words)
            {
                if (pattern.Length <= word.Length)
                {
                    for (i = 0; i < pattern.Length; i++)
                    {
                        if (word[i] != pattern[i]) { break; }
                    }
                    if (i == pattern.Length) { return true; }
                }
            }
            return false;
        }
    }
}
