using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using MonsterCardGame.Gameplay.Content;
using MonsterCardGame.Gameplay.Encounters;
using MonsterCardGame.Gameplay.Monsters;
using MonsterCardGame.Gameplay.Skills;
using MonsterCardGame.Gameplay.Skills.Effects;
using MonsterCardGame.Gameplay.World;

namespace MonsterCardGame.Editor
{
    /// <summary>
    /// Génère les assets du contenu décrit dans les docs : compétences (docs/competences.md), monstres de départ
    /// et équipes de test (docs/monstres.md), patrouille centaure (docs/combats-scenario.md).
    /// Relancer le menu met à jour les assets existants sans changer leur GUID : les références restent valides.
    /// </summary>
    public static class ContentGenerator
    {
        private const string Root = "Assets/Data";
        private const string ArtworkFolder = "Assets/Images/CardSprites";

        private static readonly Dictionary<string, SkillData> Skills = new();

        [MenuItem("MonsterCardGame/Contenu/Générer les assets depuis les docs")]
        public static void Generate()
        {
            Skills.Clear();
            GenerateSkills();

            var starters = GenerateStarters();
            var teamA    = GenerateTeamA();
            var teamB    = GenerateTeamB();
            var patrol   = GeneratePatrol();

            var encounters = new List<EncounterData>
            {
                Encounter("plaines-patrouille-centaure", "La patrouille centaure", EncounterType.Scenario, patrol),
                Encounter("test-horde-de-thrak", "La horde de Thrâk (équipe de test B)", EncounterType.Free, teamB),
            };

            GenerateZones(patrol: encounters[0], horde: encounters[1]);

            var database = GetOrCreate<ContentDatabase>("Assets/Resources", ContentDatabase.ResourcePath);
            database.Init(Skills.Values, starters.Concat(teamA).Concat(teamB).Concat(patrol), starters, encounters);
            Save(database);

            AssetDatabase.SaveAssets();
            Debug.Log($"[ContentGenerator] {Skills.Count} compétences, {starters.Count} monstres de départ, " +
                      $"équipes A et B, patrouille centaure générés dans {Root}");
        }

        public static IReadOnlyList<MonsterCardData> LoadTeam(string folder)
            => AssetDatabase.FindAssets("t:MonsterCardData", new[] { $"{Root}/Monsters/{folder}" })
                            .Select(guid => AssetDatabase.LoadAssetAtPath<MonsterCardData>(AssetDatabase.GUIDToAssetPath(guid)))
                            .OrderBy(m => m.name)
                            .ToList();

        // ── Compétences (docs/competences.md) ─────────────────────────────

        private static void GenerateSkills()
        {
            Active("Grand balayage", MonsterRole.Tank, SkillTarget.AllEnemies, 3, new DamageEffect(10), new SlowEffect());
            Active("Griffe", MonsterRole.Carry, SkillTarget.AnyMonster, 0, new DamageEffect(30));
            Active("Soin", MonsterRole.Support, SkillTarget.AnyMonster, 2, new HealEffect(40));
            Active("Bénédiction", MonsterRole.Support, SkillTarget.AllAllies, 3, new DamageBonusEffect(10, 6));
            Active("Morsure perforante", MonsterRole.Carry, SkillTarget.AnyMonster, 0, new DamageEffect(20, ignoresReductions: true));
            Active("Exécution", MonsterRole.Carry, SkillTarget.AnyMonster, 4, new ExecuteEffect(80, 10));
            Passive("Chasseur de trophées", MonsterRole.Carry, SkillTarget.None, PassiveTrigger.StartOfCombat, new MarkedSlotEffect(1));
            Active("Bouclier", MonsterRole.Support, SkillTarget.AnyMonster, 3, new DamageReductionEffect(20, 2));
            Passive("Aura apaisante", MonsterRole.Support, SkillTarget.AllAllies, PassiveTrigger.StartOfOwnPhase, new HealEffect(10));
            Active("Repli", MonsterRole.Support, SkillTarget.OtherAlly, 4, new ReturnToHandEffect());
            Passive("Provocation", MonsterRole.Tank, SkillTarget.Self, PassiveTrigger.Always, new TauntEffect());
            Passive("Carapace", MonsterRole.Tank, SkillTarget.Self, PassiveTrigger.Always, new DamageReductionEffect(10, 0));
            Active("Coup de bouclier", MonsterRole.Tank, SkillTarget.AnyMonster, 3, new DamageEffect(10), new SlowEffect());
        }

