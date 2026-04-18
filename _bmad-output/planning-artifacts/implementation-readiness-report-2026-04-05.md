---
stepsCompleted: [1, 2, 3, 4, 5, 6]
date: '2026-04-05'
project: 'MonsterCardGame'
status: 'READY'
documents:
  gdd: '_bmad-output/gdd.md'
  architecture: '_bmad-output/game-architecture.md'
  epics: '_bmad-output/planning-artifacts/epics.md'
  ux: 'N/A'
---

# Implementation Readiness Assessment Report

**Date :** 2026-04-05
**Projet :** MonsterCardGame
**Assesseur :** Claude Code (Game Producer / Scrum Master)

---

## Document Inventory

| Type | Fichier | Statut |
|---|---|---|
| GDD | `_bmad-output/gdd.md` | ✅ Trouvé |
| Architecture | `_bmad-output/game-architecture.md` | ✅ Trouvé |
| Epics & Stories | `_bmad-output/planning-artifacts/epics.md` | ✅ Trouvé |
| UX Design | N/A | ℹ️ Non applicable (décision délibérée) |

**Note :** `_bmad-output/epics.md` est une version intermédiaire archivée (GDD Step 12) — non utilisée pour cette évaluation.

---

## GDD Analysis

### Exigences Fonctionnelles (28 FRs)

FR1 à FR28 — tous documentés dans `planning-artifacts/epics.md` (Requirements Inventory).

### Exigences Non-Fonctionnelles (8 NFRs)

NFR1 à NFR8 — tous documentés dans `planning-artifacts/epics.md`.

### Exigences Additionnelles (Architecture)

- Unity 6.3 LTS (6000.3.12f1), URP 17.3.0
- ScriptableObject Architecture + Service Locator
- 4 Assembly Definitions (Core, Gameplay, UI, Infrastructure)
- State Machine explicite (ICombatState)
- Pas de sauvegarde mid-combat (ADR-001)
- UI Toolkit exclusivement (UXML/USS)
- SO Event Channels
- KeywordResolver : service pur C#
- JsonSaveSystem : System.Text.Json
- Boot scene avec ServiceLocator initialisé avant le gameplay
- Tests unitaires Edit Mode : KeywordResolver, DeckValidator, CraftSystem, JsonSaveSystem

### Exigences implicites GDD non capturées comme FRs

- **Tutoriel** : "Premier combat guidé au lancement de la partie (mécaniques de base)" — mentionné dans le GDD mais aucun FR associé
- **Deck vide = défaite** : règle de combat explicite dans le GDD
- **Recyclage des cartes** : Actions/Blocages/Réactions → bas du deck après résolution (règle mécanique fondamentale)

---

## Epic Coverage Validation

### Couverture FR : 28/28 ✅

| FR | Epic/Story | Statut |
|---|---|---|
| FR1 | Epic 2 / Story 2.2 | ✅ |
| FR2 | Epic 2 / Story 2.5 | ✅ |
| FR3 | Epic 2 / Story 2.8 | ✅ |
| FR4 | Epic 2 / Story 2.6 | ✅ |
| FR5 | Epic 2 / Story 2.7 | ✅ |
| FR6 | Epic 2 / Story 2.8 | ✅ |
| FR7 | Epic 2 / Story 2.4 | ✅ |
| FR8 | Epic 2 / Story 2.4 | ✅ |
| FR9 | Epic 2 / Story 2.3 | ✅ |
| FR10 | Epic 2 / Story 2.9 | ✅ |
| FR11 | Epic 2 / Story 2.10 | ✅ |
| FR12 | Epic 5–7 / Stories 5.2, 6.1–6.4, 7.3 | ✅ |
| FR13 | Epic 2 / Story 2.11 | ✅ |
| FR14 | Epic 4 / Story 4.3 | ✅ |
| FR15 | Epic 2 / Story 2.1 | ✅ |
| FR16 | Epic 3 / Story 3.1 | ✅ |
| FR17 | Epic 3 / Story 3.2 | ✅ |
| FR18 | Epic 3 / Story 3.3 | ✅ |
| FR19 | Epic 3 / Story 3.4 | ✅ |
| FR20 | Epic 4 / Story 4.1 | ✅ |
| FR21 | Epic 4+7 / Stories 4.2, 7.1 | ✅ |
| FR22 | Epic 5–7 / Stories 5.1, 6.1–6.4, 7.2 | ✅ |
| FR23 | Epic 4+6 / Stories 4.4, 6.5 | ✅ |
| FR24 | Epic 1 / Story 1.2 | ✅ |
| FR25 | Epic 1 / Story 1.3 | ✅ |
| FR26 | Epic 4 / Story 4.5 | ✅ |
| FR27 | Epic 8 / Story 8.1 | ✅ |
| FR28 | Epic 8 / Story 8.3 | ✅ |

