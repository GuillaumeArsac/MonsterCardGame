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

**À rééquilibrer :** depuis la refonte de l'Essence de nature et de l'Essence de vie (2026-10-24), l'équipe de début de partie gagne ce combat presque à coup sûr (100 %), et la moyenne des 21 équipes passe de 66 % à 90 %. La patrouille centaure passe aussi de 76 % à 90 % en moyenne. Choix gardé pour l'instant ; à reprendre en freinant la Nature ou en durcissant les adversaires.

Deux autres versions ont été écartées. L'une était trop dure : avec quatre attaques à recharge 0, Exécution et Bouclier, la meilleure équipe ne gagnait que 16 % de ses combats. L'autre était trop facile : avec un Guerrier sans attaque, toutes les équipes gagnaient presque à coup sûr, souvent à la fatigue.

## Combat 3 — Thrâk le Perpétuel (semi-boss)

| Champ | Valeur |
|---|---|
| **Zone** | Les Plaines Libres |
| **Moment** | Après l'avant-garde. Sa défaite ouvre la suite des Plaines (Vaelisse, boss de zone) |
| **Type** | Semi-boss : un chef et ses champions d'élite |
| **Équipe du joueur attendue** | Les Essences, avec les récompenses des deux premiers combats |
| **Objectif de difficulté** | Un vrai mur : environ une victoire sur deux, et nettement plus avec une équipe pensée contre lui |

**Contexte :** Thrâk, l'esprit réuni des plus grands guerriers centaures, mène lui-même ses champions. Chacun des trois clans lui prête sa force, et il se relève de toutes les pertes de son camp.

### Identité de Thrâk

- **Chef des clans :** Thrâk ne fait rien pour ses alliés. Ce sont ses clans qui le renforcent. Tant qu'un champion est sur le terrain, Thrâk reçoit son bonus d'allégeance :

| Clan | Monture | Allégeance |
|---|---|---|
| **Guerrier** | Taureau | Thrâk inflige +10 dégâts |
| **Paladin** | Élan | Thrâk subit 10 dégâts de moins |
| **Archer** | Chèvre | Les compétences de Thrâk ignorent la Provocation |

- **Perpétuel :** chaque fois que le joueur marque un point, Thrâk récupère tous ses PV, s'il est sur le terrain. Abattre ses champions affaiblit Thrâk, mais le soigne entièrement.

C'est tout l'enjeu du combat : tuer Thrâk d'un trait, malgré ses 320 PV, avant de toucher à son escorte ; ou affaiblir ses clans d'abord, en acceptant qu'il se relève.

### Équipe adverse

| Monstre | Rôle | PV | Compétences | Boost de stats |
|---|---|---|---|---|
| **Thrâk le Perpétuel** | Tank | 320 | Esprit perpétuel, Griffe, Grand balayage | +60 PV |
| **Champion guerrier** | Tank | 240 | Provocation, Allégeance du Guerrier | +40 PV |
| **Champion paladin** | Support | 180 | Soin, Allégeance du Paladin | +40 PV |
| **Champion archer** | Carry | 150 | Morsure perforante, Allégeance de l'Archer | +40 PV |
| **Guerrier centaure** | Carry | 160 | Morsure perforante, Coup de bouclier | +40 PV |

Thrâk n'a pas de Provocation : c'est le Champion guerrier qui protège le camp. Le joueur doit donc souvent vaincre le Champion guerrier en premier, ce qui régénère Thrâk.

**Dépeçage :** c'est toujours Thrâk qui est dépecé, pas un monstre tiré au hasard. Le joueur choisit entre Esprit perpétuel (compétence spéciale), la Griffe, le Grand balayage et le boost de +60 PV.

### Ce que le combat apprend au joueur

- **Choisir l'ordre des cibles :** chaque monstre vaincu régénère Thrâk.
- **Les dégâts concentrés et le saignement** pour l'abattre d'un trait. L'Essence du sang devient précieuse, même si la régénération retire ses saignements.
- **La Provocation ne protège plus tout :** avec l'Archer sur le terrain, Thrâk frappe qui il veut.

### Résultats des simulations

1 000 combats par équipe (2026-10-24), sans les récompenses de dépeçage :

