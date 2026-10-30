# Exploration (proposition à valider)

Ce fichier décrit comment le joueur parcourt le monde entre les combats. Il ne contient que des propositions tant qu'elles ne sont pas validées.
Le scénario est dans [scenario/index.md](scenario/index.md), les combats du scénario dans [combats-scenario.md](combats-scenario.md) et la progression dans [progression.md](progression.md).

## Principe

Chaque zone est faite de quelques **lieux illustrés**. Le joueur s'y déplace et clique sur ce qui s'y trouve : des PNJ à qui parler, des combats à lancer, des indices à examiner.
Le monde vit par les dialogues : ce que disent les PNJ et ce qui est disponible dans un lieu changent avec l'avancée de l'histoire.

## Structure

```
Carte du monde
└── Zone (carte de la zone : ses lieux, reliés entre eux)
    └── Lieu (une illustration de fond)
        └── Points d'intérêt (cliquables)
```

**Points d'intérêt**

| Type | Au clic |
|---|---|
| PNJ | Ouvre un dialogue. Le dialogue proposé dépend de l'avancée de l'histoire |
| Combat | Présente l'adversaire puis lance le combat. Les combats du scénario disparaissent une fois gagnés, les combats libres restent |
| Indice | Courte description ou réflexion du forgeron (un objet, une trace, un paysage) |
| Passage | Mène à un autre lieu de la zone |

**Naviguer dans une zone :** une carte de la zone montre ses lieux, comme la carte du monde montre les zones. Un lieu peut rester caché ou verrouillé tant que l'histoire ne l'a pas ouvert.

## Dialogues

- Une réplique = un locuteur, son portrait, une expression et un texte.
- Les dialogues peuvent proposer des choix simples. Un choix ne change que la réplique suivante et, au besoin, pose un drapeau.
- Moments où un dialogue se déclenche :
  - en parlant à un PNJ ;
  - en entrant dans un lieu pour la première fois, ou après un événement ;
  - avant un combat du scénario (présentation de l'adversaire) ;
  - après un combat du scénario, avant le dépeçage ou juste après.

## Drapeaux de l'histoire

La sauvegarde retient des **drapeaux** en plus des combats gagnés, par exemple `plaines.marche-accepte`.
Chaque élément d'un lieu (PNJ, dialogue, combat, passage) peut dépendre de **conditions** :
- tel drapeau est posé, ou ne l'est pas ;
- tel combat est gagné, ou ne l'est pas.

Les combats se débloquent alors selon l'histoire, et plus seulement dans l'ordre de la liste.

## La Cité du Foyer

| Lieu | Contenu |
|---|---|
| **La Forge** | Le prologue et le cœur de la Forge. Plus tard, les améliorations de la Forge (à concevoir) |
| **La mairie** | Rudgar Sern. Ses apartés deviennent plus fréquents au fil des retours |
| **La place du marché** | Les habitants, témoins de l'évolution de la cité. Leurs dialogues changent à chaque retour |
| **L'orphelinat** | Là où Maelor a remarqué le forgeron enfant. Souvenirs et indices sur Maelor |
| **La porte de la cité** | Mène à la carte du monde |

## Les Plaines Libres

Les noms des lieux sont provisoires.

| Lieu | Contenu | Beats |
|---|---|---|
| **La route des Plaines** | L'arrivée. La patrouille centaure barre la route (combat 1) | A, C |
| **Le bastion d'Arkhavel** | Arkhavel, son marché, puis l'alerte de la nuée et la scène finale | B, F, H |
| **Le village** | Les habitants : les centaures n'ont jamais été comme ça, le témoin de Maelor, les réactions à la chasse de l'aigle, la nuée qui « cherche à conquérir » | D, E, G, I |
| **Les collines des clans** | L'avant-garde (combat 2) puis le campement de Thrâk (combat 3). Ouvert par le marché d'Arkhavel | C |
| **Le nid de l'aigle** | Chasse optionnelle de l'Aigle Runique Géant (combat libre, à créer) | E |
| **La lisière de la nuée** | Vaelisse (combat 4). Ouvert quand Arkhavel ignore l'alerte de la nuée | F, G |

**Enchaînement proposé**

1. Le joueur arrive sur la route. La patrouille centaure l'attaque (combat 1).
2. Au bastion, Arkhavel propose son marché. Les collines des clans s'ouvrent.
3. Le joueur bat l'avant-garde puis Thrâk (combats 2 et 3). Au village, les habitants s'inquiètent du changement des centaures ; Arkhavel balaie la question.
4. Le témoin de Maelor est disponible au village entre la victoire sur Thrâk et l'alerte de la nuée.
5. Des habitants viennent alerter Arkhavel de l'avancée de la nuée. Il refuse d'agir. La lisière de la nuée s'ouvre.
6. Le joueur bat Vaelisse (combat 4). Au village, un habitant remarque que la nuée se comportait bizarrement.
7. Au bastion, Arkhavel s'attribue la victoire. La zone est terminée.

**Drapeaux proposés**

| Drapeau | Posé quand |
|---|---|
| `plaines.arrivee` | Le joueur entre dans les Plaines pour la première fois |
| `plaines.marche-accepte` | Fin du dialogue du marché avec Arkhavel |
| `plaines.temoin-maelor` | Fin du dialogue avec le témoin de Maelor |
| `plaines.alerte-nuee` | Fin de la scène où Arkhavel ignore l'alerte |
| `plaines.aigle-chasse` | Victoire sur l'Aigle Runique Géant |
| `plaines.terminee` | Fin de la scène finale avec Arkhavel |

## Questions ouvertes

- Le prologue se joue-t-il dans les lieux de la Cité (le joueur clique pour avancer), ou comme une scène continue ?
- L'alerte de la nuée arrive-t-elle automatiquement après Thrâk, ou après le témoin de Maelor ?
- Les noms définitifs des lieux, du bastion et des habitants à nommer.
- La patrouille centaure : sur la route avant de rencontrer Arkhavel (proposé ici), ou après le marché comme l'indique le beat C ?
