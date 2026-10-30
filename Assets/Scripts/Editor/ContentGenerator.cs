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
        private const string PlainsCardFramePath = "Assets/Images/Others/SYL_contour_plus_bande.png";

        private static readonly Dictionary<string, SkillData> Skills = new();

        [MenuItem("MonsterCardGame/Contenu/Générer les assets depuis les docs")]
        public static void Generate()
        {
            Skills.Clear();
            GenerateSkills();

            var starters = GenerateStarters();
            var teamA    = GenerateTeamA();
            var patrol   = GeneratePatrol();
            var vanguard = GenerateVanguard();
            var thrak    = GenerateThrak();
            var vaelisse = GenerateVaelisse();

            var encounters = new List<EncounterData>
            {
                Encounter("plaines-patrouille-centaure", "La patrouille centaure", EncounterType.Scenario, patrol),
                Encounter("plaines-avant-garde-de-thrak", "L'avant-garde de Thrâk", EncounterType.Scenario, vanguard),
                // Semi-boss : Thrâk est toujours dépecé
                Encounter("plaines-thrak-le-perpetuel", "Thrâk le Perpétuel", EncounterType.Scenario, thrak, skinnedMonster: thrak[0]),
                // Boss de zone : Vaelisse seule, toujours dépecée
                Encounter("plaines-vaelisse-mere-nuee", "Vaelisse, la Mère-Nuée", EncounterType.Scenario, vaelisse.Take(1), skinnedMonster: vaelisse[0]),
            };

            GenerateZones(scenario: encounters, boss: encounters[^1]);

            var database = GetOrCreate<ContentDatabase>("Assets/Resources", ContentDatabase.ResourcePath);
            database.Init(Skills.Values, starters.Concat(teamA).Concat(patrol).Concat(vanguard).Concat(thrak).Concat(vaelisse), starters, encounters);
            Save(database);

            AssetDatabase.SaveAssets();
            Debug.Log($"[ContentGenerator] {Skills.Count} compétences, {starters.Count} monstres de départ, " +
                      $"équipe de test A et combats des Plaines générés dans {Root}");
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

            // Attaques de base des Essences (docs/competences.md, Essences)
            Active("Entaille sanglante", MonsterRole.Carry, SkillTarget.Enemy, 2, new DamageEffect(10), new BleedEffect(3));
            Active("Frappe de l'ombre", MonsterRole.Carry, SkillTarget.Enemy, 1, new DamageEffect(40), new UntargetableEffect(1).On(EffectTarget.Self));
            Active("Croissance sauvage", MonsterRole.Carry, SkillTarget.Enemy, 0, new DamageEffect(20), new DamageGrowthEffect(5), new MaxHpGrowthEffect(10));
            Active("Marée des abysses", MonsterRole.Support, SkillTarget.AllEnemies, 3, new SlowEffect());
            Active("Souffle de vie", MonsterRole.Support, SkillTarget.Enemy, 0, new DamageEffect(15), new LifeStealEffect());
            Active("Faveur céleste", MonsterRole.Support, SkillTarget.Enemy, 2, new DamageEffect(10), new DamageBonusEffect(10, 3).On(EffectTarget.AllAllies));
            Active("Défi de pierre", MonsterRole.Tank, SkillTarget.Enemy, 0, new DamageEffect(10), new TauntEffect(1).On(EffectTarget.Self));

            // Thrâk le Perpétuel et les allégeances de ses clans
            Passive("Esprit perpétuel", MonsterRole.Tank, SkillTarget.Self, PassiveTrigger.Always, new LeaderEffect(), new PerpetualEffect());
            Special("Esprit perpétuel");
            Passive("Allégeance du Guerrier", MonsterRole.Tank, SkillTarget.None, PassiveTrigger.Always, new AllegianceEffect(AllegianceBonus.DamageBonus, 10));
            Passive("Allégeance du Paladin", MonsterRole.Support, SkillTarget.None, PassiveTrigger.Always, new AllegianceEffect(AllegianceBonus.DamageReduction, 10));
            Passive("Allégeance de l'Archer", MonsterRole.Carry, SkillTarget.None, PassiveTrigger.Always, new AllegianceEffect(AllegianceBonus.IgnoresTaunt));
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

        /// <summary>Compétence spéciale d'un boss ou d'un semi-boss : un monstre du joueur n'en porte qu'une (docs/progression.md).</summary>
        private static void Special(string name)
        {
            var skill = Skills[name];
            if (skill.IsSpecial) return;
            skill.SetSpecial();
            Save(skill);
        }

        private static string Signature(SkillData skill)
            => $"{skill.SkillName}|{skill.Type}|{skill.Role}|{skill.Target}|{skill.Cooldown}|{skill.Trigger}|"
             + string.Join("|", skill.Effects.Select(e => e == null ? "null" : $"{e.GetType().Name}{JsonUtility.ToJson(e)}"));

        // ── Monstres (docs/monstres.md, docs/combats-scenario.md) ─────────

        // Les Essences, fabriquées à la Forge. Les 5 premières forment l'équipe de début de partie, la meilleure selon les simulations (docs/monstres.md)
        private static List<MonsterCardData> GenerateStarters() => new()
        {
            Monster("Essences", "Essence de nature",      MonsterRole.Carry,   180, 0, null, "Croissance sauvage"),
            Monster("Essences", "Essence de vie",         MonsterRole.Support, 160, 0, null, "Souffle de vie"),
            Monster("Essences", "Essence de pierre",      MonsterRole.Tank,    260, 0, null, "Défi de pierre"),
            Monster("Essences", "Essence d'ombre",        MonsterRole.Carry,   100, 0, null, "Frappe de l'ombre"),
            Monster("Essences", "Essence des cieux",      MonsterRole.Support, 160, 0, null, "Faveur céleste"),
            Monster("Essences", "Essence du sang",        MonsterRole.Carry,   160, 0, null, "Entaille sanglante"),
            Monster("Essences", "Essence des profondeurs", MonsterRole.Support, 180, 0, null, "Marée des abysses"),
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

        // Combat 3, semi-boss des Plaines (docs/combats-scenario.md). Thrâk reçoit un bonus de chaque clan présent sur le terrain
        private static List<MonsterCardData> GenerateThrak() => new()
        {
            Monster("Thrak", "Thrâk le Perpétuel", MonsterRole.Tank,    320, 60, null,                               "Esprit perpétuel", "Griffe", "Grand balayage"),
            Monster("Thrak", "Champion guerrier",  MonsterRole.Tank,    240, 40, "Others/SYL_centaure_guerrier.png", "Provocation", "Allégeance du Guerrier"),
            Monster("Thrak", "Champion paladin",   MonsterRole.Support, 180, 40, "Others/SYL_centaure_paladin.png",  "Soin", "Allégeance du Paladin"),
            Monster("Thrak", "Champion archer",    MonsterRole.Carry,   150, 40, "Others/SYL_centaure_archer.png",   "Morsure perforante", "Allégeance de l'Archer"),
            Monster("Thrak", "Guerrier centaure",  MonsterRole.Carry,   160, 40, "CentaurWarrior",                   "Morsure perforante", "Coup de bouclier"),
        };

        /// <summary>
        /// Combat 4, boss des Plaines (docs/combats-scenario.md) : Vaelisse est la seule carte adverse, et sa défaite rapporte 3 points.
        /// Ses invocations tirent au hasard l'un des quatre insectes. Retourne Vaelisse en premier, puis ses insectes.
        /// </summary>
        private static List<MonsterCardData> GenerateVaelisse()
        {
            // Les insectes d'abord : les invocations les référencent
            Active("Nourrir la Reine", MonsterRole.Support, SkillTarget.Ally, 1, new HealEffect(30));
            Active("Piqûre en vol", MonsterRole.Carry, SkillTarget.Enemy, 1, new DamageEffect(20), new UntargetableEffect(1).On(EffectTarget.Self));
            Active("Crachat acide", MonsterRole.Carry, SkillTarget.Enemy, 1, new DamageEffect(10), new BleedEffect(2));

            var insects = new List<MonsterCardData>
            {
                Monster("Vaelisse", "Ouvrière de la nuée", MonsterRole.Support,  60, 0, null, "Nourrir la Reine"),
                Monster("Vaelisse", "Soldat de chitine",   MonsterRole.Tank,    120, 0, null, "Provocation"),
                Monster("Vaelisse", "Dard volant",         MonsterRole.Carry,    50, 0, null, "Piqûre en vol"),
                Monster("Vaelisse", "Cracheur acide",      MonsterRole.Carry,    70, 0, null, "Crachat acide"),
            };

            Active("Ponte", MonsterRole.Support, SkillTarget.None, 0, new SummonEffect(insects, 1, 20, 10));
            Special("Ponte");
            Active("Essaim", MonsterRole.Support, SkillTarget.None, 3, new SummonEffect(insects, 2, 20, 10));
            Special("Essaim");
            Active("Dard de la Reine", MonsterRole.Carry, SkillTarget.Enemy, 1, new DamageEffect(20));

            var vaelisse = Monster("Vaelisse", "Vaelisse, la Mère-Nuée", MonsterRole.Tank, 650, 80, null, defeatPoints: 3,
                                   "Ponte", "Essaim", "Dard de la Reine");
            return new[] { vaelisse }.Concat(insects).ToList();
        }

        private static List<MonsterCardData> GeneratePatrol() => new()
        {
            Monster("Patrouille", "Éclaireur centaure", MonsterRole.Carry,   140, 20, null, "Griffe"),
            Monster("Patrouille", "Archer novice",      MonsterRole.Carry,   120, 20, null, "Morsure perforante"),
            Monster("Patrouille", "Jeune taureau",      MonsterRole.Tank,    200, 20, null, "Provocation", "Carapace"),
            Monster("Patrouille", "Porte-étendard",     MonsterRole.Support, 140, 20, null, "Aura apaisante"),
            Monster("Patrouille", "Cavalier rabatteur", MonsterRole.Carry,   160, 20, null, "Griffe", "Coup de bouclier"),
        };

        // Combat 2 (docs/combats-scenario.md). Les illustrations SYL_ sont des PNG de Images/Others
        private static List<MonsterCardData> GenerateVanguard() => new()
        {
            Monster("AvantGarde", "Guerrier du clan",  MonsterRole.Tank,    220, 30, "Others/SYL_centaure_guerrier.png", "Provocation", "Griffe"),
            Monster("AvantGarde", "Archer du clan",    MonsterRole.Carry,   140, 30, "Others/SYL_centaure_archer.png",   "Morsure perforante"),
            Monster("AvantGarde", "Paladin du clan",   MonsterRole.Support, 160, 30, "Others/SYL_centaure_paladin.png",  "Soin"),
            Monster("AvantGarde", "Lancier du clan",   MonsterRole.Carry,   180, 30, null,                               "Grand balayage"),
            Monster("AvantGarde", "Éclaireur vétéran", MonsterRole.Carry,   150, 30, null,                               "Griffe"),
        };

        private static MonsterCardData Monster(string folder, string name, MonsterRole role, int hp, int boostHp,
                                               string artwork, params string[] skills)
            => Monster(folder, name, role, hp, boostHp, artwork, defeatPoints: 1, skills);

        private static MonsterCardData Monster(string folder, string name, MonsterRole role, int hp, int boostHp,
                                               string artwork, int defeatPoints, params string[] skills)
        {
            var card = GetOrCreate<MonsterCardData>($"{Root}/Monsters/{folder}", name);
            // L'illustration et le cadre réglés à la main dans l'asset sont gardés : le générateur ne donne
            // que ceux qui manquent. Les Essences ont le cadre de la Forge (vide) ; tous les adversaires actuels viennent des Plaines
            var art = card.Artwork != null ? card.Artwork : LoadArtwork(artwork);
            var frame = card.CardFrame != null ? card.CardFrame
                      : folder == "Essences" ? null
                      : AssetDatabase.LoadAllAssetsAtPath(PlainsCardFramePath).OfType<Sprite>().FirstOrDefault();
            card.Init(name, role, hp, skills.Select(s => Skills[s]), new StatBoost(boostHp), artwork: art, cardFrame: frame, defeatPoints: defeatPoints);
            Save(card);
            return card;
        }

        private static Sprite LoadArtwork(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return null;
            // Un nom simple désigne un .jpg de CardSprites ; un chemin avec extension part de Assets/Images
            var path = Path.HasExtension(fileName) ? $"Assets/Images/{fileName}" : $"{ArtworkFolder}/{fileName}.jpg";
            var sprite = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().FirstOrDefault();
            if (sprite == null) Debug.LogWarning($"[ContentGenerator] Illustration introuvable : {path}");
            return sprite;
        }

        private static EncounterData Encounter(string id, string displayName, EncounterType type, IEnumerable<MonsterCardData> monsters,
                                               MonsterCardData skinnedMonster = null)
        {
            var encounter = GetOrCreate<EncounterData>($"{Root}/Encounters", id);
            encounter.Init(id, displayName, type, monsters, skinnedMonster);
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
        private static void GenerateZones(IEnumerable<EncounterData> scenario, EncounterData boss)
        {
            var plains = AssetDatabase.LoadAssetAtPath<ZoneData>($"{ZoneFolder}/Plains.asset");
            if (plains == null)
            {
                Debug.LogWarning($"[ContentGenerator] Zone introuvable : {ZoneFolder}/Plains.asset");
                return;
            }

            // Vaelisse, boss des Plaines : sa défaite compte pour les Ruines. Pas encore de combat libre dans les Plaines.
            plains.SetEncounters(scenario, System.Array.Empty<EncounterData>(), boss);
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
