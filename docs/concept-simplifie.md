# MonsterCardGame — Concept simplifié

> **Statut : brouillon v0.1, à valider.**
> Ce document remplace le GDD (`_bmad-output/gdd.md`) comme référence de design.
> Règle d'or : si une mécanique ne sert pas le pilier ci-dessous, elle n'entre pas dans le jeu.

---

## 1. Le pitch en une phrase

**Tu affrontes des monstres en duel de cartes, et chaque monstre vaincu te laisse une de ses capacités : ce qui t'a frappé devient ton arme.**

## 2. Le pilier unique

**« Apprendre le monstre pour obtenir son pouvoir. »**

- Pendant le combat, le monstre joue ses capacités sous forme de cartes visibles.
- Une fois le monstre vaincu, le joueur choisit **une** de ces capacités et l'ajoute à son deck, **telle qu'il l'a subie**.
- Le deck du joueur devient ainsi son journal de chasse : chaque carte rappelle un combat.

Pas d'étape intermédiaire (matériaux, recettes, forge) entre la victoire et la carte : **le lien entre le monstre et la carte doit être immédiat et lisible.**

## 3. La boucle de jeu

```
Choisir un monstre ──► Combattre ──► Victoire : récupérer 1 capacité ──► Ajuster son deck ──┐
        ▲                  │                                                                │
        │                  └─► Défaite : on réessaie, rien n'est perdu                      │
        └───────────────────────────────────────────────────────────────────────────────────┘
```

## 4. Les règles du combat (une page maximum)

**Mise en place**
- Le joueur a **20 PV** et un deck de **10 à 15 cartes**.
- Le monstre a ses propres PV (entre 12 et 30) et son propre deck de capacités.

**Le tour du joueur**
1. Il pioche jusqu'à avoir **5 cartes** en main.
2. Il reçoit **3 points d'énergie** (non cumulables d'un tour à l'autre).
3. Il joue autant de cartes que son énergie le permet.
4. Il défausse sa main. Quand la pioche est vide, la défausse est mélangée et devient la nouvelle pioche.

**Le tour du monstre**
- Le monstre **annonce à l'avance** la capacité qu'il jouera au tour suivant (l'*intention*), affichée au-dessus de lui.
- Le joueur peut donc anticiper : se protéger, frapper fort ou contrer.

**Un seul type de carte : la capacité**
- Chaque carte a un **coût** (0 à 3), un **nom** et un **effet en une ou deux lignes**.
- Il n'y a pas de types différents (Alliés, Blocs, Équipements, Réactions…) : l'effet de la carte dit ce qu'elle fait.
- Effets de base : *Dégâts X*, *Armure X* (absorbe des dégâts jusqu'au prochain tour), *Soin X*, *Piocher X*.
- Chaque monstre ajoute **une** mécanique propre, portée par ses capacités (ex. : renvoi de dégâts, poison, invocation d'un petit allié).

**Fin du combat**
- PV du monstre à 0 : **victoire**.
- PV du joueur à 0 : **défaite**, sans pénalité. Le joueur revient à la carte et peut retenter le combat ou en choisir un autre.

## 5. Récupérer une capacité

- Chaque monstre possède **3 capacités signature**.
- Après une victoire, le joueur en **choisit 1** parmi les 3.
- Vaincre de nouveau le même monstre permet d'en prendre une autre (2 combats de plus au maximum pour obtenir les 3).
- La carte obtenue est **exactement** celle du monstre : même nom, même effet, même illustration.

## 6. Le deck du joueur

- **Deck de départ** : 8 cartes basiques (ex. : 4 × *Frappe*, 3 × *Parade*, 1 × *Souffle*).
- **Collection** : toutes les cartes obtenues sont conservées.
- **Deck de combat** : entre **10 et 15 cartes**, avec **1 seul exemplaire** par capacité de monstre.
- Ces contraintes suffisent : pas de rareté, pas de poids, pas de nombre exact imposé.

## 7. Contenu de la première version (MVP)

| Élément | Quantité |
|---|---|
| Zones | 1 (les Plaines) |
| Monstres | 4 + 1 boss |
| Cartes basiques | 3 différentes |
| Capacités de monstre | 15 (3 par monstre) |
| **Total de cartes différentes** | **18** |

Les autres zones seront ajoutées seulement si cette première zone est amusante.

## 8. Ce qu'on retire par rapport à l'ancienne version

| Ancien système | Décision | Pourquoi |
|---|---|---|
| 5 types de cartes (Alliés, Actions, Blocs, Équipements, Réactions) | **Retiré** → un seul type, la capacité | Trop de règles à expliquer |
| Sacrifice de cartes pour le mana | **Retiré** → 3 points d'énergie fixes par tour | Une seule ressource, simple |
| Fenêtre de réaction | **Retirée** → intention annoncée par le monstre | La tension vient de l'anticipation, sans interrompre le tour |
| Mots-clés | **Retirés** → effets écrits en clair sur la carte | Lisibilité |
| Matériaux, recettes, Forge à niveaux | **Retirés** → la capacité est donnée directement | C'est le détour qui diluait le concept |
| Deck de 40 cartes avec poids de rareté | **Retiré** → 10 à 15 cartes, 1 exemplaire par capacité | Chaque carte compte |
| 6 zones, PNJ, intrigue de la Forge | **En pause** | L'univers reste, mais on n'en garde que ce qui sert la première zone |

## 9. Questions ouvertes

À trancher avant de passer au prototype :

1. **La Forge** : on la supprime complètement, ou on la garde comme simple lieu pour **améliorer** une capacité déjà obtenue (ex. : *Dégâts 4* → *Dégâts 6*) ?
2. **Les alliés** : un monstre peut-il invoquer des créatures sur le plateau (et donc le joueur aussi, s'il récupère cette capacité) ? C'est riche, mais cela ajoute de la complexité.
3. **Le choix de la capacité** : le joueur choisit librement parmi les 3, ou il obtient celle qui l'a **achevé** / qu'il a le plus subie ?
4. **La progression entre les combats** : seulement la carte du monde, ou une suite de combats enchaînés façon « expédition » ?
5. **Les PV entre les combats** : remis au maximum à chaque combat (plus simple), ou conservés pendant une expédition ?

## 10. Exemple : l'Aigle Runique Géant

*(Exemple d'illustration, chiffres à équilibrer.)*

- **PV** : 20
- **Mécanique propre** : *Envol*. Tant qu'il est en vol, il subit 2 dégâts de moins.

| Capacité | Coût | Effet |
|---|---|---|
| Piqué runique | 2 | Dégâts 6. Si tu étais en *Envol*, Dégâts 9 à la place. |
| Envol | 1 | Gagne *Envol* jusqu'à ton prochain tour. Pioche 1 carte. |
| Plume tranchante | 0 | Dégâts 2. |

Le joueur qui a subi le *Piqué runique* sait exactement ce qu'il récupère, et comprend tout de suite comment l'utiliser.
