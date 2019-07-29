using System;
using System.Collections.Generic;
using System.Linq;

namespace WINReplacer
{
    public class QuickSort
    {
        public static void Sort(ref List<App> unsort, int min, int count_items)
        {
            if (unsort.Count == 1) return;
            int pivot = Partition(ref unsort, 0, unsort.Count > count_items ? count_items : unsort.Count - 1);
            if (pivot > 1)
            {
                Sort(ref unsort, 0, pivot - 1);
            }
            if (pivot == 0) { return;  }
            if (pivot + 1 < count_items)
            {
                Sort(ref unsort, pivot + 1, unsort.Count > count_items ? count_items : unsort.Count - 1);
            }
        }

        public static void Sort(ref List<App> unsort, string pattern, int min, int count_items)
        {
            if (unsort.Count == 1) return;
            List<App> patternList = unsort.Where(app => isPatternContains(ref app, ref pattern)).ToList();
            if (patternList.Count == 0) return;
            Sort(ref patternList, min, patternList.Count > count_items? count_items : patternList.Count - 1);
            unsort = patternList;
        }

        private static int Partition(ref List<App> unsort, int min, int max)
        {
            DateTime pivot = unsort[min].last_start;
            while (true)
            {

                while (unsort[min].last_start < pivot)
                {
                    min++;
                }

                while (unsort[max].last_start > pivot)
                {
                    max--;
                }

                if (min < max)
                {
                    if (unsort[min].last_start == unsort[max].last_start)
                    {
                        return max;
                    }

                    var temp = unsort[min];
                    unsort[min] = unsort[max];
                    unsort[max] = temp;


                }
                else
                {
                    return max;
                }
            }
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
