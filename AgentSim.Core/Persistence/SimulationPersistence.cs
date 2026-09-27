using System;
using System.IO;
using System.Text.Json;

namespace AgentSim.Core.Persistence
{
    public static class SimulationPersistence
    {
        public static void Save(SimulationSaveData data, string filePath)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            string json = JsonSerializer.Serialize(data, options);
            File.WriteAllText(filePath, json);
        }

        public static SimulationSaveData? Load(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return null;
            }

            try
            {
                string json = File.ReadAllText(filePath);

                return JsonSerializer.Deserialize<SimulationSaveData>(json);
            }
            catch
            {
                return null;
            }
        }
    }
}