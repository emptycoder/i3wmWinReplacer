using NUnit.Framework;
using System;
using System.Collections.Generic;
using WINReplacer;

namespace Tests
{
    public class QuickSortTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Sort()
        {
            List<App> unsort = new List<App>()
            {
                new App("C:\\test1", "test1", new DateTime(2019, 5, 23)),
                new App("C:\\test2", "test2", new DateTime(2019, 1, 23)),
                new App("C:\\test3", "test3", new DateTime(2019, 1, 21)),
                new App("C:\\test4", "test4", new DateTime(2019, 2, 20)),
                new App("C:\\test5", "test5", new DateTime(2019, 1, 30))
            };

            QuickSort.Sort(ref unsort, 0, 2);

            Assert.IsTrue(unsort[0].name == "test3"
                && unsort[1].name == "test2"
                && unsort[2].name == "test1"
                && unsort[3].name == "test4"
                && unsort[4].name == "test5");
        }
    }
}