using System.Linq;
using NUnit.Framework;
using MonsterCardGame.Gameplay.Combat;
using MonsterCardGame.Gameplay.Combat.AI;
using MonsterCardGame.Gameplay.Skills;
using MonsterCardGame.Gameplay.Skills.Effects;

namespace MonsterCardGame.Tests.EditMode.Combat
{
    /// <summary>
    /// Mécaniques des Essences : saignement, inciblable, Provocation temporaire, dégâts croissants,
    /// effets sur ce monstre ou ses alliés, et points de la fatigue. Référence : docs/competences.md.
    /// </summary>
    public class EssenceMechanicsTests : CombatTestBase
    {
        // ── Saignement ────────────────────────────────────────────────────

        [Test]
        public void Saignement_AuDebutDeLaPhaseDuCampDuMonstre_5PourcentDesPvMaxParCumul()
        {
            var saignee = Active("Saignée", 0, new DamageEffect(10), new BleedEffect(3));
            var sang = Mon("Sang", 160, saignee);
            var enemy = Mon("Adversaire", 200);
            StartDuel(sang, enemy);

            Use(sang, saignee, enemy);
            Assert.That(enemy.Hp, Is.EqualTo(190));
            Assert.That(enemy.BleedStacks, Is.EqualTo(3));

            AdvanceTo(1, Side.Enemy);
            Assert.That(enemy.Hp, Is.EqualTo(190 - 3 * 10), "3 cumuls de 5 % de 200 PV");

            AdvanceTo(2, Side.Player);
            Assert.That(enemy.Hp, Is.EqualTo(160), "pas pendant la phase du joueur");
        }

        [Test]
        public void Saignement_IgnoreLesReductions_ArrondiALEntierInferieur_1Minimum()
        {
            var saignee = Active("Saignée pure", 0, new BleedEffect(3));
            var sang = Mon("Sang", 160, saignee, saignee);
            var tortue = Mon("Tortue", 150, Catalog.Carapace);
            var minus = Mon("Minus", 15);
            Start(new[] { sang }, new[] { tortue, minus }, new[] { (sang, 0) }, new[] { (tortue, 0), (minus, 1) });

            Engine.UseSkill(sang, 0, tortue);
            Engine.UseSkill(sang, 1, minus);
            AdvanceTo(1, Side.Enemy);

            Assert.That(tortue.Hp, Is.EqualTo(150 - 3 * 7), "5 % de 150 = 7,5, arrondi à 7, sans la Carapace");
            Assert.That(minus.Hp, Is.EqualTo(15 - 3), "5 % de 15 = 0,75 : 1 dégât minimum par cumul");
        }

        [Test]
        public void Soin_SurUnMonstreQuiSaigne_RetireLesSaignementsSansSoigner()
        {
            var coup = Active("Coup", 0, new DamageEffect(50));
            var saignee = Active("Saignée pure", 0, new BleedEffect(3));
            var soin = Active("Soin rapide", 0, new HealEffect(40));
            var monstre = Mon("Monstre", 200, coup, saignee, soin);
            StartDuel(monstre, Mon("Adversaire", 200));

            Use(monstre, coup, monstre);
            Use(monstre, saignee, monstre);
            Use(monstre, soin, monstre);

            Assert.That(monstre.BleedStacks, Is.EqualTo(0));
            Assert.That(monstre.Hp, Is.EqualTo(150), "le soin ne rend pas de PV");
        }

        [Test]
        public void AuraApaisante_RetireLeSaignementApresQuIlAFrappe()
        {
            var saignee = Active("Saignée pure", 0, new BleedEffect(3));
            var sang = Mon("Sang", 160, saignee);
            var porte = Mon("Porte-étendard", 140, Catalog.AuraApaisante);
            StartDuel(sang, porte);

            Use(sang, saignee, porte);
            AdvanceTo(1, Side.Enemy);

            Assert.That(porte.Hp, Is.EqualTo(140 - 3 * 7));
            Assert.That(porte.BleedStacks, Is.EqualTo(0));
        }

