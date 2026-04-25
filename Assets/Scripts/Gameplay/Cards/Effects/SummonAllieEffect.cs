using MonsterCardGame.Gameplay.Cards.Effects;
using MonsterCardGame.Gameplay.Combat;
using UnityEngine;

namespace MonsterCardGame.Gameplay
{
    [CreateAssetMenu(menuName = "MonsterCardGame/Cards/Effects/Summon Allie")]
    public class SummonAllieEffect : CardEffect
    {
        [SerializeField] private int _quantity;

        [SerializeField] private Cards.CardData _allie;

        public override void Apply(CardEffectContext ctx)
        {
            if (ctx.IsPlayer)
            {
                for (int i = 0; i < _quantity; i++)
                    ctx.Combat.PlayerAllies.Add(new AlliedInstance(_allie));
                Core.GameLog.Info("SummonEffect", $"Joueur invoque {_quantity} {_allie.CardName}");
            }
            else
            {
                for (int i = 0; i < _quantity; i++)
                    ctx.Combat.MonsterAllies.Add(new AlliedInstance(_allie));
                Core.GameLog.Info("SummonEffect", $"Monstre invoque {_quantity} {_allie.CardName}");
            }
        }
    }
}