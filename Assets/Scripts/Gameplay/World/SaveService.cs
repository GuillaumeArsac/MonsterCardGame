using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using MonsterCardGame.Gameplay.Content;
using MonsterCardGame.Gameplay.Monsters;

namespace MonsterCardGame.Gameplay.World
{
    public class SaveService : ISaveService
    {
        private static string SavePath => Path.Combine(Application.persistentDataPath, "save.json");

        private readonly IWorldProgressionService _progression;
        private readonly MonsterCollection        _collection;
        private readonly ContentDatabase          _content;

        public SaveService(IWorldProgressionService progression, MonsterCollection collection, ContentDatabase content)
        {
            _progression = progression;
            _collection  = collection;
            _content     = content;
        }

        public void Save()
        {
            var data = new SaveData();

            foreach (var id in _progression.CompletedEncounterIds)
                data.CompletedEncounters.Add(id);
            CollectionSaveMapper.Write(_collection, data);

            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(SavePath, json);
            Core.GameLog.Info("SaveService", $"Sauvegarde écrite : {SavePath}");
        }

        public void Load()
        {
            var data = File.Exists(SavePath) ? JsonConvert.DeserializeObject<SaveData>(File.ReadAllText(SavePath)) : null;
            if (data == null)
            {
                Core.GameLog.Info("SaveService", "Aucune sauvegarde trouvée — nouvelle partie");
                StartNewCollection();
                return;
            }

            _progression.LoadCompleted(data.CompletedEncounters);

            if (data.Collection.Count == 0 || _content == null)
            {
                StartNewCollection();
            }
            else
            {
                foreach (var item in CollectionSaveMapper.Read(data, _content, _collection))
                    Core.GameLog.Warning("SaveService", $"Élément de la sauvegarde introuvable, ignoré : {item}");
            }

            Core.GameLog.Info("SaveService", $"Sauvegarde chargée — {data.CompletedEncounters.Count} combat(s) gagné(s), {_collection.Monsters.Count} monstre(s)");
        }

        private void StartNewCollection()
        {
            if (_content == null) return;
            _collection.StartNewGame(_content.StarterMonsters);
        }
    }
}
