# Game Design Document

Ce fichier donne la vision d'ensemble du jeu. Les règles détaillées sont dans des fichiers séparés, qui font foi :

| Sujet | Fichier |
|---|---|
| Règles du combat | [combat.md](combat.md) |
| Compétences | [competences.md](competences.md) |
| Monstres et Essences | [monstres.md](monstres.md) |
| Progression (dépeçage, collection, Forge) | [progression.md](progression.md) |
| Exploration du monde | [exploration.md](exploration.md) |
| Combats du scénario | [combats-scenario.md](combats-scenario.md) |
| Histoire et scénario | [scenario/histoire.md](scenario/histoire.md) et [scenario/index.md](scenario/index.md) |
| Direction artistique | [art-bible/](art-bible/) |
| Avancement technique | [plan-implementation.md](plan-implementation.md) |

**Auteur :** Guillaume · **Type :** jeu de cartes d'aventure, solo · **Plateforme :** PC (Steam)

## Concept

Monster Card Game est une aventure solo dans un monde de fantasy peuplé de monstres gigantesques. Le joueur, forgeron de la dernière cité, affronte ces monstres en duels de cartes et les **dépèce** après chaque victoire : il prend une compétence ou un boost du monstre vaincu pour le donner à l'un des siens. **Ce qui l'a battu devient sa force.**

Le joueur commence avec 7 Essences, des esprits fabriqués à la Forge, chacun avec une seule compétence. Au fil de l'aventure, chaque Essence prend la forme des monstres dont elle absorbe les compétences. À la fin, l'équipe du joueur raconte son aventure : chaque compétence rappelle le monstre dont elle vient.

Inspiration : les jeux de cartes Yu-Gi-Oh sur GBA et DS.

### Points forts

1. **Des monstres façonnés par ses victoires** : chaque compétence de l'équipe est prise à un monstre vaincu.
2. **Une mémoire mécanique** : les compétences gardent le comportement du monstre dont elles viennent.
3. **Une aventure complète et non punitive** : une seule grande aventure, sans roguelike ni mort définitive.
4. **La fierté de fin d'aventure** : l'équipe finale est l'œuvre du joueur.

## Public visé

- **Principal :** joueurs de jeux de cartes et de stratégie expérimentés (Hearthstone, Slay the Spire, Magic, Yu-Gi-Oh), qui cherchent une expérience narrative et personnelle.
- **Secondaire :** joueurs de RPG attirés par la personnalisation et l'histoire.
- **Profil :** adultes de 18 à 35 ans, joueurs réguliers.
- **Sessions :** 1 à 2 heures en soirée, avec pause et reprise possibles à tout moment.

**Ce qui les motive :** construire une équipe qui leur ressemble, vivre une aventure sans la frustration du roguelike, découvrir un univers de fantasy dans un genre familier, et s'attacher à leurs monstres comme à des trophées.

## Objectifs du projet

1. **Créer un jeu dont je suis fier**, qui correspond à ma vision créative.
2. **Progresser en développement Unity** : ScriptableObjects, interface, sauvegarde, localisation.
3. **Valider la mémoire mécanique** : prouver que des compétences prises aux monstres vaincus créent un vrai attachement.
4. **Livrer une aventure complète**, jouable du début à la fin, avec les 6 zones et la zone finale.

**Pourquoi ce jeu :** les roguelikes perdent tout à chaque partie, les jeux de cartes compétitifs ont des cartes communes où certaines dominent, et aucun ne propose de se construire avec ses victoires. Ici, chaque joueur repart avec une équipe unique et personnelle.

## Piliers

1. **Réflexion stratégique** : chaque décision de combat et de dépeçage a du poids. Aucun monstre ne se bat avec une seule stratégie : il y a toujours plusieurs chemins vers la victoire.
2. **Attachement** : chaque compétence porte la mémoire d'un monstre vaincu et d'un choix fait librement.
3. **Personnalisation** : l'équipe du joueur est son identité, et aucune ne ressemble à une autre.

**Priorité :** en cas de conflit, la personnalisation l'emporte sur la réflexion stratégique. Le joueur doit toujours pouvoir jouer à sa façon, même si ce n'est pas la façon optimale.

## Boucle de jeu

**Boucle de l'aventure**
Explorer une zone → parler aux habitants et découvrir l'histoire → affronter un monstre → le dépecer → renforcer son équipe → avancer dans l'histoire → zone suivante.

