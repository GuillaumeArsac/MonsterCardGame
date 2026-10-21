"""Simulateur de combat MonsterCardGame.

Applique les règles de docs/combat.md et docs/competences.md, avec une IA heuristique
simple pour les deux camps. Voir README.md pour l'utilisation et les limites.
"""
import random
from collections import defaultdict

# ---------------------------------------------------------------- compétences
# Compétences actives : nom -> temps de recharge
ACTIVE = {
    "Griffe": 0,
    "Morsure perforante": 0,
    "Soin": 2,
    "Bénédiction": 3,
    "Exécution": 4,
    "Bouclier": 3,
    "Repli": 4,
    "Coup de bouclier": 3,
    "Grand balayage": 3,
}
PASSIVE = {"Chasseur de trophées", "Aura apaisante", "Provocation", "Carapace"}
CFG = {"benediction_duree": 6, "fatigue_start": 16, "max_turns": 60}

# Monstres : nom -> (rôle, PV, compétences). Une compétence peut apparaître plusieurs fois.
# Le rôle ne sert qu'à l'IA (ordre de pose et d'action), comme dans docs/monstres.md.
MONSTERS = {
    # Équipe de test A (docs/monstres.md)
    "Griffon": ("carry", 16, ["Griffe", "Exécution"]),
    "Archer centaure": ("carry", 14, ["Morsure perforante", "Chasseur de trophées"]),
    "Orbe de lumière": ("support", 18, ["Soin", "Bénédiction", "Aura apaisante"]),
    "Svathurim": ("support", 20, ["Griffe", "Bouclier", "Repli"]),
    "Paladin centaure": ("tank", 28, ["Provocation", "Carapace", "Coup de bouclier"]),
    # Équipe de test B, la horde de Thrâk (docs/monstres.md)
    "Thrâk le Perpétuel": ("tank", 26, ["Provocation", "Griffe", "Grand balayage"]),
    "Aigle Runique Géant": ("carry", 18, ["Griffe", "Exécution", "Chasseur de trophées"]),
    "Guerrier centaure": ("carry", 16, ["Morsure perforante", "Coup de bouclier"]),
    "Chaman centaure": ("support", 16, ["Griffe", "Soin", "Bénédiction"]),
    "Taureau de guerre": ("tank", 30, ["Carapace", "Griffe", "Coup de bouclier"]),
    # Collection de départ (docs/monstres.md)
    "Loup des cendres": ("carry", 15, ["Griffe"]),
    "Renard de braise": ("carry", 14, ["Morsure perforante"]),
    "Ours brun": ("carry", 20, ["Griffe"]),
    "Chouette guérisseuse": ("support", 16, ["Soin"]),
    "Luciole sacrée": ("support", 16, ["Bénédiction"]),
    "Chien de garde": ("tank", 26, ["Provocation"]),
    "Sanglier": ("tank", 24, ["Coup de bouclier"]),
    # Combat 1, la patrouille centaure (docs/combats-scenario.md)
    "Éclaireur centaure": ("carry", 14, ["Griffe"]),
    "Archer novice": ("carry", 12, ["Morsure perforante"]),
    "Jeune taureau": ("tank", 20, ["Provocation", "Carapace"]),
    "Porte-étendard": ("support", 14, ["Aura apaisante"]),
    "Cavalier rabatteur": ("carry", 16, ["Griffe", "Coup de bouclier"]),
}

# Équipes de 5 monstres : nom -> liste de noms de MONSTERS
TEAMS = {
    "A": ["Griffon", "Archer centaure", "Orbe de lumière", "Svathurim", "Paladin centaure"],
    "B": ["Thrâk le Perpétuel", "Aigle Runique Géant", "Guerrier centaure", "Chaman centaure", "Taureau de guerre"],
    "Départ": ["Loup des cendres", "Renard de braise", "Ours brun", "Chouette guérisseuse", "Chien de garde"],
    "Patrouille": ["Éclaireur centaure", "Archer novice", "Jeune taureau", "Porte-étendard", "Cavalier rabatteur"],
}


