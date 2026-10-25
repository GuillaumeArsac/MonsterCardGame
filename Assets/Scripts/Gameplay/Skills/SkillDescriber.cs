using System.Collections.Generic;
using System.Linq;
using MonsterCardGame.Core;
using MonsterCardGame.Gameplay.Skills.Effects;

namespace MonsterCardGame.Gameplay.Skills
{
    /// <summary>
    /// Texte d'une compétence pour le joueur, construit à partir de ses effets : il reste juste quand une compétence change.
    /// Formulations reprises de docs/competences.md.
    /// </summary>
    public static class SkillDescriber
    {
        /// <summary>Type et rythme : « Active · recharge 3 », « Passive · au début du combat » ou « Passive spéciale · toujours ».</summary>
        public static string Summary(SkillData skill)
        {
            string special = skill.IsSpecial ? " spéciale" : "";
            return skill.IsActive
                ? $"Active{special} · {CooldownText(skill.Cooldown)}"
                : $"Passive{special} · {TriggerText(skill.Trigger)}";
        }

        public static string TargetText(SkillTarget target) => target switch
        {
            SkillTarget.AnyMonster => "un monstre sur le terrain, allié ou ennemi",
            SkillTarget.Enemy      => "un ennemi sur le terrain",
            SkillTarget.Ally       => "un allié sur le terrain",
            SkillTarget.OtherAlly  => "un autre allié sur le terrain",
            SkillTarget.Self       => "ce monstre",
            SkillTarget.AllEnemies => "tous les ennemis sur le terrain",
            SkillTarget.AllAllies  => "tous les alliés sur le terrain, y compris ce monstre",
            _                      => "aucune"
        };

        /// <summary>Ce que fait la compétence, en une ou deux phrases.</summary>
        public static string Effect(SkillData skill)
        {
            var parts = skill.Effects.Where(e => e != null).Select(EffectText).Where(t => t.Length > 0).ToList();
            if (parts.Count == 0) return "Aucun effet.";
            return Capitalize(JoinFrench(parts)) + ".";
        }

        /// <summary>Précision sur le ciblage, quand elle compte (Provocation).</summary>
        public static string TargetingNote(SkillData skill)
        {
            if (!skill.IsActive) return "";
            return skill.Target switch
            {
                SkillTarget.AllEnemies => "Ne choisit pas de cible : ignore la Provocation.",
                SkillTarget.AnyMonster => "Si elle vise un ennemi, elle est soumise à la Provocation.",
                SkillTarget.Enemy      => "Soumise à la Provocation.",
                _                      => ""
            };
        }

        /// <summary>Description complète : résumé, cible, effet et précision de ciblage, une par ligne.</summary>
        public static string FullText(SkillData skill)
        {
            var lines = new List<string> { Summary(skill) };
            if (skill.Target != SkillTarget.None && skill.Target != SkillTarget.Self)
                lines.Add($"Cible : {TargetText(skill.Target)}");
            lines.Add(Effect(skill));
            var note = TargetingNote(skill);
            if (note.Length > 0) lines.Add(note);
            return string.Join("\n", lines);
        }

        // ── Effets ────────────────────────────────────────────────────────

