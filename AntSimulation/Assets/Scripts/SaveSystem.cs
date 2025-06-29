using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;

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

    public static void SaveAntsToCVS(string fileName, List<Colony.FitAntsPerCycle> ants)
    {
        string filePath = Path.Combine(Application.persistentDataPath, $"{fileName}.csv");

        List<string[]> _data = new List<string[]>
        {
            new string[] { "MovementSpeed", "Vision", "Strenght", "Curiosity", "Scouting", "Passiveness", "Agressiveness", "Fitness", "ADN", "Best Fitness" },
        };

        for (int i = 0; i < ants.Count; i += 2)
        {
            Colony.FitAntsPerCycle ant = ants[i];
            _data.Add(new string[] {ant.antData.moveSpeed.ToString(), ant.antData.senseRadius.ToString(), ant.antData.strenght.ToString(),
                                    ant.antData.curiosity.ToString(), ant.antData.scouting.ToString(), ant.antData.pasiveness.ToString(),
                                    ant.antData.agressiveness.ToString(), ant.antData.fitness.ToString(), ant.antData.ADN.ToString()});
        }

        _data.Add(new string[] { "MovementSpeed", "Vision", "Strenght", "Curiosity", "Scouting", "Passiveness", "Agressiveness", "Fitness", "ADN", "2nd Best Fitness" });

        for (int i = 1; i < ants.Count; i += 2)
        {
            Colony.FitAntsPerCycle ant = ants[i];
            _data.Add(new string[] {ant.antData.moveSpeed.ToString(), ant.antData.senseRadius.ToString(), ant.antData.strenght.ToString(),
                                    ant.antData.curiosity.ToString(), ant.antData.scouting.ToString(), ant.antData.pasiveness.ToString(),
                                    ant.antData.agressiveness.ToString(), ant.antData.fitness.ToString(), ant.antData.ADN.ToString()});
        }

        using (StreamWriter writer = new StreamWriter(filePath))
        {
            foreach (var line in _data)
            {
                writer.WriteLine(string.Join(",", line));
            }
        }

        Debug.Log("CSV saved to: " + filePath);
    }
}
