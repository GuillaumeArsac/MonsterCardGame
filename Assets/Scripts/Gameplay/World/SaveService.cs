using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using MonsterCardGame.Gameplay.Inventory;

namespace MonsterCardGame.Gameplay.World
{
    public class SaveService : ISaveService
    {
        private static string SavePath => Path.Combine(Application.persistentDataPath, "save.json");

        private readonly GameRegistry            _registry;
        private readonly IPlayerInventory        _inventory;
        private readonly IWorldProgressionService _progression;

        public SaveService(GameRegistry registry, IPlayerInventory inventory, IWorldProgressionService progression)
        {
            _registry    = registry;
            _inventory   = inventory;
            _progression = progression;
        }

        public void Save()
        {
            var data = new SaveData();

            foreach (var (card, count) in _inventory.OwnedCards)
                for (int i = 0; i < count; i++)
                    data.OwnedCardNames.Add(card.name);

            foreach (var (mat, count) in _inventory.Materials)
                data.OwnedMaterials.Add(new MaterialSaveEntry { MaterialName = mat.name, Count = count });

            foreach (var card in _inventory.ActiveDeck)
                data.ActiveDeckNames.Add(card.name);

            foreach (var name in _progression.DefeatedMonsterNames)
                data.DefeatedMonsters.Add(name);

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

            if (_registry == null)
            {
                Core.GameLog.Warning("SaveService", "GameRegistry introuvable — sauvegarde ignorée");
                return;
            }

            var json = File.ReadAllText(SavePath);
            var data = JsonConvert.DeserializeObject<SaveData>(json);
            if (data == null) return;

            _inventory.Clear();

            foreach (var cardName in data.OwnedCardNames)
            {
                var card = FindCard(cardName);
                if (card != null) _inventory.AddCard(card);
            }

            foreach (var entry in data.OwnedMaterials)
            {
                var mat = FindMaterial(entry.MaterialName);
                if (mat != null) _inventory.AddMaterial(mat, entry.Count);
            }

            foreach (var cardName in data.ActiveDeckNames)
            {
                var card = FindCard(cardName);
                if (card != null) _inventory.AddCardToDeck(card);
            }

            _progression.LoadDefeated(data.DefeatedMonsters);

            Core.GameLog.Info("SaveService", $"Sauvegarde chargée — {data.OwnedCardNames.Count} carte(s), {data.DefeatedMonsters.Count} monstre(s) vaincu(s)");
        }

        private Cards.CardData FindCard(string assetName)
        {
            foreach (var c in _registry.AllCards)
                if (c.name == assetName) return c;
            return null;
        }

        private Inventory.MaterialData FindMaterial(string assetName)
        {
            foreach (var m in _registry.AllMaterials)
                if (m.name == assetName) return m;
            return null;
        }
    }
}
