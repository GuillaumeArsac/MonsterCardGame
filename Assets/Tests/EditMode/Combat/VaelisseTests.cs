using System.Linq;
using NUnit.Framework;
using MonsterCardGame.Gameplay.Combat;
using MonsterCardGame.Gameplay.Combat.AI;
using MonsterCardGame.Gameplay.Skills;
using MonsterCardGame.Gameplay.Skills.Effects;

namespace MonsterCardGame.Tests.EditMode.Combat
{
    /// <summary>
    /// Vaelisse, la Mère-Nuée : seule carte adverse, sa défaite rapporte 3 points, et ses invocations posent des insectes
    /// qui ne rapportent rien, ou renforcent un insecte quand le terrain est plein.
    /// Référence : docs/competences.md et docs/combats-scenario.md (combat 4).
    /// </summary>
    public class VaelisseTests : CombatTestBase
    {
        private CombatMonster Vaelisse() => CombatMonster.From(Catalog.VaelisseMereNuee().Monsters[0]);

        /// <summary>Vaelisse seule sur l'emplacement du centre, contre un monstre du joueur.</summary>
        private (CombatMonster vaelisse, CombatMonster joueur) StartAgainstVaelisse(params SkillData[] playerSkills)
        {
            var vaelisse = Vaelisse();
            var joueur = Mon("Joueur", 400, playerSkills);
            Start(new[] { joueur }, new[] { vaelisse }, new[] { (joueur, 0) }, new[] { (vaelisse, 1) });
            return (vaelisse, joueur);
        }

        [Test]
        public void Vaelisse_SeuleCarteAdverse_SaDefaiteRapporte3Points()
        {
            var coup = Active("Coup", 0, new DamageEffect(2000));
            var (vaelisse, joueur) = StartAgainstVaelisse(coup);

            Use(joueur, coup, vaelisse);

            Assert.That(Engine.GetPoints(Side.Player), Is.EqualTo(3));
            Assert.That(Engine.State, Is.EqualTo(CombatState.Ended));
            Assert.That(Engine.Winner, Is.EqualTo(Side.Player));
            Assert.That(Engine.EndReason, Is.EqualTo(CombatEndReason.Points));
        }

        [Test]
        public void Ponte_InvoqueUnInsecte_QuiRejointLEquipe_EtNAgitQuALaPhaseSuivante()
        {
            var (vaelisse, _) = StartAgainstVaelisse();
            MonsterSummonedEvent? summoned = null;
            Engine.MonsterSummoned += e => summoned = e;
            Engine.EndPhase();

            Use(vaelisse, Catalog.Ponte);

            Assert.That(summoned.HasValue, Is.True);
            var insect = summoned.Value.Monster;
            Assert.That(Catalog.Insects().Select(c => c.MonsterName), Has.Member(insect.Name));
            Assert.That(insect.IsSummoned, Is.True);
            Assert.That(insect.Side, Is.EqualTo(Side.Enemy));
            Assert.That(insect.IsOnField, Is.True);
            Assert.That(insect.Slot, Is.Not.EqualTo(vaelisse.Slot));
            Assert.That(Engine.GetTeam(Side.Enemy), Has.Member(insect));
            Assert.That(Enumerable.Range(0, insect.Skills.Count).Any(i => Engine.CanUseSkill(insect, i)), Is.False,
                        "un insecte invoqué n'agit qu'à partir de la phase suivante");
        }

        [Test]
        public void Insecte_Vaincu_NeRapporteAucunPoint()
        {
            var coup = Active("Coup", 0, new DamageEffect(500));
            var (vaelisse, joueur) = StartAgainstVaelisse(coup);
            Engine.EndPhase();
            Use(vaelisse, Catalog.Ponte);
            var insect = Engine.GetField(Side.Enemy).Single(m => m.IsSummoned);
            Engine.EndPhase();

            // L'insecte peut avoir Provocation (Soldat de chitine) : on le vise directement
            Use(joueur, coup, insect);

            Assert.That(insect.Zone, Is.EqualTo(MonsterZone.Defeated));
            Assert.That(Engine.GetPoints(Side.Player), Is.EqualTo(0));
            Assert.That(Engine.State, Is.EqualTo(CombatState.Phase));
        }

        [Test]
        public void Invocation_TerrainPlein_RenforceUnInsecte()
        {
            var (vaelisse, _) = StartAgainstVaelisse();
            Engine.EndPhase();
            Use(vaelisse, Catalog.Ponte);
            AdvanceTo(2, Side.Enemy);
            Use(vaelisse, Catalog.Ponte);
            Assert.That(Engine.GetField(Side.Enemy).Count(), Is.EqualTo(3), "terrain plein");

            var insects = Engine.GetField(Side.Enemy).Where(m => m.IsSummoned).ToList();
            var before = insects.Select(m => (m.MaxHp, m.Hp, m.DamageBonus)).ToList();
            ReinforcedEvent? reinforced = null;
            Engine.Reinforced += e => reinforced = e;

            AdvanceTo(3, Side.Enemy);
            Use(vaelisse, Catalog.Ponte);

            Assert.That(reinforced.HasValue, Is.True);
            Assert.That(Engine.GetField(Side.Enemy).Count(), Is.EqualTo(3));
            int index = insects.IndexOf(reinforced.Value.Monster);
            Assert.That(index, Is.GreaterThanOrEqualTo(0), "seul un insecte invoqué est renforcé");
            var insect = insects[index];
            Assert.That(insect.MaxHp, Is.EqualTo(before[index].MaxHp + 20));
            Assert.That(insect.Hp, Is.EqualTo(before[index].Hp + 20));
            Assert.That(insect.DamageBonus, Is.EqualTo(before[index].DamageBonus + 10));
        }

        [Test]
        public void Essaim_InvoqueDeuxInsectes()
        {
            var (vaelisse, _) = StartAgainstVaelisse();
            AdvanceTo(4, Side.Enemy);
            Assert.That(CanUse(vaelisse, Catalog.Essaim), Is.True, "recharge 3 : disponible à sa quatrième phase");

            Use(vaelisse, Catalog.Essaim);

            Assert.That(Engine.GetField(Side.Enemy).Count(m => m.IsSummoned), Is.EqualTo(2));
        }

        [Test]
        public void Simulation_LeCombatDeVaelisseSeJoueJusquAuBout()
        {
            var essences = Catalog.Essences().Take(5).ToArray();
            var vaelisse = Catalog.VaelisseMereNuee().Monsters.ToArray();

            var report = CombatSimulation.Run(() => essences.Select(CombatMonster.From), () => vaelisse.Select(CombatMonster.From), 100, seed: 1);

            Assert.That(report.Combats, Is.EqualTo(100));
            Assert.That(report.AverageTurns, Is.LessThan(CombatSimulation.MaxTurns));
        }
    }
}
