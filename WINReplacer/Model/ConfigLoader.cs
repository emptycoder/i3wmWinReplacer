using Newtonsoft.Json;
using System.IO;

namespace WINReplacer
{
    class ConfigLoader
    {
        public static IndexedList LoadFirstHashConfig(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            if (!File.Exists($"{path}\\first.json"))
            {
                IndexedList create = new IndexedList();
                SaveFirstHashConfig(path, create);
                return create;
            }
            return JsonConvert.DeserializeObject<IndexedList>(File.ReadAllText($"{path}\\first.json"));
        }

        public static FixedSizedQueue<App> LoadFavoritesConfig(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            if (!File.Exists($"{path}\\history.json"))
            {
                FixedSizedQueue<App> create = new FixedSizedQueue<App>(10);
                SaveLastStartedConfig(path, create);
                return create;
            }
            return JsonConvert.DeserializeObject<FixedSizedQueue<App>>(File.ReadAllText($"{path}\\history.json"));
        }

        public static IndexedList LoadSecondHashConfig(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            if (!File.Exists($"{path}\\second.json"))
            {
                IndexedList create = new IndexedList();
                SaveSecondHashConfig(path, create);
                return create;
            }
            return JsonConvert.DeserializeObject<IndexedList>(File.ReadAllText($"{path}\\second.json"));
        }

        public static void SaveLastStartedConfig(string path, FixedSizedQueue<App> config)
        {
            File.WriteAllText($"{path}\\history.json", JsonConvert.SerializeObject(config));
        }

        public static void SaveFirstHashConfig(string path, IndexedList config)
        {
            File.WriteAllText($"{path}\\first.json", JsonConvert.SerializeObject(config));
        }

        public static void SaveSecondHashConfig(string path, IndexedList config)
        {
            File.WriteAllText($"{path}\\second.json", JsonConvert.SerializeObject(config));
        }
    }
}