| Équipe du joueur | Victoires | Durée moyenne |
|---|---|---|
| Nature, Vie, Pierre, Sang, Profondeurs (la meilleure) | 79 % | 13 tours |
| Nature, Vie, Pierre, Ombre, Cieux (l'équipe de début de partie) | 54 % | 10 tours |
| Vie, Pierre, Ombre, Cieux, Profondeurs (la pire) | 11 % | 12 tours |
| Moyenne des 21 équipes | 51 % | |

- **L'Essence du sang** compte beaucoup : 54 % de victoires en moyenne avec elle, 41 % sans.
- **L'Essence de pierre** n'apporte presque plus rien (51 % avec, 50 % sans), car l'Archer permet à Thrâk d'ignorer sa Provocation.

Ce combat remplace la « horde de Thrâk » (équipe de test B), qui servait de combat libre provisoire et que personne ne battait.

## Combat 4 — Vaelisse, la Mère-Nuée (boss de zone, proposition à équilibrer)

| Champ | Valeur |
|---|---|
| **Zone** | Les Plaines Libres |
| **Moment** | Après Thrâk le Perpétuel. Sa défaite compte pour l'ouverture des Ruines |
| **Type** | Boss de zone : une seule carte adverse, qui invoque des insectes |
| **Équipe du joueur attendue** | Les Essences, avec les récompenses des trois premiers combats |

**Contexte :** Vaelisse, reine d'essaim pervertie par l'Entité, ne se bat pas seule : elle pond sans cesse des insectes tirés au hasard (voir [la fiche de scénario](scenario/monstres/plaines/vaelisse-mere-nuee.md)).

### Identité de Vaelisse

- **Seule carte adverse :** elle est posée seule au début du combat. L'adversaire n'a aucun monstre en main.
- **Sa défaite rapporte 3 points :** la vaincre fait gagner le combat. Ses insectes ne rapportent **aucun** point.
- **Les invocations :** chaque invocation pose un insecte tiré au hasard sur un emplacement libre. Un insecte invoqué n'agit qu'à partir de la phase suivante de son camp.
- **Terrain plein :** l'invocation renforce à la place un insecte tiré au hasard : +20 PV (PV de départ et actuels) et +10 dégâts infligés, pour le reste du combat.

Le joueur doit choisir : frapper la Reine, ou nettoyer des insectes qui ne rapportent rien mais grossissent à chaque ponte quand le terrain est plein.

### Équipe adverse

| Monstre | Rôle | PV | Compétences | Boost de stats |
|---|---|---|---|---|
| **Vaelisse, la Mère-Nuée** | Tank | 650 | Ponte, Essaim, Dard de la Reine | +80 PV |

### Les insectes

| Insecte | Rôle | PV | Compétence |
|---|---|---|---|
| **Ouvrière de la nuée** | Support | 60 | Nourrir la Reine : rend 30 PV à un allié (recharge 1) |
| **Soldat de chitine** | Tank | 120 | Provocation : protège la Reine |
| **Dard volant** | Carry | 50 | Piqûre en vol : 20 dégâts, puis inciblable jusqu'à la fin de la prochaine phase adverse (recharge 1) |
| **Cracheur acide** | Carry | 70 | Crachat acide : 10 dégâts et 2 saignements (recharge 1) |

**Dépeçage :** c'est toujours Vaelisse qui est dépecée. Ponte et Essaim sont des compétences spéciales : le joueur peut les prendre, mais un monstre n'en porte qu'une.

### Résultats des simulations

1 000 combats par équipe (2026-10-31), sans les récompenses de dépeçage, avec Vaelisse à 650 PV et Essaim en recharge 3 :

| Équipe du joueur | Victoires | Durée moyenne |
|---|---|---|
| Nature, Vie, Cieux, Sang, Profondeurs (la meilleure) | 83 % | 10 tours |
| Nature, Vie, Pierre, Ombre, Cieux (l'équipe de début de partie) | 14 % | 12 tours |
| Vie, Pierre, Ombre, Cieux, Profondeurs (la pire) | 0 % | 13 tours |
| Moyenne des 21 équipes | 49 % | 11 tours |

- **L'Essence du sang reste presque indispensable** (64 % de victoires avec elle, 11 % sans). Le saignement retire 5 % des PV max par cumul : 32 PV par cumul sur les 650 PV de Vaelisse.
- **L'Essence d'ombre** est la plus faible ici (43 % avec, 64 % sans) : ses 100 PV ne résistent pas aux insectes.
- Premier réglage (2026-10-29) : avec 900 PV et Essaim en recharge 2, 35 % de victoires en moyenne, et aucune sans le Sang.