        private static void Active(string name, MonsterRole role, SkillTarget target, int cooldown, params SkillEffect[] effects)
            => Skill(name, SkillType.Active, role, target, cooldown, PassiveTrigger.Always, effects);

        private static void Passive(string name, MonsterRole role, SkillTarget target, PassiveTrigger trigger, params SkillEffect[] effects)
            => Skill(name, SkillType.Passive, role, target, 0, trigger, effects);

        private static void Skill(string name, SkillType type, MonsterRole role, SkillTarget target, int cooldown,
                                  PassiveTrigger trigger, SkillEffect[] effects)
        {
            var skill = GetOrCreate<SkillData>($"{Root}/Skills", name);

            // Réécrire les effets change leurs identifiants internes (rid) : on ne touche pas une compétence inchangée
            var expected = ScriptableObject.CreateInstance<SkillData>();
            expected.Init(name, type, role, target, cooldown, trigger, effects);
            if (Signature(skill) != Signature(expected))
            {
                skill.Init(name, type, role, target, cooldown, trigger, effects);
                Save(skill);
            }
            Object.DestroyImmediate(expected);

            Skills[name] = skill;
        }

        private static string Signature(SkillData skill)
            => $"{skill.SkillName}|{skill.Type}|{skill.Role}|{skill.Target}|{skill.Cooldown}|{skill.Trigger}|"
             + string.Join("|", skill.Effects.Select(e => e == null ? "null" : $"{e.GetType().Name}{JsonUtility.ToJson(e)}"));

        // ── Monstres (docs/monstres.md, docs/combats-scenario.md) ─────────

        // Les 5 premiers forment l'équipe de début de partie : la meilleure selon les simulations (docs/combats-scenario.md)
        private static List<MonsterCardData> GenerateStarters() => new()
        {
            Monster("Depart", "Loup des cendres",     MonsterRole.Carry,   150, 0, null, "Griffe"),
            Monster("Depart", "Renard de braise",     MonsterRole.Carry,   140, 0, null, "Morsure perforante"),
            Monster("Depart", "Ours brun",            MonsterRole.Carry,   200, 0, null, "Griffe"),
            Monster("Depart", "Chouette guérisseuse", MonsterRole.Support, 160, 0, null, "Soin"),
            Monster("Depart", "Chien de garde",       MonsterRole.Tank,    260, 0, null, "Provocation"),
            Monster("Depart", "Luciole sacrée",       MonsterRole.Support, 160, 0, null, "Bénédiction"),
            Monster("Depart", "Sanglier",             MonsterRole.Tank,    240, 0, null, "Coup de bouclier"),
        };

        // Les docs ne donnent pas de boost de stats aux équipes de test : 0 en attendant.
        private static List<MonsterCardData> GenerateTeamA() => new()
        {
            Monster("EquipeA", "Griffon",          MonsterRole.Carry,   160, 0, "Griffin",         "Griffe", "Exécution"),
            Monster("EquipeA", "Archer centaure",  MonsterRole.Carry,   140, 0, "Centaur Archer",  "Morsure perforante", "Chasseur de trophées"),
            Monster("EquipeA", "Orbe de lumière",  MonsterRole.Support, 180, 0, "Light Orb",       "Soin", "Bénédiction", "Aura apaisante"),
            Monster("EquipeA", "Svathurim",        MonsterRole.Support, 200, 0, "Svathurim",       "Griffe", "Bouclier", "Repli"),
            Monster("EquipeA", "Paladin centaure", MonsterRole.Tank,    280, 0, "Centaur Paladin", "Provocation", "Carapace", "Coup de bouclier"),
        };

        private static List<MonsterCardData> GenerateTeamB() => new()
        {
            Monster("EquipeB", "Thrâk le Perpétuel",  MonsterRole.Tank,    260, 0, null,               "Provocation", "Griffe", "Grand balayage"),
            Monster("EquipeB", "Aigle Runique Géant", MonsterRole.Carry,   180, 0, "Giant Rune Eagle", "Griffe", "Exécution", "Chasseur de trophées"),
            Monster("EquipeB", "Guerrier centaure",   MonsterRole.Carry,   160, 0, "CentaurWarrior",   "Morsure perforante", "Coup de bouclier"),
            Monster("EquipeB", "Chaman centaure",     MonsterRole.Support, 160, 0, null,               "Griffe", "Soin", "Bénédiction"),
            Monster("EquipeB", "Taureau de guerre",   MonsterRole.Tank,    300, 0, null,               "Carapace", "Griffe", "Coup de bouclier"),
        };