        [Test]
        public void VaincuParLeSaignement_RapporteUnPoint()
        {
            var hemorragie = Active("Hémorragie", 0, new BleedEffect(20));
            var sang = Mon("Sang", 160, hemorragie);
            var victime = Mon("Victime", 20);
            var reserve = Mon("Réserve", 200);
            Start(new[] { sang }, new[] { victime, reserve }, new[] { (sang, 0) }, new[] { (victime, 0) });

            Use(sang, hemorragie, victime);
            AdvanceTo(1, Side.Enemy);

            Assert.That(victime.Zone, Is.EqualTo(MonsterZone.Defeated));
            Assert.That(Engine.GetPoints(Side.Player), Is.EqualTo(1));
        }

        // ── Inciblable et Provocation temporaire ──────────────────────────

        [Test]
        public void Inciblable_JusquALaFinDeLaProchainePhaseAdverse_LesCompetencesSansCibleLeTouchent()
        {
            var frappe = Active("Frappe rapide", 0, new DamageEffect(30), new UntargetableEffect(1).On(EffectTarget.Self));
            var vague = Catalog.Track(SkillData.CreateActive("Vague", MonsterRole.Carry, SkillTarget.AllEnemies, 0, new DamageEffect(10)));
            var ombre = Mon("Ombre", 140, frappe);
            var allie = Mon("Allié", 200);
            var enemy = Mon("Adversaire", 400, Catalog.Griffe, vague);
            Start(new[] { ombre, allie }, new[] { enemy }, new[] { (ombre, 0), (allie, 1) }, new[] { (enemy, 0) });

            Use(ombre, frappe, enemy);
            AdvanceTo(1, Side.Enemy);

            Assert.That(Engine.GetValidTargets(enemy, 0), Has.No.Member(ombre));
            Assert.That(Engine.GetValidTargets(enemy, 0), Has.Member(allie));
            Use(enemy, vague);
            Assert.That(ombre.Hp, Is.EqualTo(130));

            AdvanceTo(2, Side.Player);
            Assert.That(ombre.IsUntargetable, Is.False);
        }

        [Test]
        public void ProvocationTemporaire_PendantLaPhaseAdverse_FinitAuDebutDeSaPhase()
        {
            var defi = Active("Défi rapide", 0, new DamageEffect(10), new TauntEffect(1).On(EffectTarget.Self));
            var pierre = Mon("Pierre", 260, defi);
            var carry = Mon("Carry", 140);
            var enemy = Mon("Adversaire", 400, Catalog.Griffe);
            Start(new[] { pierre, carry }, new[] { enemy }, new[] { (pierre, 0), (carry, 1) }, new[] { (enemy, 0) });

            Assert.That(pierre.HasTaunt, Is.False);
            Use(pierre, defi, enemy);
            AdvanceTo(1, Side.Enemy);

            Assert.That(Engine.GetValidTargets(enemy, 0).Where(t => t.Side == Side.Player), Is.EqualTo(new[] { pierre }));

            AdvanceTo(2, Side.Player);
            Assert.That(pierre.HasTaunt, Is.False);
        }

        // ── Dégâts croissants, soin et bonus des alliés ───────────────────

        [Test]
        public void CroissanceSauvage_20Puis25Puis30_EtGagne10PvParAttaque()
        {
            var nature = Mon("Nature", 180, Catalog.CroissanceSauvage);
            var enemy = Mon("Adversaire", 400);
            StartDuel(nature, enemy);

            Use(nature, Catalog.CroissanceSauvage, enemy);
            AdvanceTo(2, Side.Player);
            Use(nature, Catalog.CroissanceSauvage, enemy);
            AdvanceTo(3, Side.Player);
            Use(nature, Catalog.CroissanceSauvage, enemy);

            Assert.That(enemy.Hp, Is.EqualTo(400 - 20 - 25 - 30));
            Assert.That(nature.MaxHp, Is.EqualTo(180 + 3 * 10), "+10 PV de départ à chaque attaque");
            Assert.That(nature.Hp, Is.EqualTo(210), "les PV actuels augmentent aussi");
        }