**Statistiques :** 28/28 FRs couverts — 100%

---

## UX Alignment Assessment

### Statut du document UX

Non trouvé — décision délibérée documentée dans `planning-artifacts/epics.md`.

### Avertissement

Le GDD implique une UI riche (plateau 6 zones, carte du monde, Forge, construction de deck, menus PNJ). Pas de document UX formel. Ce n'est pas un bloquant pour un projet solo car :
- L'architecture couvre le choix technologique (UI Toolkit — UXML/USS)
- Les stories couvrent les exigences UX essentielles (Story 8.2 accessibilité, NFR6 feedback <100ms)
- La décision a été faite délibérément

---

## Epic Quality Review

### Structure des Epics

| Epic | Valeur Joueur | Indépendance | Stories |
|---|---|---|---|
| Epic 1 : Fondations Techniques | ✅ Borderline (titre technique, goal joueur) | ✅ Standalone | 3 |
| Epic 2 : Système de Combat | ✅ | ✅ (dépend Epic 1) | 11 |
| Epic 3 : Craft & Forge | ✅ | ✅ (dépend Epic 2) | 4 |
| Epic 4 : Monde & Navigation | ✅ | ✅ (dépend Epic 1+2) | 5 |
| Epic 5 : Zone Pilote | ✅ | ✅ (dépend Epic 2+4) | 2 |
| Epic 6 : Zones Thématiques | ✅ | ✅ (dépend Epic 5) | 5 |
| Epic 7 : Zone Finale | ✅ | ✅ (dépend Epic 6) | 3 |
| Epic 8 : Polish & Release | ✅ | ✅ (dépend Epic 1–7) | 4 |

**Dépendances circulaires :** 0 détectées ✅
**Forward dependencies intra-epic :** 0 détectées ✅

### Violations détectées

#### 🔴 CRITIQUE — Conflits GDD vs Stories (3 issues)

**[C1] Story 2.3 — Condition de défaite sur deck vide**
- Story AC : *"si le deck est vide, aucun dommage n'est infligé (la main conserve ses cartes restantes)"*
- GDD (Turn Structure) : *"Si le deck est vide et qu'il faut piocher → défaite (rare en pratique)"*
- **Correction requise :** Aligner l'AC avec le GDD — deck vide lors d'une pioche obligatoire = défaite

**[C2] Story 2.4 — Destination des cartes sacrifiées**
- Story AC : *"glisse une carte vers la zone Cimetière"*
- GDD (Turn Structure) : *"Sacrifice — envoyer des cartes en bas du deck, générer du mana"*
- **Correction requise :** Les cartes sacrifiées vont en bas du deck (zone Deck), pas au Cimetière

**[C3] Story 2.5, 2.8 — Destination des cartes Action/Blocage/Réaction après résolution**
- Story 2.5 AC : *"la carte est envoyée au Cimetière"*
- GDD (Card Types table) : Actions, Blocages, Réactions → *"Bas du deck"* après résolution
- **Correction requise :** Action/Blocage/Réaction → bas du deck. Seuls Alliés détruits → Cimetière. Équipements → détruits

#### 🟠 MAJEUR — Tutoriel non couvert

