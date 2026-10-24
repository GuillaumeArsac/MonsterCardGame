# Combats du scénario

Ce fichier décrit les combats du scénario, dans l'ordre de l'aventure.
Les règles sont dans [combat.md](combat.md), les compétences dans [competences.md](competences.md), les monstres dans [monstres.md](monstres.md) et la progression dans [progression.md](progression.md).

## Combat 1 — La patrouille centaure (proposition à valider)

| Champ | Valeur |
|---|---|
| **Zone** | Les Plaines Libres |
| **Moment** | Beat C, premier combat centaure, avant l'affrontement avec Thrâk |
| **Type** | Combat d'introduction |
| **Équipe du joueur attendue** | Collection de départ, sans aucun dépeçage |
| **Objectif de difficulté** | Le joueur gagne environ 3 combats sur 4 avec une équipe bien composée |

**Contexte :** sur la route des Plaines, une patrouille d'éclaireurs centaures barre le passage. Ils sont nerveux et agressifs, et ne ressemblent pas aux centaures que décrivent les habitants. C'est le premier signe de l'influence de Thrâk.

### Équipe adverse

| Monstre | Rôle | PV | Compétences | Boost de stats |
|---|---|---|---|---|
| **Éclaireur centaure** | Carry | 140 | Griffe | +20 PV |
| **Archer novice** | Carry | 120 | Morsure perforante | +20 PV |
| **Jeune taureau** | Tank | 200 | Provocation, Carapace | +20 PV |
| **Porte-étendard** | Support | 140 | Aura apaisante | +20 PV |
| **Cavalier rabatteur** | Carry | 160 | Griffe, Coup de bouclier | +20 PV |

### Ce que le combat apprend au joueur

- **La Provocation :** le Jeune taureau force à frapper le tank en premier.
- **La Carapace :** Griffe ne lui inflige que 20 dégâts, alors que Morsure perforante l'ignore. Le Renard de braise prend tout son sens.
- **Le ralentissement :** le Cavalier rabatteur retarde les compétences du joueur.
- **Le dépeçage :** deux compétences sont nouvelles pour le joueur (Carapace, Aura apaisante). Les autres permettent de doubler une attaque sur un monstre, par exemple une deuxième Griffe pour l'Ours brun.

### Résultats des simulations

Environ 2 000 combats par équipe, avec l'IA simple :

| Équipe du joueur | Victoires | Durée moyenne |
|---|---|---|
| Loup, Renard, Ours, Luciole, Chien | 78 % | 10 tours |
| Loup, Renard, Ours, Chouette, Chien | 83 % | 10 tours |
| Loup, Ours, Chouette, Luciole, Chien (2 attaquants seulement) | 30 % | 12 tours |
| Loup, Renard, Ours, Chouette, Sanglier (sans Provocation) | 1 % | 10 tours |

Ces résultats datent de l'ancienne collection de départ (Loup, Renard, Ours…). Avec les Essences (2026-10-23, 1 000 combats par équipe), la meilleure équipe, Nature, Vie, Pierre, Ombre et Cieux, gagne 97 % de ses combats en 10,2 tours en moyenne : ce premier combat est très facile. Le détail est dans [monstres.md](monstres.md).

Les combats durent tous entre 7 et 15 tours.
La composition de l'équipe compte beaucoup : ce combat apprend aussi qu'il faut **3 attaquants et un tank avec Provocation**.

## Combat 2 — L'avant-garde de Thrâk

| Champ | Valeur |
|---|---|
| **Zone** | Les Plaines Libres |
| **Moment** | Après la patrouille centaure, avant l'affrontement avec Thrâk |
| **Type** | Monstres normaux de zone |
| **Équipe du joueur attendue** | Les Essences, avec la récompense du dépeçage de la patrouille |
| **Objectif de difficulté** | Plus dur que la patrouille : l'équipe de début de partie gagne environ 4 combats sur 5 |

**Contexte :** après la patrouille, l'avant-garde des clans garde la route du campement de Thrâk. Ses guerriers viennent des trois clans de Thrâk : le Guerrier (monture taureau), le Paladin (élan) et l'Archer (chèvre).

### Équipe adverse

| Monstre | Rôle | PV | Compétences | Boost de stats |
|---|---|---|---|---|
| **Guerrier du clan** | Tank | 220 | Provocation, Griffe | +30 PV |
| **Archer du clan** | Carry | 140 | Morsure perforante | +30 PV |
| **Paladin du clan** | Support | 160 | Soin | +30 PV |
| **Lancier du clan** | Carry | 180 | Grand balayage | +30 PV |
| **Éclaireur vétéran** | Carry | 150 | Griffe | +30 PV |

### Ce que le combat apprend au joueur

- **Les compétences de zone contre l'Ombre :** le Grand balayage du Lancier touche l'Essence d'ombre même quand elle est inciblable, et elle n'a que 100 PV.
- **Le soin contre le saignement :** le Soin du Paladin retire les saignements de l'Essence du sang.
- **Un tank qui frappe :** contrairement au Jeune taureau de la patrouille, le Guerrier attaque aussi.

### Résultats des simulations

1 000 combats par équipe (2026-10-24), sans la récompense du dépeçage de la patrouille :

| Équipe du joueur | Victoires |
|---|---|
| Nature, Vie, Pierre, Sang, Profondeurs (la meilleure) | 90 % |
| Nature, Vie, Pierre, Ombre, Cieux (l'équipe de début de partie) | 79 % |
| Nature, Ombre, Cieux, Sang, Profondeurs (la pire, sans Pierre ni Vie) | 18 % |

Les combats durent de 10 à 14 tours en moyenne. L'Essence de pierre devient indispensable : 75 % de victoires en moyenne avec elle, 42 % sans.

Deux autres versions ont été écartées. L'une était trop dure : avec quatre attaques à recharge 0, Exécution et Bouclier, la meilleure équipe ne gagnait que 16 % de ses combats. L'autre était trop facile : avec un Guerrier sans attaque, toutes les équipes gagnaient presque à coup sûr, souvent à la fatigue.
