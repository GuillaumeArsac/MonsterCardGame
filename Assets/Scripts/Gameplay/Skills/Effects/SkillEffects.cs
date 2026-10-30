using System;
using System.Collections.Generic;
using UnityEngine;
using MonsterCardGame.Gameplay.Monsters;

namespace MonsterCardGame.Gameplay.Skills.Effects
{
    // Briques d'effet du catalogue de docs/competences.md.
    // Les durées sont en tours : un effet de X tours dure jusqu'à la fin de la X-ième phase de l'autre camp.

    /// <summary>Inflige des dégâts à chaque cible (Griffe, Morsure perforante, Grand balayage, Coup de bouclier).</summary>
    [Serializable]
    public class DamageEffect : SkillEffect
    {
        [SerializeField, Min(0)] private int _amount = 10;

        [SerializeField, Tooltip("Si vrai, les réductions de dégâts ne s'appliquent pas (les bonus s'appliquent normalement)")]
        private bool _ignoresReductions;

        public int  Amount            => _amount;
        public bool IgnoresReductions => _ignoresReductions;

        public DamageEffect() { }
        public DamageEffect(int amount, bool ignoresReductions = false)
        {
            _amount            = amount;
            _ignoresReductions = ignoresReductions;
        }
    }

    /// <summary>Rend des PV à chaque cible, sans dépasser ses PV de départ (Soin, Aura apaisante). Sur une cible qui saigne, retire les saignements au lieu de soigner.</summary>
    [Serializable]
    public class HealEffect : SkillEffect
    {
        [SerializeField, Min(0)] private int _amount = 10;

        public int Amount => _amount;

        public HealEffect() { }
        public HealEffect(int amount) => _amount = amount;
    }

    /// <summary>Augmente les dégâts infligés par chaque cible (Bénédiction).</summary>
    [Serializable]
    public class DamageBonusEffect : SkillEffect
    {
        [SerializeField, Min(0)] private int _amount = 10;

        [SerializeField, Min(0), Tooltip("Durée en tours. 0 = permanent")]
        private int _durationTurns;

        public int  Amount        => _amount;
        public int  DurationTurns => _durationTurns;
        public bool IsPermanent   => _durationTurns == 0;

        public DamageBonusEffect() { }
        public DamageBonusEffect(int amount, int durationTurns)
        {
            _amount        = amount;
            _durationTurns = durationTurns;
        }
    }

    /// <summary>Réduit les dégâts subis par chaque cible, sans descendre sous 0 (Bouclier, Carapace).</summary>
    [Serializable]
    public class DamageReductionEffect : SkillEffect
    {
        [SerializeField, Min(0)] private int _amount = 10;

        [SerializeField, Min(0), Tooltip("Durée en tours. 0 = permanent")]
        private int _durationTurns;

        public int  Amount        => _amount;
        public int  DurationTurns => _durationTurns;
        public bool IsPermanent   => _durationTurns == 0;

        public DamageReductionEffect() { }
        public DamageReductionEffect(int amount, int durationTurns)
        {
            _amount        = amount;
            _durationTurns = durationTurns;
        }
    }

    /// <summary>
    /// Ralentissement : le compteur de toutes les compétences actives de la cible augmente de 1,
    /// y compris celles qui sont disponibles. S'applique même si les dégâts sont réduits à 0.
    /// </summary>
    [Serializable]
    public class SlowEffect : SkillEffect
    {
    }

    /// <summary>Si la cible a au plus le seuil de PV, elle est vaincue. Sinon, elle subit des dégâts (Exécution).</summary>
    [Serializable]
    public class ExecuteEffect : SkillEffect
    {
        [SerializeField, Min(0), Tooltip("La cible est vaincue si ses PV sont inférieurs ou égaux à ce seuil")]
        private int _hpThreshold = 80;

        [SerializeField, Min(0), Tooltip("Dégâts infligés si la cible est au-dessus du seuil")]
        private int _fallbackDamage = 10;

