using MonsterCardGame.Gameplay.Cards;

namespace MonsterCardGame.Gameplay.Combat
{
    /// <summary>État runtime d'un allié pendant le combat. Classe C# pure — jamais MonoBehaviour ni SO.</summary>
    public class AlliedInstance
    {
        public CardData Data { get; }
        public int ATK { get; private set; }
        public int DEF { get; private set; }
        public bool IsSleeping { get; private set; }

        public AlliedInstance(CardData data)
        {
            Data = data;
            ATK = data.Attack;
            DEF = data.Defense;
            IsSleeping = !data.HasKeyword(Keyword.Eveille);
        }

        public void SetSleeping(bool sleeping) => IsSleeping = sleeping;

        /// <summary>Réduit les charges (DEF) de 1. Retourne true si l'équipement est épuisé (DEF ≤ 0).</summary>
        public bool SpendCharge()
        {
            DEF--;
            return DEF <= 0;
        }
    }
}