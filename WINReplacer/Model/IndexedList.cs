using System;
using System.Collections.Generic;
using System.Linq;
using ListExtension;

namespace WINReplacer
{
    public class IndexedList : IIndexedList
    {
        private HashSet<Index> indexedArray;

        public IndexedList()
        {
            indexedArray = new HashSet<Index>();
        }

        public IndexedList(HashSet<Index> indexedArray)
        {
            this.indexedArray = indexedArray;
        }

        public HashSet<Index> GetIndexedArray()
        {
            return indexedArray;
        }

        public bool TryToAdd(string name, string path, ref App output, string args = null)
        {
            return TryToAdd(name, path, DateTime.Now, ref output, args);
        }

        public bool TryToAdd(string name, string path, DateTime last_start, ref App output, string args = null)
        {
            name = name.ToLower();
            string sub_name = (name.Length > 3) ? name.Substring(0, 3) : ThreeSymbolFix(name);

            string sec_name = sub_name.Substring(0, 2);
            string fir_name = sub_name.Substring(0, 1);

            if (indexedArray.TryGetValue(new Index(fir_name), out Index nextStep1))
            {
                if (nextStep1.nextIndex.TryGetValue(new Index(sec_name), out Index nextStep2))
                {
                    if (nextStep2.nextIndex.TryGetValue(new Index(sub_name), out Index nextStep3))
                    {
                        if ((output = nextStep3.apps.FirstOrDefault(app => app.name == name)) != null)
                        {
                            output.lastStart = DateTime.Now;
                            return false;
                        }
                        else
                        {
                            output = new App(path, name, args, last_start);
                            nextStep3.apps.Add(output);
                            nextStep2.apps.Add(output);
                        }
                    }
                    else
                    {
                        output = new App(path, name, args, DateTime.Now);
                        nextStep2.nextIndex.Add(
                            new Index(
                                sub_name,
                                new List<App> { output }
                            )
                        );
                    }
                }
                else
                {
                    output = new App(path, name, args, last_start);
                    nextStep1.nextIndex.Add(
                        new Index(
                            sec_name,
                            new List<App> { output },
                            new HashSet<Index> {
                                new Index(
                                    sub_name,
                                    new List<App> { output }
                                )
                            }
                        )
                    );
                }
            }
            else
            {
                output = new App(path, name, args, last_start);
                indexedArray.Add(
                    new Index(
                        fir_name,
                        null,
                        new HashSet<Index>()
                        {
                            new Index(
                                sec_name,
                                new List<App>() { output },
                                new HashSet<Index>()
                                {
                                    new Index(
                                        sub_name,
                                        new List<App>() { output }
                                    )
                                }
                            )
                        }
                    )
                );
            }

            return true;
        }

        public List<App> TryToFind(string pattern)
        {
            if (pattern.Length < 2) return null;
            pattern = pattern.ToLower();

            if (indexedArray.TryGetValue(new Index(pattern.Substring(0, 1)), out Index nextStep1))
            {
                if (nextStep1.nextIndex.TryGetValue(new Index(pattern.Substring(0, 2)), out Index nextStep2))
                {
                    if (pattern.Length == 2)
                    {
                        QuickSort.Sort(ref nextStep2.apps, 0, WIN.ControlsCount);
                        return nextStep2.apps;
                    }

                    if (nextStep2.nextIndex.TryGetValue(new Index(pattern.Substring(0, 3)), out Index nextStep3))
                    {
                        QuickSort.Sort(ref nextStep3.apps, pattern, 0, WIN.ControlsCount);
                        return nextStep3.apps;
                    }
                }
            }

            return null;
        }

        public App TryToGetApp(string name)
        {
            if (name.Length < 2) return null;
            name = name.ToLower();

            if (indexedArray.TryGetValue(new Index(name.Substring(0, 1)), out Index nextStep1))
            {
                if (nextStep1.nextIndex.TryGetValue(new Index(name.Substring(0, 2)), out Index nextStep2))
                {
                    if (name.Length == 2)
                    {
                        return nextStep2.apps.FirstOrDefault(item => item.name == name);
                    }

                    if (nextStep2.nextIndex.TryGetValue(new Index(name.Substring(0, 3)), out Index nextStep3))
                    {
                        return nextStep3.apps.FirstOrDefault(item => item.name == name);
                    }
                }
            }

            return null;
        }

        public void TryToDelete(string name)
        {
            name = (name.Length > 3) ? name.Substring(0, 3) : ThreeSymbolFix(name);
            name = name.ToLower();

            if (indexedArray.TryGetValue(new Index(name.Substring(0, 1)), out Index nextStep1))
            {
                if (nextStep1.nextIndex.TryGetValue(new Index(name.Substring(0, 2)), out Index nextStep2))
                {
                    if (nextStep2.nextIndex.TryGetValue(new Index(name.Substring(0, 3)), out Index nextStep3))
                    {
                        nextStep2.apps.RemoveByName(name);
                        nextStep1.apps.RemoveByName(name);
                    }
                }
            }
        }

        private string ThreeSymbolFix(string name)
        {
            while (name.Length < 3)
            {
                name += "?";
            }

            return name;
        }

        public struct Index : IEquatable<Index>
        {
            public string pattern;
            public List<App> apps;
            public HashSet<Index> nextIndex { get; set; }

            public Index(string pattern, List<App> apps = null, HashSet<Index> nextIndex = null)
            {
                this.pattern = pattern;
                this.apps = apps;
                this.nextIndex = nextIndex;
            }

            public bool Equals(Index other)
            {
                return this.pattern == other.pattern;
            }
        }
    }
}