class Effect:
    def __init__(self, kind, value, remaining, other_camp):
        self.kind, self.value, self.remaining, self.other_camp = kind, value, remaining, other_camp


class Mon:
    def __init__(self, name, camp):
        role, hp, skills = MONSTERS[name]
        self.name, self.role, self.max, self.hp, self.skills, self.camp = name, role, hp, hp, list(skills), camp
        # Chaque exemplaire d'une compétence active a son propre compteur (index dans self.actives)
        self.actives = [s for s in skills if s in ACTIVE]
        self.cd = []
        self.effects = []
        self.slot = None

    def has(self, s):
        return s in self.skills

    def count(self, s):
        return self.skills.count(s)

    def enter(self):
        # Entrée sur le terrain : tous les compteurs au maximum
        self.cd = [ACTIVE[s] for s in self.actives]
        self.effects = []

    def bonus(self):
        return sum(e.value for e in self.effects if e.kind == "bonus")

    def reduction(self):
        r = sum(e.value for e in self.effects if e.kind == "red")
        return r + self.count("Carapace")

    def ready(self):
        """Index des compétences actives disponibles (compteur à 0)."""
        return [i for i, c in enumerate(self.cd) if c == 0]


class Game:
    def __init__(self, team_p, team_e, rng, stats=None):
        self.rng = rng
        self.side = {0: [Mon(n, 0) for n in TEAMS[team_p]],
                     1: [Mon(n, 1) for n in TEAMS[team_e]]}
        self.hand = {c: list(self.side[c]) for c in (0, 1)}
        self.field = {0: [], 1: []}
        self.points = {0: 0, 1: 0}
        self.winner = None
        self.reason = None
        self.turn = 0
        self.stats = stats if stats is not None else defaultdict(lambda: defaultdict(float))
        self.first_kill_turn = None
        # marks[c] : emplacements du camp c marqués par les Chasseurs de trophées adverses
        self.marks = {c: [] for c in (0, 1)}
        for c in (0, 1):
            for m in self.side[c]:
                for _ in range(m.count("Chasseur de trophées")):
                    self.marks[1 - c].append(self.rng.randrange(3))

    # ------------------------------------------------------------ utilitaires
    def alive_count(self, c):
        return len(self.hand[c]) + len(self.field[c])

    def check_end(self):
        if self.winner is not None:
            return True
        for c in (0, 1):
            if self.points[c] >= 3:
                self.winner, self.reason = c, "points"
                return True
        e0, e1 = self.alive_count(0) == 0, self.alive_count(1) == 0
        if e0 and e1:
            self.winner, self.reason = 1, "vide (égalité → joueur perd)"
            return True
        if e0 or e1:
            self.winner, self.reason = (1 if e0 else 0), "plus de monstre"
            return True
        return False

    def kill(self, m, killer=None, fatigue=False):
        self.field[m.camp].remove(m)
        self.stats[m.name]["morts"] += 1
        if self.first_kill_turn is None:
            self.first_kill_turn = self.turn
        if fatigue:
            return
        opp = 1 - m.camp
        bonus = self.marks[m.camp].count(m.slot)
        self.points[opp] += 1 + bonus
        self.stats["_camp%d" % opp]["points bonus"] += bonus
        if killer is not None:
            self.stats[killer.name]["kills"] += 1

    def damage(self, src, tgt, base, pierce=False):
        if tgt not in self.field[tgt.camp]:
            return 0
        dmg = base + src.bonus()
        if not pierce:
            dmg -= tgt.reduction()
        dmg = max(0, dmg)
        dealt = min(dmg, tgt.hp)
        tgt.hp -= dmg
        if src.camp != tgt.camp:
            self.stats[src.name]["dégâts"] += dealt
        if tgt.hp <= 0:
            self.kill(tgt, src)
        return dealt

    def slow(self, tgt):
        for i in range(len(tgt.cd)):
            tgt.cd[i] += 1

    def enemy_targets(self, src):
        enemies = self.field[1 - src.camp]
        prov = [m for m in enemies if m.has("Provocation")]
        return prov or enemies

    # ------------------------------------------------------------ IA : pose
    def place_value(self, m, c):
        # priorités simples : un tank si aucun, puis carry, puis support
        roles = [x.role for x in self.field[c]]
        r = m.role
        v = {"carry": 3, "support": 2, "tank": 2}[r]
        if r == "tank" and "tank" not in roles:
            v += 2
        if r == "support" and "support" in roles:
            v -= 1
        return v + self.rng.random() * 1.5

    def place(self, c, max_n=3):
        n = min(max_n, 3 - len(self.field[c]), len(self.hand[c]))
        for _ in range(n):
            m = max(self.hand[c], key=lambda x: self.place_value(x, c))
            self.hand[c].remove(m)
            used = {x.slot for x in self.field[c]}
            free = [i for i in range(3) if i not in used]
            # l'IA remplit d'abord les emplacements non marqués
            free.sort(key=lambda i: (self.marks[c].count(i), self.rng.random()))
            m.slot = free[0]
            m.enter()
            self.field[c].append(m)
            self.stats[m.name]["poses"] += 1

    # ------------------------------------------------------------ IA : action
    def options(self, m):
        c = m.camp
        allies = self.field[c]
        enemies = self.field[1 - c]
        opts = []
        for i in m.ready():
            s = m.actives[i]
            if s in ("Griffe", "Morsure perforante", "Exécution", "Coup de bouclier"):
                for t in self.enemy_targets(m):
                    opts.append((self.score(m, s, t), i, t))
            elif s == "Grand balayage":
                if enemies:
                    opts.append((self.score(m, s, None), i, None))
            elif s in ("Soin", "Bouclier"):
                for t in allies:
                    opts.append((self.score(m, s, t), i, t))
            elif s == "Bénédiction":
                opts.append((self.score(m, s, None), i, None))
            elif s == "Repli":
                for t in allies:
                    if t is not m:
                        opts.append((self.score(m, s, t), i, t))
        return opts

    def threat(self, t):
        return sum({"Griffe": 3, "Morsure perforante": 2, "Exécution": 2, "Grand balayage": 2,
                    "Coup de bouclier": 1.5, "Soin": 2, "Bénédiction": 2.5, "Bouclier": 1,
                    "Repli": 0.5}.get(s, 0) for s in t.actives) + t.bonus()

    def eff_dmg(self, m, t, base, pierce=False):
        d = base + m.bonus() - (0 if pierce else t.reduction())
        return max(0, d)

    def score(self, m, s, t):
        noise = self.rng.random() * 0.6
        if s in ("Griffe", "Morsure perforante"):
            d = self.eff_dmg(m, t, 3 if s == "Griffe" else 2, s == "Morsure perforante")
            if d >= t.hp:
                pts = 1 + self.marks[t.camp].count(t.slot)
                return 10 * pts + self.threat(t) + noise
            return d + 0.15 * self.threat(t) + d / max(t.hp, 1) * 2 + noise
        if s == "Exécution":
            if t.hp <= 8:
                pts = 1 + self.marks[t.camp].count(t.slot)
                return 10 * pts + self.threat(t) + noise
            d = self.eff_dmg(m, t, 1)
            return d * 0.8 + noise
        if s == "Coup de bouclier":
            d = self.eff_dmg(m, t, 1)
            if d >= t.hp:
                return 10 + self.threat(t) + noise
            return d + 0.4 * self.threat(t) + noise
        if s == "Grand balayage":
            v = 0
            for e in self.field[1 - m.camp]:
                d = self.eff_dmg(m, e, 1)
                v += d + 0.4 * self.threat(e) + (10 if d >= e.hp else 0)
            return v + noise
        if s == "Soin":
            missing = t.max - t.hp
            if missing < 2:
                return -1
            return min(4, missing) * (1.5 if t.hp <= 8 else 0.9) + noise
        if s == "Bouclier":
            danger = max(0, 8 - t.hp)
            return 0.5 + danger * 0.5 + (1 if t.role == "carry" else 0) + noise
        if s == "Bénédiction":
            dealers = [a for a in self.field[m.camp]
                       if any(x in a.actives for x in ("Griffe", "Morsure perforante", "Grand balayage", "Coup de bouclier"))]
            return 1.6 * len(dealers) + noise
        if s == "Repli":
            if t.hp <= 3 and self.hand[m.camp]:
                return 6 + noise
            return -1
        return -5

    def act(self, m, i, t):
        s = m.actives[i]
        self.stats[m.name]["uses:" + s] += 1
        m.cd[i] = ACTIVE[s]
        other = 1 - m.camp
        if s == "Griffe":
            self.damage(m, t, 3)
        elif s == "Morsure perforante":
            self.damage(m, t, 2, pierce=True)
        elif s == "Exécution":
            if t.hp <= 8:
                self.stats[m.name]["dégâts"] += t.hp
                t.hp = 0
                self.kill(t, m)
            else:
                self.damage(m, t, 1)
        elif s == "Coup de bouclier":
            self.slow(t)
            self.damage(m, t, 1)
        elif s == "Grand balayage":
            for e in list(self.field[other]):
                self.slow(e)
                self.damage(m, e, 1)
        elif s == "Soin":
            healed = min(4, t.max - t.hp)
            t.hp += healed
            self.stats[m.name]["soins"] += healed
        elif s == "Bouclier":
            t.effects.append(Effect("red", 2, 2, other))
        elif s == "Bénédiction":
            for a in self.field[m.camp]:
                a.effects.append(Effect("bonus", 1, CFG["benediction_duree"], other))
        elif s == "Repli":
            self.field[m.camp].remove(t)
            t.effects = []
            self.hand[m.camp].append(t)

    def phase(self, c):
        # passives de début de phase (monstres déjà sur le terrain)
        for m in list(self.field[c]):
            for _ in range(m.count("Aura apaisante")):
                for a in self.field[c]:
                    h = min(1, a.max - a.hp)
                    a.hp += h
                    self.stats[m.name]["soins"] += h
        self.place(c)
        # ordre d'action : supports, tanks, carry
        order = sorted(self.field[c], key=lambda x: {"support": 0, "tank": 1, "carry": 2}[x.role])
        for m in order:
            if m not in self.field[c] or self.winner is not None:
                continue
            used = set()
            while m in self.field[c] and self.winner is None:
                opts = [o for o in self.options(m) if o[0] > 0 and o[1] not in used]
                if not opts:
                    break
                _, i, t = max(opts, key=lambda o: o[0])
                used.add(i)
                self.act(m, i, t)
                if self.check_end():
                    return
        # fin de phase : les compteurs des monstres de ce camp baissent de 1
        for m in self.field[c]:
            m.cd = [x - 1 if x > 0 else 0 for x in m.cd]
        # effets qui expirent à la fin de la phase de ce camp (posés par l'autre camp)
        for cc in (0, 1):
            for m in self.field[cc]:
                for e in m.effects:
                    if e.other_camp == c:
                        e.remaining -= 1
                m.effects = [e for e in m.effects if e.remaining > 0]

    def play(self):
        # mise en place simultanée
        self.place(0)
        self.place(1)
        while self.winner is None:
            self.turn += 1
            if self.turn >= CFG["fatigue_start"]:
                f = self.turn - CFG["fatigue_start"] + 1
                for c in (0, 1):
                    for m in list(self.field[c]):
                        m.hp -= f
                        if m.hp <= 0:
                            self.kill(m, fatigue=True)
                if self.check_end():
                    break
            for c in (0, 1):
                self.phase(c)
                if self.winner is not None:
                    break
            if self.turn >= CFG["max_turns"]:
                self.winner, self.reason = 1, "limite technique"
        return self.winner, self.turn, self.reason
