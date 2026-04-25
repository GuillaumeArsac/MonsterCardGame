using MonsterCardGame.Gameplay.Cards.Effects;
using UnityEngine;

namespace MonsterCardGame.Gameplay
{
    [CreateAssetMenu(menuName = "MonsterCardGame/Cards/Effects/Generate Mana")]
    public class GenerateManaEffect : CardEffect
    {
        [SerializeField] private int _value;

        public override void Apply(CardEffectContext ctx)
        {
            if (ctx.IsPlayer)
            {
                ctx.Combat.PlayerMana += _value;
                Core.GameLog.Info("GenerateManaEffect", $"Joueur génère {_value} MANA → {ctx.Combat.PlayerMana} PV");
            }
        }
    }
}