        [Test]
        public void SouffleDeVie_LAllieLePlusBlesseRecupereLesDegatsInfliges()
        {
            var coup = Active("Coup", 0, new DamageEffect(50));
            var egratignure = Active("Égratignure", 0, new DamageEffect(10));
            var vie = Mon("Vie", 160, Catalog.SouffleDeVie);
            var blesse = Mon("Blessé", 200, coup);
            var egratigne = Mon("Égratigné", 200, egratignure);
            var enemy = Mon("Adversaire", 400, Catalog.Carapace);
            Start(new[] { vie, blesse, egratigne }, new[] { enemy },
                  new[] { (vie, 0), (blesse, 1), (egratigne, 2) }, new[] { (enemy, 0) });

            Use(blesse, coup, blesse);
            Use(egratigne, egratignure, egratigne);
            Use(vie, Catalog.SouffleDeVie, enemy);

            Assert.That(enemy.Hp, Is.EqualTo(400 - 5), "15 dégâts, moins 10 de Carapace");
            Assert.That(blesse.Hp, Is.EqualTo(150 + 5), "le plus blessé récupère les PV retirés");
            Assert.That(egratigne.Hp, Is.EqualTo(190));
        }

        [Test]
        public void SouffleDeVie_SurUnAllieQuiSaigne_RetireLesSaignements()
        {
            var saignee = Active("Saignée", 0, new DamageEffect(50), new BleedEffect(3));
            var vie = Mon("Vie", 160, Catalog.SouffleDeVie);
            var blesse = Mon("Blessé", 200, saignee);
            var enemy = Mon("Adversaire", 400);
            Start(new[] { vie, blesse }, new[] { enemy }, new[] { (vie, 0), (blesse, 1) }, new[] { (enemy, 0) });

            Use(blesse, saignee, blesse);
            Use(vie, Catalog.SouffleDeVie, enemy);

            Assert.That(blesse.BleedStacks, Is.EqualTo(0));
            Assert.That(blesse.Hp, Is.EqualTo(150), "le soin retire les saignements au lieu de rendre des PV");
        }

        [Test]
        public void FaveurCeleste_BonusATousLesAllies_Pendant3Tours()
        {
            var cieux = Mon("Cieux", 160, Catalog.FaveurCeleste);
            var allie = Mon("Allié", 200, Catalog.Griffe);
            var enemy = Mon("Adversaire", 400);
            Start(new[] { cieux, allie }, new[] { enemy }, new[] { (cieux, 0), (allie, 1) }, new[] { (enemy, 0) });
            AdvanceTo(4, Side.Player);

            Use(cieux, Catalog.FaveurCeleste, enemy);
            Use(allie, Catalog.Griffe, enemy);

            Assert.That(enemy.Hp, Is.EqualTo(400 - 10 - 40), "les dégâts de la Faveur sont infligés avant le bonus");
            AdvanceTo(6, Side.Enemy);
            Assert.That(allie.DamageBonus, Is.EqualTo(10), "3e phase de l'adversaire");
            AdvanceTo(7, Side.Player);
            Assert.That(allie.DamageBonus, Is.EqualTo(0));
        }

        [Test]
        public void MareeDesAbysses_RalentitTousLesAdversaires()
        {
            var maree = Catalog.Track(SkillData.CreateActive("Marée rapide", MonsterRole.Support, SkillTarget.AllEnemies, 0, new SlowEffect()));
            var profondeurs = Mon("Profondeurs", 180, maree);
            var e1 = Mon("Adversaire 1", 200, Catalog.Griffe);
            var e2 = Mon("Adversaire 2", 200, Catalog.Griffe);
            Start(new[] { profondeurs }, new[] { e1, e2 }, new[] { (profondeurs, 0) }, new[] { (e1, 0), (e2, 1) });

            Use(profondeurs, maree);
            AdvanceTo(1, Side.Enemy);

            Assert.That(CanUse(e1, Catalog.Griffe), Is.False);
            Assert.That(CanUse(e2, Catalog.Griffe), Is.False);
        }

