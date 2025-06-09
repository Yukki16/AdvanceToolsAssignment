using System.IO;
using UnityEngine;
using Newtonsoft.Json;
public static class SaveSystem
{
    private static string GetSavePath(string fileName) => Path.Combine(Application.persistentDataPath, fileName + ".json");

    public static void Save<T>(string fileName, T data)
    {
        string json = JsonConvert.SerializeObject(data, Formatting.Indented);
        File.WriteAllText(GetSavePath(fileName), json);
        Debug.Log($"Saved {typeof(T).Name} data to {GetSavePath(fileName)}");
    }

    public static T Load<T>(string fileName) where T : new()
    {
        string path = GetSavePath(fileName);
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<T>(json);
        }

        Debug.LogWarning($"Save file '{fileName}' not found, creating new {typeof(T).Name} data.");
        return new T(); // Return a new instance if no save exists
    }
}