**Boucle du combat** (détail dans [combat.md](combat.md))
Chaque camp a jusqu'à 5 monstres en main et 3 emplacements sur le terrain. À chaque phase, le joueur pose des monstres puis fait agir chacun d'eux avec ses compétences disponibles. Chaque monstre vaincu rapporte des points ; le premier à 3 points gagne.

**Durées**
- Un combat : quelques minutes, environ 8 à 12 tours.
- Une zone : 1 à 3 sessions.
- L'aventure complète : plusieurs semaines en soirée.

**Variété :** chaque combat est différent parce que l'équipe du joueur évolue après chaque dépeçage. Chaque adversaire a une équipe fixe et unique : prévisible, mais stratégiquement distincte.

## Victoire et défaite

| Niveau | Victoire | Défaite |
|---|---|---|
| Combat | Marquer 3 points (voir [combat.md](combat.md)) | L'adversaire marque 3 points, ou le joueur n'a plus de monstre |
| Zone | Les combats obligatoires sont gagnés et l'histoire de la zone est terminée | — |
| Aventure | Le boss final des Ruines est vaincu | Pas de défaite définitive |

**Après une défaite :** le joueur revient à la carte de la zone et peut retenter le combat. Sa progression et son équipe sont conservées.

## Mécaniques principales

| Mécanique | Ce que fait le joueur | Pilier servi |
|---|---|---|
| **Explorer** | Parcourt les lieux de chaque zone, parle aux PNJ, découvre l'histoire (voir [exploration.md](exploration.md)) | Personnalisation |
| **Combattre** | Choisit quels monstres poser et quand, et les cibles de leurs compétences | Réflexion stratégique |
| **Dépecer** | Après chaque victoire, choisit une compétence ou un boost du monstre dépecé, et le monstre qui le reçoit (voir [progression.md](progression.md)) | Personnalisation, attachement |
| **Composer son équipe** | Choisit les 5 monstres qui partent au combat parmi sa collection | Réflexion stratégique |
| **Forger** | À concevoir : la Forge fabrique les Essences et pourra leur rendre leur attaque de base ou leur donner une passive | Personnalisation |

**Ce qui fait la progression**
- Un monstre a jusqu'à 3 compétences, et un seul boost de stats.
- Les compétences spéciales des boss (par exemple Ponte ou Essaim de Vaelisse) sont limitées à une par monstre.
- De nouveaux monstres s'obtiennent par les quêtes secondaires. Ils ont une compétence unique qui ne peut pas être remplacée.
- En fin d'aventure, le joueur possède 8 à 12 monstres.

**Principe de difficulté :** le joueur peut construire des combinaisons très fortes, et c'est voulu. La difficulté se règle par la conception des adversaires de chaque zone, pas en limitant ce que le joueur construit.

## Commandes

Souris uniquement, le clavier en complément.

| Action | Commande |
|---|---|
| Choisir un monstre, une compétence, une cible | Clic gauche |
| Inspecter une carte (carte agrandie et compétences détaillées) | Clic droit |
| Parler à un PNJ, explorer un lieu | Clic gauche |
| Naviguer sur la carte du monde | Clic gauche |

**Ressenti :** chaque action doit donner un retour visuel immédiat (cible valide en surbrillance, animation de la compétence, dégâts affichés).

**Accessibilité :** taille du texte réglable, français et anglais dès la sortie. Les options pour daltoniens viendront plus tard.

## Structure du monde

**Un hub avec choix libre.** La Cité du Foyer sert de hub. Depuis la carte du monde, le joueur choisit sa zone.

| Zone | Ambiance | Mécanique dominante |
|---|---|---|
| **Les Plaines Libres** | Horizon infini, cités | Polyvalence. Zone d'entrée |
| **Les Marais de la Putréfaction** | Brume, morts-vivants | À concevoir |
| **La Grande Forêt des Esprits** | Canopée, esprits animaux | À concevoir |
| **Les Montagnes Éternelles** | Pics enneigés, grottes | À concevoir |
| **L'Archipel des Abysses** | Îles et océan profond | À concevoir |
| **Les Ruines Oubliées** *(finale)* | Cités effondrées | À concevoir |

