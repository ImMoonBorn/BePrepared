using System.IO;
using UnityEngine;

namespace MoonBorn.Utils
{
    public static class FileManager
    {
        private static string s_SavePath = Application.persistentDataPath;
        private const string c_Extension = ".txt";

        public static void SetPath(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            s_SavePath = path;
        }

        public static void Save<T>(string filename, T data)
        {
            string dataToJson = JsonUtility.ToJson(data, true);
            string filePath = Path.Combine(s_SavePath, filename + c_Extension);

            File.WriteAllText(filePath, dataToJson);
        }

        public static T Load<T>(string filename) where T : new()
        {
            string filePath = Path.Combine(s_SavePath, filename + c_Extension);

            T data;
            if (File.Exists(filePath))
            {
                string dataAsJson = File.ReadAllText(filePath);
                data = JsonUtility.FromJson<T>(dataAsJson);
            }
            else
            {
                data = new T();
            }
            return data;
        }

        public static bool TryLoad<T>(string filename, out T data)
        {
            string filePath = Path.Combine(s_SavePath, filename + c_Extension);

            if (File.Exists(filePath))
            {
                string dataAsJson = File.ReadAllText(filePath);
                data = JsonUtility.FromJson<T>(dataAsJson);
                return true;
            }
            data = default;
            return false;
        }

        public static void Delete(string filename)
        {
            string filePath = Path.Combine(s_SavePath, filename + c_Extension);

            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }
}