        public int HpThreshold    => _hpThreshold;
        public int FallbackDamage => _fallbackDamage;

        public ExecuteEffect() { }
        public ExecuteEffect(int hpThreshold, int fallbackDamage)
        {
            _hpThreshold    = hpThreshold;
            _fallbackDamage = fallbackDamage;
        }
    }

    /// <summary>
    /// Renvoie la cible dans la main (Repli). Elle garde ses PV actuels et perd ses effets temporaires.
    /// Ce n'est pas une défaite : aucun point n'est marqué.
    /// </summary>
    [Serializable]
    public class ReturnToHandEffect : SkillEffect
    {
    }

    /// <summary>
    /// Le monstre a Provocation : les compétences adverses qui visent un ennemi doivent le cibler.
    /// En passive (Provocation), c'est permanent. Dans une compétence active (Essence de pierre), pour la durée indiquée.
    /// </summary>
    [Serializable]
    public class TauntEffect : SkillEffect
    {
        [SerializeField, Min(0), Tooltip("Durée en tours, pour une compétence active. 0 = permanent")]
        private int _durationTurns;

        public int DurationTurns => _durationTurns;

        public TauntEffect() { }
        public TauntEffect(int durationTurns) => _durationTurns = durationTurns;
    }

    /// <summary>
    /// Saignement (Essence du sang) : au début de chaque phase de son camp, la cible subit, par cumul,
    /// un pourcentage de ses PV max (GameRules.BleedPercentPerStack), sans réduction. Les cumuls s'additionnent.
    /// Un soin retire tous les saignements au lieu de rendre des PV.
    /// </summary>
    [Serializable]
    public class BleedEffect : SkillEffect
    {
        [SerializeField, Min(1)] private int _stacks = 1;

        public int Stacks => _stacks;

        public BleedEffect() { }
        public BleedEffect(int stacks) => _stacks = stacks;
    }

    /// <summary>
    /// La cible ne peut plus être choisie par les compétences adverses (Essence de l'ombre).
    /// Les compétences qui ne choisissent pas de cible (Grand balayage) la touchent quand même.
    /// </summary>
    [Serializable]
    public class UntargetableEffect : SkillEffect
    {
        [SerializeField, Min(0), Tooltip("Durée en tours. 0 = permanent")]
        private int _durationTurns = 1;

        public int DurationTurns => _durationTurns;

        public UntargetableEffect() { }
        public UntargetableEffect(int durationTurns) => _durationTurns = durationTurns;
    }

    /// <summary>
    /// Après chaque utilisation, les dégâts de cette compétence augmentent pour le reste du combat (Essence de nature).
    /// S'applique au monstre qui l'utilise, quelle que soit la cible.
    /// </summary>
    [Serializable]
    public class DamageGrowthEffect : SkillEffect
    {
        [SerializeField, Min(0)] private int _amount = 5;

        public int Amount => _amount;

        public DamageGrowthEffect() { }
        public DamageGrowthEffect(int amount) => _amount = amount;
    }

    /// <summary>
    /// Après chaque utilisation, ce monstre gagne des PV pour le reste du combat : ses PV de départ et ses PV actuels
    /// augmentent d'autant (Essence de nature). S'applique au monstre qui l'utilise, quelle que soit la cible.
    /// </summary>
    [Serializable]
    public class MaxHpGrowthEffect : SkillEffect
    {
        [SerializeField, Min(0)] private int _amount = 10;

        public int Amount => _amount;

        public MaxHpGrowthEffect() { }
        public MaxHpGrowthEffect(int amount) => _amount = amount;
    }

    /// <summary>
    /// Vol de vie (Essence de vie) : l'allié le plus blessé sur le terrain récupère autant de PV que les dégâts
    /// infligés par les effets précédents de la compétence. C'est un soin : sur un allié qui saigne, il retire les saignements.
    /// </summary>
    [Serializable]
    public class LifeStealEffect : SkillEffect
    {
    }