Les mécaniques des zones autres que les Plaines datent de l'ancienne version (cimetière, jetons, blocages…) et sont à repenser avec les règles actuelles.

**Ordre des zones**
- Les Plaines sont la première zone (acte 1).
- Les Marais, la Forêt, les Montagnes et l'Archipel se font ensuite dans l'ordre choisi par le joueur (acte 2).
- Les Ruines s'ouvrent quand le boss de chaque autre zone est vaincu (acte 3).

**Chaque zone contient**
- des lieux à explorer, avec leurs PNJ ;
- des combats du scénario, uniques, et des combats libres, rejouables ;
- un boss de zone ;
- une histoire locale avec un début, un milieu et une fin ;
- des PNJ dont certains n'apparaissent qu'après des actions dans une autre zone.

**Tutoriel :** pas de zone dédiée. Le prologue dans la Cité et les premiers combats des Plaines enseignent les règles.

**Rejouer :** le joueur peut revenir dans une zone terminée pour les combats libres et les PNJ débloqués plus tard.

**Principes**
- **Cohérence entre thème et mécanique :** chaque zone enseigne une mécanique par ses monstres. Les vaincre, c'est comprendre leur pouvoir pour se l'approprier.
- **Une histoire complète par zone**, sans lassitude de monde ouvert.
- **Liberté sans punition :** une défaite ne fait jamais perdre de progression.
- **La carte comme mémoire :** la carte du monde se dévoile et reflète le parcours du joueur.

## Direction artistique et sonore

Le détail est dans [art-bible/](art-bible/).

**Style :** 2D illustrée, colorée et vivante. Chaque zone a sa propre identité visuelle : couleurs, lumière et ton.
- **Monstres :** les pièces maîtresses du jeu. Des illustrations majestueuses, faites pour marquer le joueur.
- **Cartes :** la lisibilité d'abord. Texte clair, mots-clés visibles, hiérarchie de l'information immédiate.
- **Interface :** sobre et fonctionnelle, au service du jeu, jamais en concurrence avec les monstres.
- **Lieux :** une illustration de fond par lieu, et des portraits pour les PNJ (voir [exploration.md](exploration.md)).

**Références :** le conte fantastique illustré pour l'ambiance, Hearthstone pour la lisibilité des cartes, Monster Hunter pour la majesté des monstres.

**Palette :** lumineuse et contrastée, avec une dominante par zone (par exemple des verts troubles et des violets pour les Marais, des bleus glacés et des gris acier pour les Montagnes).

**Musique :** une ambiance atmosphérique, un thème par zone, et un thème propre aux boss emblématiques.

| Zone | Musique |
|---|---|
| Plaines | Thème ouvert et neutre |
| Marais | Sons organiques, cordes graves, atmosphère trouble |
| Forêt | Instruments acoustiques, nappes aériennes, calme vivant |
| Montagnes | Percussions lourdes, harmoniques froides |
| Archipel | Textures aquatiques, contraste entre calme et tension |
| Ruines | Ambiance inquiète, dissonances subtiles |

**Dialogues :** pas de doublage, tout est en texte.

**Production**

| Élément | Approche |
|---|---|
| Illustrations des monstres et des personnages | Un artiste partenaire |
| Interface et éléments simples | En interne |
| Musique et effets sonores | Asset Store et outils génératifs en attendant, finalisation en fin de projet |

## Spécifications techniques

- **Moteur :** Unity 6.3 LTS (6000.3.12f1), en C#.
- **Performances :** 60 images par seconde stables, 30 au minimum sur une configuration modeste. Le jeu est au tour par tour : la priorité est la fluidité de l'interface.
- **Résolution :** 1080p en priorité, interface adaptée au 1440p et à la 4K, formats 16:9 et 16:10.
- **Chargement :** moins de 10 secondes au lancement, transitions courtes.
- **Sauvegarde :** locale, automatique après chaque combat. La sauvegarde sur Steam Cloud est à décider.
- **Steam :** succès à définir. Pas de DLC, pas de microtransactions, pas de mods.
- **Taille :** moins de 1 Go.

| Configuration | Minimum | Recommandée |
|---|---|---|
| Système | Windows 10 64 bits | Windows 10 ou 11 64 bits |
| Mémoire | 4 Go | 8 Go |
| Carte graphique | Intégrée récente | Dédiée d'entrée de gamme |
| Stockage | Environ 500 Mo | Environ 1 Go |

