using System.IO;
using UnityEngine;

namespace MoonBorn.Utils
{
    public static class FileManager
    {
        public static void Save<T>(string filePath, T data)
        {
            string dataToJson = JsonUtility.ToJson(data, true);

            File.WriteAllText(filePath, dataToJson);
        }

        public static T Load<T>(string filePath) where T : new()
        {
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

        public static bool TryLoad<T>(string filePath, out T data)
        {
            if (File.Exists(filePath))
            {
                string dataAsJson = File.ReadAllText(filePath);
                data = JsonUtility.FromJson<T>(dataAsJson);
                return true;
            }
            data = default;
            return false;
        }

        public static void Delete(string filePath)
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }
}
