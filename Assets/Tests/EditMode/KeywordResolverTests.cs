using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using MonsterCardGame.Gameplay.Cards;
using MonsterCardGame.Gameplay.Combat;
using MonsterCardGame.Gameplay.Combat.Keywords;

namespace MonsterCardGame.Tests.EditMode
{
    public class KeywordResolverTests
    {
        private KeywordResolver _resolver;

        [SetUp]
        public void SetUp() => _resolver = new KeywordResolver();

        // --- Helpers ---

        private static CardData CreateCard(
            int      attack      = 1,
            int      defense     = 1,
            int      ritualValue = 0,
            params Keyword[] keywords)
        {
            var card = ScriptableObject.CreateInstance<CardData>();
            var so   = new SerializedObject(card);

            var kwProp = so.FindProperty("_keywords");
            kwProp.ClearArray();
            for (int i = 0; i < keywords.Length; i++)
            {
                kwProp.InsertArrayElementAtIndex(i);
                kwProp.GetArrayElementAtIndex(i).intValue = (int)keywords[i];
            }

            so.FindProperty("_attack").intValue      = attack;
            so.FindProperty("_defense").intValue     = defense;
            so.FindProperty("_ritualValue").intValue = ritualValue;
            so.ApplyModifiedPropertiesWithoutUndo();
            return card;
        }

        private static AlliedInstance MakeAlly(int attack = 1, int defense = 1,
                                                params Keyword[] keywords)
            => new AlliedInstance(CreateCard(attack, defense, 0, keywords));

        // --- CanTarget ---

        [Test]
        public void CanTarget_GroundVsGround_ReturnsTrue()
        {
            var attacker = MakeAlly();
            var target   = MakeAlly();
            Assert.IsTrue(_resolver.CanTarget(attacker, target));
        }

        [Test]
        public void CanTarget_GroundVsVol_ReturnsFalse()
        {
            var attacker = MakeAlly();
            var target   = MakeAlly(1, 1, Keyword.Vol);
            Assert.IsFalse(_resolver.CanTarget(attacker, target));
        }

        [Test]
        public void CanTarget_PorteeVsVol_ReturnsTrue()
        {
            var attacker = MakeAlly(1, 1, Keyword.Portee);
            var target   = MakeAlly(1, 1, Keyword.Vol);
            Assert.IsTrue(_resolver.CanTarget(attacker, target));
        }

        [Test]
        public void CanTarget_VolVsVol_ReturnsTrue()
        {
            var attacker = MakeAlly(1, 1, Keyword.Vol);
            var target   = MakeAlly(1, 1, Keyword.Vol);
            Assert.IsTrue(_resolver.CanTarget(attacker, target));
        }

        // --- GetPriorityTarget ---

        [Test]
        public void GetPriorityTarget_OnlyProvocation_ReturnsProvocation()
        {
            var normal = MakeAlly();
            var taunt  = MakeAlly(1, 1, Keyword.Provocation);
            var list   = new List<AlliedInstance> { normal, taunt };
            Assert.AreEqual(taunt, _resolver.GetPriorityTarget(list));
        }

        [Test]
        public void GetPriorityTarget_NoProvocation_ReturnsFirst()
        {
            var a    = MakeAlly();
            var b    = MakeAlly();
            var list = new List<AlliedInstance> { a, b };
            Assert.AreEqual(a, _resolver.GetPriorityTarget(list));
        }

        [Test]
        public void GetPriorityTarget_EmptyList_ReturnsNull()
        {
            Assert.IsNull(_resolver.GetPriorityTarget(new List<AlliedInstance>()));
        }

        // --- CanBeBlocked ---

        [Test]
        public void CanBeBlocked_NoKeyword_ReturnsTrue()
        {
            var ally = MakeAlly();
            Assert.IsTrue(_resolver.CanBeBlocked(ally));
        }

        [Test]
        public void CanBeBlocked_Instantane_ReturnsFalse()
        {
            var ally = MakeAlly(1, 1, Keyword.Instantane);
            Assert.IsFalse(_resolver.CanBeBlocked(ally));
        }

        // --- GetRitualCost ---

        [Test]
        public void GetRitualCost_RituelKeyword_ReturnsValue()
        {
            var card = CreateCard(1, 1, 3, Keyword.Rituel);
            Assert.AreEqual(3, _resolver.GetRitualCost(card));
        }

        [Test]
        public void GetRitualCost_NoRituelKeyword_ReturnsZero()
        {
            var card = CreateCard();
            Assert.AreEqual(0, _resolver.GetRitualCost(card));
        }
    }
}