**Contraintes**
- Développement solo.
- Entièrement hors ligne, sans contenu procédural : tout est conçu à la main.
- Les choix techniques (moteur de combat en C# pur, données en ScriptableObjects, contenu généré par des scripts d'éditeur) sont dans [plan-implementation.md](plan-implementation.md).

**Estimation des assets**

| Catégorie | Estimation |
|---|---|
| Illustrations de monstres | 30 à 50 |
| Portraits de PNJ | Un par personnage nommé |
| Fonds de lieux | Quelques lieux par zone, plus la Cité |
| Musiques | 6 thèmes de zone, 6 à 8 thèmes de boss |
| Effets sonores | 50 à 100 |

## Mesures de réussite

**Mesures techniques**

| Mesure | Cible |
|---|---|
| Images par seconde | 60 stables (recommandée), 30 au minimum |
| Chargement initial | Moins de 10 secondes |
| Taille du jeu | Moins de 1 Go |
| Bugs critiques à la sortie (plantage, perte de sauvegarde) | 0 |
| Bugs bloquants à la sortie | Moins de 5 |

**Mesures de jeu**

| Mesure | Cible |
|---|---|
| Joueurs qui terminent le premier combat | Plus de 90 % |
| Temps avant le premier dépeçage | Moins de 20 minutes |
| Joueurs qui terminent les Plaines | Plus de 70 % |
| Victoires contre le premier boss | 60 à 80 % (accessible mais résistant) |
| Diversité des équipes | Pas deux équipes identiques entre testeurs |
| Ressenti après un boss | « J'ai gagné grâce à mon équipe », pas grâce à la chance |

**Critères qualitatifs**
- Les testeurs parlent spontanément de leurs choix et de leurs combinaisons.
- Une défaite donne envie d'adapter son équipe, pas de dire que le jeu est trop dur.
- Les joueurs parlent de leurs monstres avec affection et sont fiers de montrer leur équipe.
- Les joueurs se reconnaissent un style de jeu (« moi je joue saignement »).

**Objectifs personnels :** finir et publier le jeu sur Steam, progresser sur Unity, être fier de le montrer.

**Suivi :** tests informels au fil du développement, tests structurés avant la sortie, lecture des avis après. Pas de statistiques en ligne.

## Hors du périmètre (version 1.0)

- **Fonctionnalités :** multijoueur, éditeur de niveaux ou de cartes, mods, New Game+, modes alternatifs (défis, roguelike, sans fin), classements en ligne, DLC et microtransactions.
- **Plateformes :** consoles, mobile (envisageable après la sortie), VR et web.
- **Finitions :** doublage, musique orchestrale complète, accessibilité avancée.
- **Plus tard :** portage mobile, modes de jeu supplémentaires, nouvelles zones et nouveaux monstres.

## Hypothèses et dépendances

**Hypothèses**
- Unity 6.3 LTS reste stable pendant tout le développement.
- Environ 5 à 6 heures de développement par semaine.
- L'artiste reste disponible pour les illustrations des monstres et des personnages.
- L'Asset Store couvre les besoins provisoires en son et en images.

| Dépendance | Type | Risque |
|---|---|---|
| L'artiste | Collaborateur externe bénévole | Moyen : disponibilité non garantie |
| Unity 6.3 LTS | Moteur | Faible |
| Asset Store | Contenu provisoire | Faible |
| Steam (Steamworks) | Distribution | Faible |

**Risques**
- **Disponibilité de l'artiste :** prévoir des illustrations commandées, ou garder les illustrations provisoires plus longtemps.
- **Dérive du périmètre :** chaque zone est une histoire complète. Les Plaines servent de signal d'alerte : si elles prennent trop de temps, il faudra réduire les autres zones.
- **Expérience Unity :** certaines fonctionnalités (animations, shaders, Steamworks) peuvent prendre plus de temps que prévu.

## Historique

| Version | Date | Changements |
|---|---|---|
| 1.0 | 2026-04-05 | GDD initial, pour l'ancienne version du jeu (deck de 40 cartes, mana, cartes forgées à partir de matériaux) |
| 2.0 | 2026-10-31 | Mis à jour pour la refonte : combat de monstres sur 3 emplacements, dépeçage, Essences, exploration par lieux. Les règles détaillées renvoient aux fichiers dédiés |
