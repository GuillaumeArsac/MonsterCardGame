using System.IO;
using UnityEngine;
using Newtonsoft.Json;

namespace MonsterCardGame.Gameplay.World
{
    public class SaveService : ISaveService
    {
        private static string SavePath => Path.Combine(Application.persistentDataPath, "save.json");

        private readonly IWorldProgressionService _progression;

        public SaveService(IWorldProgressionService progression)
        {
            _progression = progression;
        }

        public void Save()
        {
            var data = new SaveData();

            foreach (var id in _progression.CompletedEncounterIds)
                data.CompletedEncounters.Add(id);

            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(SavePath, json);
            Core.GameLog.Info("SaveService", $"Sauvegarde écrite : {SavePath}");
        }

        public void Load()
        {
            if (!File.Exists(SavePath))
            {
                Core.GameLog.Info("SaveService", "Aucune sauvegarde trouvée — nouvelle partie");
                return;
            }

            var json = File.ReadAllText(SavePath);
            var data = JsonConvert.DeserializeObject<SaveData>(json);
            if (data == null) return;

            _progression.LoadCompleted(data.CompletedEncounters);

            Core.GameLog.Info("SaveService", $"Sauvegarde chargée — {data.CompletedEncounters.Count} combat(s) gagné(s)");
        }
    }
}