**[M1] Premier combat guidé absent des stories**
- GDD : *"Premier combat guidé au lancement de la partie (mécaniques de base)"*
- Aucune story ne couvre ce tutoriel
- **Recommandation :** Ajouter Story 2.0 "Tutoriel du premier combat" dans Epic 2 (avant Story 2.1)

#### 🟠 MAJEUR — Ambiguïté Rampant / Cimetière

**[M2] Story 2.9 — AC Rampant insuffisamment précise**
- Story AC : *"peut être jouée depuis le Cimetière selon sa définition"* — trop vague pour implémenter
- Question ouverte : Quels types de cartes peuvent avoir le mot-clé Rampant ? Si seuls les Alliés détruits vont au Cimetière (per GDD), Rampant s'applique uniquement aux Alliés. Mais les cartes sacrifiées vont en bas du deck, pas au Cimetière.
- **Correction requise :** Préciser dans Story 2.9 : (a) quels types de cartes peuvent porter Rampant, (b) les conditions exactes d'accès au Cimetière pour ces cartes

#### 🟡 MINEUR

**[m1] Story 8.4 — "As a développeur"**
- Pas orienté joueur — acceptable pour une validation de build, mais signalé pour cohérence

**[m2] Story 2.9 — Format ACs incomplet**
- Plusieurs ACs en format Given/Then sans When (manque le déclencheur)
- Exemple : *"Given une carte avec Vol... Then elle ne peut être ciblée..."* — préciser When (pendant la phase de ciblage d'une Action)

---

## Summary and Recommendations

### Overall Readiness Status

**✅ READY** — Les 3 conflits critiques ont été corrigés dans les stories. M2 résolu (Rampant = Alliés uniquement, pas de conflit). Les issues mineures restantes ne bloquent pas l'implémentation.

### Issues par priorité

| # | Sévérité | Issue | Story concernée |
|---|---|---|---|
| C1 | 🔴 Critique | Deck vide → défaite (pas "aucun dommage") | Story 2.3 |
| C2 | 🔴 Critique | Sacrifice → bas du deck (pas Cimetière) | Story 2.4 |
| C3 | 🔴 Critique | Action/Blocage/Réaction → bas du deck (pas Cimetière) | Stories 2.5, 2.8 |
| M1 | 🟠 Majeur | Tutoriel premier combat absent | À créer (Story 2.0) |
| M2 | 🟠 Majeur | AC Rampant trop vague | Story 2.9 |
| m1 | 🟡 Mineur | Story 8.4 "As a développeur" | Story 8.4 |
| m2 | 🟡 Mineur | ACs Story 2.9 sans When | Story 2.9 |

### Corrections appliquées ✅

1. **Story 2.3 corrigée** : Deck vide lors d'une pioche obligatoire → défaite
2. **Story 2.4 corrigée** : Cartes sacrifiées → bas du deck (zone Deck)
3. **Stories 2.5 et 2.8 corrigées** : Action, Blocage, Réaction → bas du deck après résolution
4. **Story 2.9 corrigée** : Rampant précisé — Alliés uniquement, joué depuis le Cimetière (où ils vont à la destruction)

### Issues résiduelles (non bloquantes)

- **M1** : Tutoriel premier combat absent des stories — à ajouter en début d'Epic 2 si nécessaire
- **m1** : Story 8.4 "As a développeur" — reformulation cosmétique optionnelle
- **m2** : Story 2.9 — quelques ACs sans clause When — acceptable pour implémentation

### Ce qui est prêt ✅

- 28/28 FRs couverts dans les epics
- Structure des 8 epics solide, dépendances propres
- 34 des 37 stories sont implémentation-ready
- Architecture complète et cohérente
- Project context optimisé pour les agents IA

### Note finale

Ce rapport a identifié **7 issues** (3 critiques, 2 majeures, 2 mineures) sur 37 stories. Les 3 conflits critiques (C1, C2, C3) concernent tous la même mécanique fondamentale : **le circuit de vie des cartes** (où vont les cartes après résolution). Résoudre ces 3 conflits donne une vision cohérente du système de combat et débloque l'implémentation de la totalité d'Epic 2.