        private static string EffectText(SkillEffect effect)
        {
            // Effet appliqué à d'autres monstres que les cibles de la compétence
            string who = effect.AppliesTo switch
            {
                EffectTarget.Self      => "ce monstre",
                EffectTarget.AllAllies => "tous les alliés sur le terrain",
                _                      => null
            };
            string to = who != null ? $" à {who}" : "";
            string by = who != null ? $" par {who}" : "";

            return effect switch
            {
                DamageEffect d          => $"inflige {Plural(d.Amount, "dégât")}{to}" + (d.IgnoresReductions ? ", qui ignorent les réductions de dégâts" : ""),
                HealEffect h            => $"rend {h.Amount} PV{to}, sans dépasser les PV de départ (à un monstre qui saigne, retire ses saignements au lieu de le soigner)",
                DamageBonusEffect b     => $"les dégâts infligés{by} augmentent de {b.Amount} {DurationText(b.DurationTurns)}",
                DamageReductionEffect r => $"les dégâts subis{by} sont réduits de {r.Amount}, sans descendre sous 0, {DurationText(r.DurationTurns)}",
                SlowEffect              => "applique Ralentissement : +1 au compteur de toutes ses compétences actives, même disponibles",
                ExecuteEffect x         => $"si la cible a {x.HpThreshold} PV ou moins, elle est vaincue ; sinon, inflige {Plural(x.FallbackDamage, "dégât")}",
                ReturnToHandEffect      => "renvoie la cible dans la main : elle garde ses PV et perd ses effets temporaires, sans point marqué",
                TauntEffect t when t.DurationTurns > 0
                                        => $"{who ?? "la cible"} a Provocation {DurationText(t.DurationTurns)}",
                TauntEffect             => "ce monstre a Provocation : tant qu'il est sur le terrain, les compétences adverses qui visent un ennemi doivent le cibler",
                BleedEffect s           => $"applique {Plural(s.Stacks, "saignement")}{to} : au début de chaque phase de son camp, le monstre perd "
                                         + $"{GameRules.BleedPercentPerStack} % de ses PV max par saignement, jusqu'à un soin",
                UntargetableEffect u    => $"{who ?? "la cible"} ne peut plus être ciblé par l'adversaire {DurationText(u.DurationTurns)} "
                                         + "(les compétences sans cible le touchent quand même)",
                DamageGrowthEffect g    => $"les dégâts de cette compétence augmentent ensuite de {g.Amount} pour le reste du combat",
                MaxHpGrowthEffect p     => $"ce monstre gagne ensuite {p.Amount} PV pour le reste du combat, PV de départ compris",
                LeaderEffect            => "ce monstre est le chef des clans : il reçoit les bonus d'allégeance de ses alliés sur le terrain",
                PerpetualEffect         => "chaque fois que l'adversaire marque un point, ce monstre, s'il est sur le terrain, récupère tous ses PV et perd ses saignements",
                AllegianceEffect a      => "tant que ce monstre est sur le terrain, le chef des clans allié " + a.Bonus switch
                {
                    AllegianceBonus.DamageBonus     => $"inflige {a.Amount} dégâts de plus",
                    AllegianceBonus.DamageReduction => $"subit {a.Amount} dégâts de moins",
                    _                               => "ignore la Provocation"
                },
                LifeStealEffect         => "l'allié le plus blessé sur le terrain récupère autant de PV que les dégâts infligés (s'il saigne, ses saignements sont retirés à la place)",
                MarkedSlotEffect m      => $"marque un emplacement adverse tiré au hasard : un monstre adverse vaincu sur cet emplacement rapporte {Plural(m.BonusPoints, "point")} de plus",
                _                       => ""
            };
        }

        private static string DurationText(int turns) => turns <= 0 ? "en permanence" : $"pendant {Plural(turns, "tour")}";

        private static string CooldownText(int cooldown) => cooldown switch
        {
            0 => "recharge 0 : utilisable à chaque phase",
            1 => "recharge 1 : à chaque phase, sauf celle où le monstre est posé",
            _ => $"recharge {cooldown} : une phase sur {cooldown}"
        };

        private static string TriggerText(PassiveTrigger trigger) => trigger switch
        {
            PassiveTrigger.StartOfOwnPhase => "au début de chaque phase de ce monstre",
            PassiveTrigger.StartOfCombat   => "au début du combat",
            _                              => "toujours active"
        };

        private static string Plural(int count, string word) => $"{count} {word}{(count > 1 ? "s" : "")}";

        private static string JoinFrench(IReadOnlyList<string> parts)
            => parts.Count == 1 ? parts[0] : string.Join(", ", parts.Take(parts.Count - 1)) + " et " + parts[parts.Count - 1];

        private static string Capitalize(string text) => text.Length == 0 ? text : char.ToUpper(text[0]) + text.Substring(1);
    }
}
