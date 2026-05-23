# 🎨 Art Bible — Monster Card Game

Ce dossier rassemble toutes les fiches destinées à **l'artiste / graphiste** du projet.
Chaque fiche donne, pour une créature, une faction, une zone ou un personnage, les
indications nécessaires à la production des illustrations.

> Univers en bref : un monde fantasy où l'humanité, repoussée par des monstres
> gigantesques, survit dans une dernière cité grâce à la **Forge**. Le joueur, forgeron,
> récolte l'essence des créatures pour raviver la Forge — sans savoir qu'il libère ainsi
> une entité ancienne scellée dans les Ruines. Voir `00-direction-artistique.md` et
> `_bmad-output/gdd.md` / `_bmad-output/narrative-design.md` pour le détail.

---

## 📁 Organisation

```
docs/art-bible/
├── README.md                     ← ce fichier (mode d'emploi)
├── 00-direction-artistique.md    ← style global, palettes, références (à lire en premier)
├── _templates/                   ← modèles vierges à copier
│   ├── template-creature.md
│   ├── template-faction.md
│   ├── template-zone.md
│   └── template-personnage.md
├── creatures/                    ← une fiche par monstre
├── factions/                     ← une fiche par faction / groupe
├── zones/                        ← une fiche par zone
└── personnages/                  ← une fiche par PNJ
```

Les fichiers commençant par `_exemple-…` sont des **exemples remplis** : copie-les ou
inspire-t'en. Ne les envoie pas tels quels au graphiste — ils servent de référence.

---

## ✍️ Comment créer une nouvelle fiche

1. Choisis le bon template dans `_templates/`.
2. Crée un nouveau fichier dans le dossier correspondant
   (ex. `creatures/golem-des-marais.md`).
3. Remplis les sections. Laisse vide ce qui n'est pas encore décidé — c'est normal.
4. Commite (voir workflows ci-dessous).

**Convention de nommage des fichiers :** `kebab-case`, sans accents
(ex. `aigle-runique-geant.md`, `nereth-vilevase.md`). Ça évite tout souci d'encodage.

---

## 📱 Workflows depuis le téléphone (weekend sans PC)

Tout ce dossier est sur GitHub (`GuillaumeArsac/MonsterCardGame`). Deux façons de
travailler depuis le mobile, puis un `git pull` sur le PC pour tout récupérer.

### Option A — Claude Code sur le web (le plus fluide)

1. Ouvre **`claude.ai/code`** dans le navigateur du téléphone.
2. Connecte le dépôt **`GuillaumeArsac/MonsterCardGame`**.
3. Demande, par ex. :
   > « Crée une fiche créature pour un golem de boue des Marais en suivant le template
   >   `docs/art-bible/_templates/template-creature.md`, et range-la dans
   >   `docs/art-bible/creatures/`. »
4. L'agent écrit le fichier **et le commite directement** sur GitHub.

### Option B — App Claude (chat) + github.com

1. Dans l'app Claude, génère le contenu de la fiche (copie le template au besoin).
2. Va sur **`github.com/GuillaumeArsac/MonsterCardGame`** dans le navigateur mobile.
3. Navigue dans `docs/art-bible/creatures/` → bouton **Add file → Create new file**.
4. Nomme le fichier (`golem-des-marais.md`), colle le contenu, **Commit changes**.

### De retour sur le PC (lundi)

```powershell
git pull
```

Tes nouvelles fiches apparaissent dans `docs/art-bible/`.

---

## 📤 Envoyer les fiches au graphiste

- **Le plus simple :** partage-lui un lien GitHub direct vers le fichier
  (ex. `https://github.com/GuillaumeArsac/MonsterCardGame/blob/main/docs/art-bible/creatures/aigle-runique-geant.md`).
  GitHub affiche le Markdown en clair, lisible dans n'importe quel navigateur.
- **Ou** copie-colle le contenu dans un email / message.
- **Ou** exporte en PDF si besoin d'un format figé (depuis le PC).