        private static List<MonsterCardData> GeneratePatrol() => new()
        {
            Monster("Patrouille", "Éclaireur centaure", MonsterRole.Carry,   140, 20, null, "Griffe"),
            Monster("Patrouille", "Archer novice",      MonsterRole.Carry,   120, 20, null, "Morsure perforante"),
            Monster("Patrouille", "Jeune taureau",      MonsterRole.Tank,    200, 20, null, "Provocation", "Carapace"),
            Monster("Patrouille", "Porte-étendard",     MonsterRole.Support, 140, 20, null, "Aura apaisante"),
            Monster("Patrouille", "Cavalier rabatteur", MonsterRole.Carry,   160, 20, null, "Griffe", "Coup de bouclier"),
        };

        private static MonsterCardData Monster(string folder, string name, MonsterRole role, int hp, int boostHp,
                                               string artwork, params string[] skills)
        {
            var card = GetOrCreate<MonsterCardData>($"{Root}/Monsters/{folder}", name);
            card.Init(name, role, hp, skills.Select(s => Skills[s]), new StatBoost(boostHp), artwork: LoadArtwork(artwork));
            Save(card);
            return card;
        }

        private static Sprite LoadArtwork(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return null;
            var path = $"{ArtworkFolder}/{fileName}.jpg";
            var sprite = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().FirstOrDefault();
            if (sprite == null) Debug.LogWarning($"[ContentGenerator] Illustration introuvable : {path}");
            return sprite;
        }

        private static EncounterData Encounter(string id, string displayName, EncounterType type, IEnumerable<MonsterCardData> monsters)
        {
            var encounter = GetOrCreate<EncounterData>($"{Root}/Encounters", id);
            encounter.Init(id, displayName, type, monsters);
            Save(encounter);
            return encounter;
        }

        // ── Zones (docs/scenario/zones) ───────────────────────────────────

        private const string ZoneFolder = Root + "/Zone";
        private const string WorldDataPath = "Assets/Resources/WorldData.asset";

        /// <summary>
        /// Combats des zones. L'identité des zones (nom, illustration, position) se règle à la main dans leurs assets :
        /// seules les Ruines sont créées si elles manquent.
        /// </summary>
        private static void GenerateZones(EncounterData patrol, EncounterData horde)
        {
            var plains = AssetDatabase.LoadAssetAtPath<ZoneData>($"{ZoneFolder}/Plains.asset");
            if (plains == null)
            {
                Debug.LogWarning($"[ContentGenerator] Zone introuvable : {ZoneFolder}/Plains.asset");
                return;
            }

            // Vaelisse, boss des Plaines, n'est pas encore conçue : sans boss, les Ruines restent verrouillées.
            // La horde de Thrâk (équipe de test B) sert de combat libre en attendant les vrais combats libres.
            plains.SetEncounters(new[] { patrol }, new[] { horde }, null);
            Save(plains);

            var ruins = AssetDatabase.LoadAssetAtPath<ZoneData>($"{ZoneFolder}/Ruins.asset");
            if (ruins == null)
            {
                ruins = ScriptableObject.CreateInstance<ZoneData>();
                ruins.InitIdentity("Les Ruines Oubliées", "Zone finale. La vérité sur l'origine de tout. S'ouvre quand les boss des autres zones sont vaincus.",
                                   new Vector2(0.78f, 0.28f), requiresAllOtherBossesDefeated: true);
                AssetDatabase.CreateAsset(ruins, $"{ZoneFolder}/Ruins.asset");
            }

            var world = AssetDatabase.LoadAssetAtPath<WorldData>(WorldDataPath);
            if (world != null && !world.Zones.Contains(ruins))
            {
                world.SetZones(world.Zones.Append(ruins));
                Save(world);
            }
        }

        // ── Outils ────────────────────────────────────────────────────────

        private static T GetOrCreate<T>(string folder, string name) where T : ScriptableObject
        {
            EnsureFolder(folder);
            var path  = $"{folder}/{name}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void Save(Object asset) => EditorUtility.SetDirty(asset);

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder)) return;
            var parent = Path.GetDirectoryName(folder)?.Replace('\\', '/');
            if (!string.IsNullOrEmpty(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(folder));
        }
    }
}