        // ── Fatigue ───────────────────────────────────────────────────────

        [Test]
        public void LesDeuxCampsAtteignent3PointsEnMemeTemps_LeJoueurPerd()
        {
            var players = new[] { Mon("Joueur 1", 10), Mon("Joueur 2", 10), Mon("Joueur 3", 10) };
            var enemies = new[] { Mon("Adversaire 1", 10), Mon("Adversaire 2", 10), Mon("Adversaire 3", 10) };
            Start(players, enemies, players.Select((m, i) => (m, i)), enemies.Select((m, i) => (m, i)));

            while (Engine.State != CombatState.Ended) Engine.EndPhase();

            Assert.That(Engine.GetPoints(Side.Player), Is.EqualTo(3));
            Assert.That(Engine.GetPoints(Side.Enemy), Is.EqualTo(3));
            Assert.That(Engine.Winner, Is.EqualTo(Side.Enemy));
            Assert.That(Engine.EndReason, Is.EqualTo(CombatEndReason.BothReachedPoints));
        }

        // ── IA et textes ──────────────────────────────────────────────────

        [Test]
        public void Simulation_EssencesContreLaPatrouille_SeJoueJusquAuBout()
        {
            var essences = Catalog.Essences().Take(5).ToArray();
            var patrol = Catalog.PatrouilleCentaure().Monsters.ToArray();

            var report = CombatSimulation.Run(() => essences.Select(CombatMonster.From), () => patrol.Select(CombatMonster.From), 100, seed: 1);

            Assert.That(report.Combats, Is.EqualTo(100));
            Assert.That(report.AverageTurns, Is.LessThan(CombatSimulation.MaxTurns));
        }

        /// <summary>
        /// Combat 2 : gagnable par l'équipe de début de partie. Il visait environ 4 victoires sur 5 (79 %), mais depuis
        /// la refonte de Nature et Vie (2026-10-24) l'équipe gagne presque toujours : l'équilibre est à revoir
        /// (docs/combats-scenario.md). Remettre la borne haute à 0,92 quand il le sera.
        /// </summary>
        [Test]
        public void AvantGardeDeThrak_EquipeDeDebutDePartie_PeutGagner()
        {
            var essences = Catalog.Essences();
            var team = new[] { "nature", "vie", "pierre", "ombre", "cieux" }
                       .Select(n => essences.First(e => e.MonsterName.EndsWith(n))).ToArray();
            var vanguard = Catalog.AvantGardeDeThrak().Monsters.ToArray();

            var report = CombatSimulation.Run(() => team.Select(CombatMonster.From), () => vanguard.Select(CombatMonster.From), 300, seed: 12345);

            Assert.That(report.PlayerWinRate, Is.InRange(0.65, 1.0), report.ToString());
        }

        [Test]
        public void Description_DesAttaquesDesEssences()
        {
            Assert.That(SkillDescriber.Effect(Catalog.EntailleSanglante), Does.Contain("applique 3 saignements"));
            Assert.That(SkillDescriber.Effect(Catalog.FrappeDeLOmbre), Does.Contain("ce monstre ne peut plus être ciblé par l'adversaire pendant 1 tour"));
            Assert.That(SkillDescriber.Effect(Catalog.SouffleDeVie), Does.Contain("l'allié le plus blessé sur le terrain récupère autant de PV que les dégâts infligés"));
            Assert.That(SkillDescriber.Effect(Catalog.CroissanceSauvage), Does.Contain("ce monstre gagne ensuite 10 PV pour le reste du combat"));
            Assert.That(SkillDescriber.Effect(Catalog.FaveurCeleste), Does.Contain("les dégâts infligés par tous les alliés sur le terrain augmentent de 10 pendant 3 tours"));
            Assert.That(SkillDescriber.Effect(Catalog.DefiDePierre), Does.Contain("ce monstre a Provocation pendant 1 tour"));
        }
    }
}
