using System.Linq;
using NUnit.Framework;
using MonsterCardGame.Gameplay.Combat;
using MonsterCardGame.Gameplay.Combat.AI;
using MonsterCardGame.Gameplay.Skills.Effects;

namespace MonsterCardGame.Tests.EditMode.Combat
{
    /// <summary>
    /// Thrâk le Perpétuel : bonus d'allégeance de ses clans et régénération quand l'adversaire marque.
    /// Référence : docs/competences.md et docs/combats-scenario.md (combat 3).
    /// </summary>
    public class ThrakTests : CombatTestBase
    {
        [Test]
        public void Allegeances_ChaqueClanSurLeTerrainDonneSonBonusAThrak()
        {
            var thrak    = Mon("Thrâk", 320, Catalog.EspritPerpetuel, Catalog.Griffe);
            var guerrier = Mon("Champion guerrier", 240, Catalog.AllegianceDuGuerrier);
            var paladin  = Mon("Champion paladin", 180, Catalog.AllegianceDuPaladin);
            var archer   = Mon("Champion archer", 150, Catalog.AllegianceDeLArcher);
            var joueur   = Mon("Joueur", 400);
            Start(new[] { joueur }, new[] { thrak, guerrier, paladin, archer },
                  new[] { (joueur, 0) }, new[] { (thrak, 0), (guerrier, 1), (paladin, 2) });

            Assert.That(thrak.DamageBonus, Is.EqualTo(10), "Guerrier : +10 dégâts infligés");
            Assert.That(thrak.DamageReduction, Is.EqualTo(10), "Paladin : -10 dégâts subis");
            Assert.That(thrak.IgnoresTaunt, Is.False, "l'Archer est encore en main");
            Assert.That(guerrier.DamageBonus, Is.EqualTo(0), "seul le chef reçoit les bonus");
        }

        [Test]
        public void Allegeances_DisparaissentQuandLeClanQuitteLeTerrain()
        {
            var coup     = Active("Coup", 0, new DamageEffect(300));
            var thrak    = Mon("Thrâk", 320, Catalog.EspritPerpetuel);
            var guerrier = Mon("Champion guerrier", 240, Catalog.AllegianceDuGuerrier);
            var reserve  = Mon("Réserve", 200);
            var joueur   = Mon("Joueur", 400, coup);
            Start(new[] { joueur }, new[] { thrak, guerrier, reserve }, new[] { (joueur, 0) }, new[] { (thrak, 0), (guerrier, 1) });

            Use(joueur, coup, guerrier);

            Assert.That(guerrier.Zone, Is.EqualTo(MonsterZone.Defeated));
            Assert.That(thrak.DamageBonus, Is.EqualTo(0));
        }

        [Test]
        public void AllegeanceDeLArcher_ThrakIgnoreLaProvocation()
        {
            var thrak   = Mon("Thrâk", 320, Catalog.EspritPerpetuel, Catalog.Griffe);
            var archer  = Mon("Champion archer", 150, Catalog.AllegianceDeLArcher);
            var chien   = Mon("Chien de garde", 260, Catalog.Provocation);
            var carry   = Mon("Carry", 140);
            Start(new[] { chien, carry }, new[] { thrak, archer },
                  new[] { (chien, 0), (carry, 1) }, new[] { (thrak, 0), (archer, 1) });

            Assert.That(thrak.IgnoresTaunt, Is.True);
            Assert.That(Engine.GetValidTargets(thrak, 1), Has.Member(carry));
            Assert.That(Engine.GetValidTargets(archer, 0), Is.Empty, "l'Archer n'a pas de compétence active");
        }

        [Test]
        public void Perpetuel_QuandLeJoueurMarqueUnPoint_ThrakRecupereTousSesPv()
        {
            var coup      = Active("Coup", 0, new DamageEffect(100));
            var saignee   = Active("Saignée", 0, new BleedEffect(3));
            var execution = Active("Achever", 0, new DamageEffect(500));
            var thrak     = Mon("Thrâk", 320, Catalog.EspritPerpetuel);
            var escorte   = Mon("Escorte", 100);
            var reserve   = Mon("Réserve", 200);
            var joueur    = Mon("Joueur", 400, coup, saignee, execution);
            Start(new[] { joueur }, new[] { thrak, escorte, reserve }, new[] { (joueur, 0) }, new[] { (thrak, 0), (escorte, 1) });

            Use(joueur, coup, thrak);
            Use(joueur, saignee, thrak);
            Assert.That(thrak.Hp, Is.EqualTo(220));

            Use(joueur, execution, escorte);

            Assert.That(Engine.GetPoints(Side.Player), Is.EqualTo(1));
            Assert.That(thrak.Hp, Is.EqualTo(320), "tous ses PV");
            Assert.That(thrak.BleedStacks, Is.EqualTo(0), "et plus de saignement");
        }

        [Test]
        public void Perpetuel_ThrakEnMain_NeSeRegenerePas()
        {
            var execution = Active("Achever", 0, new DamageEffect(500));
            var thrak     = Mon("Thrâk", 320, Catalog.EspritPerpetuel);
            var escorte   = Mon("Escorte", 100);
            var reserve   = Mon("Réserve", 200);
            var joueur    = Mon("Joueur", 400, execution);
            Start(new[] { joueur }, new[] { escorte, thrak, reserve }, new[] { (joueur, 0) }, new[] { (escorte, 0) });
            var healed = 0;
            Engine.Healed += _ => healed++;

            Use(joueur, execution, escorte);

            Assert.That(thrak.Zone, Is.EqualTo(MonsterZone.Hand));
            Assert.That(healed, Is.EqualTo(0));
        }

        [Test]
        public void Simulation_LeCombatDeThrakSeJoueJusquAuBout()
        {
            var essences = Catalog.Essences().Take(5).ToArray();
            var thrak = Catalog.ThrakLePerpetuel().Monsters.ToArray();

            var report = CombatSimulation.Run(() => essences.Select(CombatMonster.From), () => thrak.Select(CombatMonster.From), 100, seed: 1);

            Assert.That(report.Combats, Is.EqualTo(100));
            Assert.That(report.AverageTurns, Is.LessThan(CombatSimulation.MaxTurns));
        }
    }
}
