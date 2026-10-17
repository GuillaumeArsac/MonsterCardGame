# Monstres

Ce fichier contient les cartes monstres. Les règles du combat sont dans [combat.md](combat.md) et les compétences dans [competences.md](competences.md).

Une carte monstre affiche son nom, son illustration, ses PV et jusqu'à 3 compétences du catalogue.

## Valeurs de référence

Ces repères servent de base pour créer de nouveaux monstres. Ce ne sont pas des règles : un monstre peut s'en écarter s'il le justifie.

| Rôle | PV | Repères |
|---|---|---|
| Carry | 14 à 18 | Une compétence de dégâts à recharge 0 (environ 2 à 3 dégâts par phase) |
| Support | 16 à 20 | Une compétence à recharge 0 ou une passive, pour agir dès la pose |
| Tank | 26 à 30 | Provocation et/ou Carapace |

- **Un seul tank avec Provocation par équipe.** Avec deux, les carry de l'équipe deviennent presque intouchables.
- **Une équipe de 3 monstres sur le terrain inflige environ 6 à 8 dégâts par phase.** C'est ce rythme qui donne des combats d'environ 9 à 12 tours.
- **Une compétence avec une recharge de 4 ou plus sert rarement** : le monstre est posé avec ses compteurs au maximum.

Résultats des simulations avec les deux équipes ci-dessous (environ 16 000 combats, IA simple) :
- durée moyenne d'environ 9 tours, et plus de 90 % des combats entre 7 et 15 tours ;
- premier monstre vaincu vers le tour 5 ;
- le joueur avec l'équipe A gagne environ 50 % de ses combats contre la horde de Thrâk ;
- le premier joueur garde un léger avantage (57 à 67 % en miroir).

## Équipes de test

Deux équipes de 5 monstres, pour tester le combat. Les noms reprennent les créatures des Plaines déjà présentes dans le scénario.

### Équipe A — équipe de test du joueur

Une équipe équilibrée : 2 carry, 2 supports, 1 tank.

| Monstre | Rôle | PV | Compétences |
|---|---|---|---|
| **Griffon** | Carry | 16 | Griffe, Exécution |
| **Archer centaure** | Carry | 14 | Morsure perforante, Chasseur de trophées |
| **Orbe de lumière** | Support | 18 | Soin, Bénédiction, Aura apaisante |
| **Svathurim** | Support | 20 | Griffe, Bouclier, Repli |
| **Paladin centaure** | Tank | 28 | Provocation, Carapace, Coup de bouclier |

### Équipe B — la horde de Thrâk (adversaire des Plaines)

Une équipe plus agressive : 2 carry, 1 support, 2 tanks, avec un chef qui ralentit tout le terrain.

| Monstre | Rôle | PV | Compétences |
|---|---|---|---|
| **Thrâk le Perpétuel** | Tank | 26 | Provocation, Griffe, Grand balayage |
| **Aigle Runique Géant** | Carry | 18 | Griffe, Exécution, Chasseur de trophées |
| **Guerrier centaure** | Carry | 16 | Morsure perforante, Coup de bouclier |
| **Chaman centaure** | Support | 16 | Griffe, Soin, Bénédiction |
| **Taureau de guerre** | Tank | 30 | Carapace, Griffe, Coup de bouclier |

## Principes suivis

- **Chaque monstre est utile dès sa pose** : il a une compétence à recharge 0, ou une passive qui fonctionne dès son arrivée (Aura apaisante, Provocation, Carapace).
- **Les carry ont peu de PV** et une compétence à recharge 0, pour frapper tout de suite.
- **Les supports ont une compétence offensive** (Griffe) ou une passive, pour ne pas rester inactifs pendant les recharges.
- **Un seul tank avec Provocation par équipe.** Les simulations ont montré que deux Provocations rendent les carry de l'équipe presque intouchables.
- **Chaque équipe a une Exécution**, et l'adversaire a le ralentissement de zone (Grand balayage), pour tester ces mécaniques.
