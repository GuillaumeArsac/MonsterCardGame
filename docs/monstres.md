# Monstres

Ce fichier contient les cartes monstres. Les règles du combat sont dans [combat.md](combat.md) et les compétences dans [competences.md](competences.md).

Une carte monstre affiche son nom, son illustration, ses PV et jusqu'à 3 compétences du catalogue.

## Valeurs de référence

Ces repères servent de base pour créer de nouveaux monstres. Ce ne sont pas des règles : un monstre peut s'en écarter s'il le justifie.

| Rôle | PV | Repères |
|---|---|---|
| Carry | 140 à 180 | Une compétence de dégâts à recharge 0 (environ 20 à 30 dégâts par phase) |
| Support | 160 à 200 | Une compétence à recharge 0 ou une passive, pour agir dès la pose |
| Tank | 260 à 300 | Provocation et/ou Carapace |

- **Un seul tank avec Provocation par équipe.** Avec deux, les carry de l'équipe deviennent presque intouchables.
- **Une équipe de 3 monstres sur le terrain inflige environ 60 à 80 dégâts par phase.** C'est ce rythme qui donne des combats d'environ 9 à 12 tours.
- **Une compétence avec une recharge de 4 ou plus sert rarement** : le monstre est posé avec ses compteurs au maximum.

Résultats des simulations avec les deux équipes ci-dessous (2 000 combats par configuration, IA simple, voir [tools/simulation](../tools/simulation/README.md)) :
- durée moyenne d'environ 8 tours, et plus de 90 % des combats entre 7 et 15 tours ;
- premier monstre vaincu vers le tour 5 ;
- le joueur avec l'équipe A gagne environ 35 % de ses combats contre la horde de Thrâk. Avant la règle « un monstre utilise toutes ses compétences disponibles », c'était environ 50 % : l'équipe B a davantage de compétences actives offensives ;
- le premier joueur garde un avantage (54 à 73 % en miroir).

## Équipes de test

Deux équipes de 5 monstres, pour tester le combat. Les noms reprennent les créatures des Plaines déjà présentes dans le scénario.

### Équipe A — équipe de test du joueur

Une équipe équilibrée : 2 carry, 2 supports, 1 tank.

| Monstre | Rôle | PV | Compétences |
|---|---|---|---|
| **Griffon** | Carry | 160 | Griffe, Exécution |
| **Archer centaure** | Carry | 140 | Morsure perforante, Chasseur de trophées |
| **Orbe de lumière** | Support | 180 | Soin, Bénédiction, Aura apaisante |
| **Svathurim** | Support | 200 | Griffe, Bouclier, Repli |
| **Paladin centaure** | Tank | 280 | Provocation, Carapace, Coup de bouclier |

### Équipe B — la horde de Thrâk (adversaire des Plaines)

Une équipe plus agressive : 2 carry, 1 support, 2 tanks, avec un chef qui ralentit tout le terrain.

| Monstre | Rôle | PV | Compétences |
|---|---|---|---|
| **Thrâk le Perpétuel** | Tank | 260 | Provocation, Griffe, Grand balayage |
| **Aigle Runique Géant** | Carry | 180 | Griffe, Exécution, Chasseur de trophées |
| **Guerrier centaure** | Carry | 160 | Morsure perforante, Coup de bouclier |
| **Chaman centaure** | Support | 160 | Griffe, Soin, Bénédiction |
| **Taureau de guerre** | Tank | 300 | Carapace, Griffe, Coup de bouclier |

## Principes suivis

- **Chaque monstre est utile dès sa pose** : il a une compétence à recharge 0, ou une passive qui fonctionne dès son arrivée (Aura apaisante, Provocation, Carapace).
- **Les carry ont peu de PV** et une compétence à recharge 0, pour frapper tout de suite.
- **Les supports ont une compétence offensive** (Griffe) ou une passive, pour ne pas rester inactifs pendant les recharges.
- **Un seul tank avec Provocation par équipe.** Les simulations ont montré que deux Provocations rendent les carry de l'équipe presque intouchables.
- **Chaque équipe a une Exécution**, et l'adversaire a le ralentissement de zone (Grand balayage), pour tester ces mécaniques.

## Collection de départ

7 monstres de base, avec une seule compétence chacun. Le joueur en choisit 5 pour combattre.

| Monstre | Rôle | PV | Compétence |
|---|---|---|---|
| **Loup des cendres** | Carry | 150 | Griffe |
| **Renard de braise** | Carry | 140 | Morsure perforante |
| **Ours brun** | Carry | 200 | Griffe |
| **Chouette guérisseuse** | Support | 160 | Soin |
| **Luciole sacrée** | Support | 160 | Bénédiction |
| **Chien de garde** | Tank | 260 | Provocation |
| **Sanglier** | Tank | 240 | Coup de bouclier |

- **3 attaquants sur 7.** Avec une seule compétence, un support ou un tank n'inflige aucun dégât. Les simulations montrent qu'une équipe de base a besoin d'au moins 3 attaquants pour finir ses combats avant la fatigue.
- **Les compétences de base sont toutes remplaçables**, contrairement aux compétences uniques des monstres de quête.
- **Les premiers adversaires doivent avoir 1 ou 2 compétences** et des PV modestes. Face à la horde de Thrâk, l'équipe de base perd tous ses combats.
