using System.Collections.Generic;
using MonsterCardGame.Gameplay.Cards;
using MonsterCardGame.Gameplay.Cards.Effects;

namespace MonsterCardGame.Gameplay.Combat
{
    public static class CombatHelper
    {
        /// <summary>
        /// Retire <paramref name="ally"/> de sa zone, l'envoie au cimetière,
        /// puis déclenche ses OnDestroyEffects.
        /// </summary>
        public static void DestroyAlly(
            CombatContext ctx,
            List<AlliedInstance> zone,
            List<CardData> cemetery,
            AlliedInstance ally,
            bool isPlayer)
        {
            zone.Remove(ally);
            cemetery.Add(ally.Data);
            Core.GameLog.Info("Combat", $"{ally.Data.CardName} est détruit → cimetière");

            foreach (var effect in ally.Data.OnDestroyEffects)
                effect.Apply(new CardEffectContext(ctx, ally, isPlayer));
        }
    }
}
