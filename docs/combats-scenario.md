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

Les combats durent tous entre 7 et 15 tours.
La composition de l'équipe compte beaucoup : ce combat apprend aussi qu'il faut **3 attaquants et un tank avec Provocation**.
