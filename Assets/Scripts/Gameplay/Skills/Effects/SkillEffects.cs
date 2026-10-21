using System;
using UnityEngine;

namespace MonsterCardGame.Gameplay.Skills.Effects
{
    // Briques d'effet du catalogue de docs/competences.md.
    // Les durées sont en tours : un effet de X tours dure jusqu'à la fin de la X-ième phase de l'autre camp.

    /// <summary>Inflige des dégâts à chaque cible (Griffe, Morsure perforante, Grand balayage, Coup de bouclier).</summary>
    [Serializable]
    public class DamageEffect : SkillEffect
    {
        [SerializeField, Min(0)] private int _amount = 1;

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

    /// <summary>Rend des PV à chaque cible, sans dépasser ses PV de départ (Soin, Aura apaisante).</summary>
    [Serializable]
    public class HealEffect : SkillEffect
    {
        [SerializeField, Min(0)] private int _amount = 1;

        public int Amount => _amount;

        public HealEffect() { }
        public HealEffect(int amount) => _amount = amount;
    }

    /// <summary>Augmente les dégâts infligés par chaque cible (Bénédiction).</summary>
    [Serializable]
    public class DamageBonusEffect : SkillEffect
    {
        [SerializeField, Min(0)] private int _amount = 1;

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
        [SerializeField, Min(0)] private int _amount = 1;

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
        private int _hpThreshold = 8;

        [SerializeField, Min(0), Tooltip("Dégâts infligés si la cible est au-dessus du seuil")]
        private int _fallbackDamage = 1;

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

    /// <summary>Le monstre a Provocation : les compétences adverses qui visent un ennemi doivent le cibler.</summary>
    [Serializable]
    public class TauntEffect : SkillEffect
    {
    }

    /// <summary>
    /// Marque un emplacement adverse tiré au hasard (Chasseur de trophées) : un monstre adverse vaincu
    /// sur cet emplacement rapporte des points en plus, sauf s'il est vaincu par la fatigue.
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
