using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using MonsterCardGame.UI.Combat;
using MonsterCardGame.UI.Progression;
using static MonsterCardGame.Editor.UIBuilder;

namespace MonsterCardGame.Editor
{
    /// <summary>
    /// Génère les écrans Équipe et Dépeçage (Canvas). Ils utilisent le prefab de carte de la scène de combat.
    /// Relancer le menu les reconstruit : les réglages faits à la main y sont perdus.
    /// Référence : docs/plan-implementation.md (étape 6).
    /// </summary>
    public static class ProgressionScenesGenerator
    {
        private const string TeamScenePath     = "Assets/Scenes/Team.unity";
        private const string SkinningScenePath = "Assets/Scenes/Skinning.unity";
        private const string CardPrefabPath    = "Assets/Prefabs/Combat/MonsterCard.prefab";

        private static readonly Vector2 CardSize = new(190, 280);

        [MenuItem("MonsterCardGame/Progression/Générer les écrans Équipe et Dépeçage")]
        public static void Generate()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (AssetDatabase.LoadAssetAtPath<GameObject>(CardPrefabPath) == null)
            {
                Debug.LogError($"[ProgressionScenesGenerator] Prefab de carte introuvable : générer d'abord la scène de combat");
                return;
            }

            BuildTeamScene();
            BuildSkinningScene();
            AddSceneToBuildSettings(TeamScenePath);
            AddSceneToBuildSettings(SkinningScenePath);
            AssetDatabase.SaveAssets();

            Debug.Log($"[ProgressionScenesGenerator] Scènes générées : {TeamScenePath}, {SkinningScenePath}");
        }

        /// <summary>Contenu, scène de combat, puis écrans Équipe et Dépeçage.</summary>
        [MenuItem("MonsterCardGame/Tout générer", priority = 0)]
        public static void GenerateAll()
        {
            CombatSceneGenerator.Generate();
            Generate();
        }

        private static MonsterCardView CardPrefab()
            => AssetDatabase.LoadAssetAtPath<GameObject>(CardPrefabPath).GetComponent<MonsterCardView>();

        /// <summary>Grille de cartes, 6 par ligne, centrée en haut de la zone.</summary>
        private static RectTransform CardGrid(Transform parent, Vector2 position, Vector2 size)
        {
            var grid = NewEmpty("Collection", parent);
            Center(grid, position, size);
            var layout = grid.gameObject.AddComponent<GridLayoutGroup>();
            layout.cellSize = CardSize;
            layout.spacing = new Vector2(16, 16);
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = 6;
            return grid;
        }

        /// <summary>Colonne de boutons. Le premier sert de modèle, copié par l'écran pour chaque élément.</summary>
        private static Button ButtonColumn(Transform parent, string name, Vector2 position, Vector2 size)
        {
            var column = NewEmpty(name, parent);
            Center(column, position, size);
            var layout = column.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 12;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var (template, _) = NewButton("Modèle", column, "Bouton", 22, SecondaryButtonColor);
            ((RectTransform)template.transform).sizeDelta = new Vector2(0, 60);
            return template;
        }

        // ── Équipe ────────────────────────────────────────────────────────

        private static void BuildTeamScene()
        {
            var (scene, canvasGo) = NewCanvasScene();
            var canvas = canvasGo.transform;
            TopBar(canvas, "Équipe");

            // Alignés sur le bord gauche des cartes (6 colonnes de 190 + 5 × 16, centrées sur la grille)
            var teamLabel = NewText("Équipe", canvas, "", 30, TextAlignmentOptions.MidlineLeft, FontStyles.Bold);
            Center(teamLabel.rectTransform, new Vector2(-570, 410), new Vector2(560, 50));
            var hint = NewText("Consigne", canvas, "", 22, TextAlignmentOptions.MidlineLeft);
            Center(hint.rectTransform, new Vector2(-240, 360), new Vector2(1220, 50));

            var grid = CardGrid(canvas, new Vector2(-240, -60), new Vector2(1240, 760));

            var (worldMap, _) = NewButton("Carte du monde", canvas, "Carte du monde", 24);
            Anchor((RectTransform)worldMap.transform, new Vector2(1, 0), new Vector2(1, 0), new Vector2(1, 0), new Vector2(-50, 50), new Vector2(420, 64));

            var screen = canvasGo.AddComponent<TeamScreen>();
            Assign(screen, "_cardPrefab", CardPrefab());
            Assign(screen, "_grid", grid);
            Assign(screen, "_teamLabel", teamLabel);
            Assign(screen, "_hintLabel", hint);
            Assign(screen, "_worldMapButton", worldMap);

            EditorSceneManager.SaveScene(scene, TeamScenePath);
        }

        // ── Dépeçage ──────────────────────────────────────────────────────

        private static void BuildSkinningScene()
        {
            var (scene, canvasGo) = NewCanvasScene();
            var canvas = canvasGo.transform;
            var (_, title) = TopBar(canvas, "Dépeçage");

            // Monstre dépecé et récompenses, à gauche
            var enemyTitle = NewText("Monstre dépecé", canvas, "Monstre tiré", 24, TextAlignmentOptions.Center, FontStyles.Bold);
            Center(enemyTitle.rectTransform, new Vector2(-750, 380), new Vector2(360, 40));
            var enemyAnchor = NewEmpty("Carte dépecée", canvas);
            Center(enemyAnchor, new Vector2(-750, 200), CardSize);
            var rewardsTitle = NewText("Titre des récompenses", canvas, "Récompense", 24, TextAlignmentOptions.Center, FontStyles.Bold);
            Center(rewardsTitle.rectTransform, new Vector2(-750, 20), new Vector2(360, 40));
            var template = ButtonColumn(canvas, "Récompenses", new Vector2(-750, -170), new Vector2(360, 320));

            // Monstres du joueur, au centre
            var hint = NewText("Consigne", canvas, "", 24, TextAlignmentOptions.MidlineLeft);
            Center(hint.rectTransform, new Vector2(170, 400), new Vector2(1240, 70));
            var grid = CardGrid(canvas, new Vector2(170, -70), new Vector2(1240, 760));

            var (confirm, confirmLabel) = NewButton("Confirmer", canvas, "Confirmer");
            Anchor((RectTransform)confirm.transform, new Vector2(1, 0), new Vector2(1, 0), new Vector2(1, 0), new Vector2(-50, 40), new Vector2(300, 70));

            var screen = canvasGo.AddComponent<SkinningScreen>();
            Assign(screen, "_cardPrefab", CardPrefab());
            Assign(screen, "_enemyCardAnchor", enemyAnchor);
            Assign(screen, "_grid", grid);
            Assign(screen, "_titleLabel", title);
            Assign(screen, "_hintLabel", hint);
            Assign(screen, "_rewardButtonTemplate", template);
            Assign(screen, "_confirmButton", confirm);
            Assign(screen, "_confirmLabel", confirmLabel);

            EditorSceneManager.SaveScene(scene, SkinningScenePath);
        }
    }
}