    /// <summary>
    /// Chef des clans (Thrâk le Perpétuel) : ce monstre reçoit les bonus d'allégeance des alliés sur le terrain.
    /// </summary>
    [Serializable]
    public class LeaderEffect : SkillEffect
    {
    }

    /// <summary>
    /// Perpétuel (Thrâk) : chaque fois que l'adversaire marque un point, ce monstre, s'il est sur le terrain,
    /// récupère tous ses PV et perd ses saignements. Ce n'est pas un soin.
    /// </summary>
    [Serializable]
    public class PerpetualEffect : SkillEffect
    {
    }

    public enum AllegianceBonus
    {
        /// <summary>Le chef inflige plus de dégâts.</summary>
        DamageBonus,
        /// <summary>Le chef subit moins de dégâts.</summary>
        DamageReduction,
        /// <summary>Les compétences du chef ignorent la Provocation.</summary>
        IgnoresTaunt
    }

    /// <summary>
    /// Allégeance (clans de Thrâk) : tant que ce monstre est sur le terrain, le chef allié (LeaderEffect) reçoit ce bonus.
    /// Les bonus de plusieurs alliés se cumulent.
    /// </summary>
    [Serializable]
    public class AllegianceEffect : SkillEffect
    {
        [SerializeField] private AllegianceBonus _bonus;
        [SerializeField, Min(0), Tooltip("Valeur du bonus. Sans effet pour IgnoresTaunt")]
        private int _amount = 10;

        public AllegianceBonus Bonus  => _bonus;
        public int             Amount => _amount;

        public AllegianceEffect() { }
        public AllegianceEffect(AllegianceBonus bonus, int amount = 0)
        {
            _bonus  = bonus;
            _amount = amount;
        }
    }

    /// <summary>
    /// Invocation (Vaelisse) : pose des monstres tirés au hasard dans la liste, sur les emplacements libres de ce camp.
    /// Un monstre invoqué ne rapporte aucun point quand il est vaincu, et n'agit qu'à partir de la phase suivante de son camp.
    /// Sans emplacement libre, chaque invocation renforce à la place un monstre invoqué allié tiré au hasard
    /// (PV de départ et PV actuels, et dégâts infligés, pour le reste du combat).
    /// </summary>
    [Serializable]
    public class SummonEffect : SkillEffect
    {
        [SerializeField, Tooltip("Monstres possibles, tirés au hasard à chaque invocation")]
        private List<MonsterCardData> _pool = new();

        [SerializeField, Min(1), Tooltip("Nombre d'invocations")]
        private int _count = 1;

        [SerializeField, Min(0), Tooltip("Terrain plein : PV gagnés par le monstre invoqué renforcé")]
        private int _reinforceHp = 20;

        [SerializeField, Min(0), Tooltip("Terrain plein : dégâts infligés en plus par le monstre invoqué renforcé")]
        private int _reinforceDamage = 10;

        public IReadOnlyList<MonsterCardData> Pool            => _pool;
        public int                            Count           => _count;
        public int                            ReinforceHp     => _reinforceHp;
        public int                            ReinforceDamage => _reinforceDamage;

        public SummonEffect() { }
        public SummonEffect(IEnumerable<MonsterCardData> pool, int count, int reinforceHp, int reinforceDamage)
        {
            _pool            = new List<MonsterCardData>(pool);
            _count           = count;
            _reinforceHp     = reinforceHp;
            _reinforceDamage = reinforceDamage;
        }
    }

    /// <summary>
    /// Marque un emplacement adverse tiré au hasard (Chasseur de trophées) : un monstre adverse vaincu
    /// sur cet emplacement rapporte des points en plus, fatigue comprise.
    /// </summary>
    [Serializable]
    public class MarkedSlotEffect : SkillEffect
    {
        [SerializeField, Min(0)] private int _bonusPoints = 1;

        public int BonusPoints => _bonusPoints;

        public MarkedSlotEffect() { }
        public MarkedSlotEffect(int bonusPoints) => _bonusPoints = bonusPoints;
    }
}
