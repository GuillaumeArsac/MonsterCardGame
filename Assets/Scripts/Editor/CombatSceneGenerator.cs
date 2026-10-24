using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using MonsterCardGame.Gameplay.Combat;
using MonsterCardGame.Gameplay.Encounters;
using MonsterCardGame.UI.Combat;
using static MonsterCardGame.Editor.UIBuilder;

namespace MonsterCardGame.Editor
{
    /// <summary>
    /// Génère la scène de combat (Canvas) et le prefab de carte. Relancer le menu les reconstruit :
    /// les réglages faits à la main dans la scène ou le prefab sont alors perdus.
    /// Référence : docs/plan-implementation.md (étape 5).
    /// </summary>
    public static class CombatSceneGenerator
    {
        private const string ScenePath      = "Assets/Scenes/Combat.unity";
        private const string CardPrefabPath = "Assets/Prefabs/Combat/MonsterCard.prefab";
        private const string EncounterPath  = "Assets/Data/Encounters/test-horde-de-thrak.asset";

        private const string CardBackgroundPath = "Assets/Images/Others/SYL_fond_carte.png";
        private const string CardBorderPath     = "Assets/Images/Others/SYL_contour_plus_bande_forge.png";
        /// <summary>Encre brune des textes posés sur le cadre (nom, PV).</summary>
        private static readonly Color CardInk = new(0.25f, 0.11f, 0.04f);

        /// <summary>Même proportions que les visuels de la carte (992 × 1386).</summary>
        private static readonly Vector2 CardSize = new(200, 280);
        private const float SkillRowHeight = 20;
        /// <summary>Position verticale de l'illustration dans la fenêtre : 1 = haut de l'image visible, 0,5 = centrée.</summary>
        private const float ArtworkPivotY = 1f;
        private static readonly Vector2 SlotSize = new(210, 300);
        private const float SlotSpacing = 260;


        [MenuItem("MonsterCardGame/Combat/Générer la scène de combat")]
        public static void Generate()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            ContentGenerator.Generate();
            BuildCardPrefab();
            BuildScene();
            AddSceneToBuildSettings(ScenePath);
            AssetDatabase.SaveAssets();

            Debug.Log($"[CombatSceneGenerator] Scène générée : {ScenePath} (prefab de carte : {CardPrefabPath})");
        }

        // ── Prefab de carte ───────────────────────────────────────────────

        /// <summary>
        /// Carte en calques (visuels SYL, 992 × 1386) : fond parchemin, illustration dans la fenêtre du haut,
        /// cadre de la Forge par-dessus. Les positions sont mesurées sur les images, ramenées à la taille de la carte.
        /// </summary>
        private static void BuildCardPrefab()
        {
            var root = new GameObject("MonsterCard", typeof(RectTransform));
            var rect = (RectTransform)root.transform;
            rect.sizeDelta = CardSize;

            var highlight = Stretch(NewImage("Surbrillance", root.transform, Color.white), -6);

            // Fond : il reçoit les clics et prend la teinte du camp
            var frame = Stretch(NewImage("Fond", root.transform, Color.white), 0);
            frame.GetComponent<Image>().sprite = LoadSprite(CardBackgroundPath);
            var button = root.AddComponent<Button>();
            button.targetGraphic = frame.GetComponent<Image>();
            button.transition    = Selectable.Transition.None;

            // Fenêtre transparente du cadre, jusqu'au bandeau du nom. L'illustration la couvre entièrement :
            // elle est recadrée par le masque, et le cadre cache ce qui dépasse sur ses bords
            var window = NewEmpty("Fenêtre", root.transform);
            TopBand(window, 4, 156, inset: 8);
            window.gameObject.AddComponent<RectMask2D>();
            var artwork = NewImage("Illustration", window, Color.white);
            artwork.GetComponent<Image>().raycastTarget = false;
            Center(artwork, Vector2.zero, Vector2.zero);
            // Ancrée en haut : une illustration plus haute que la fenêtre est coupée en bas, pas en haut
            artwork.pivot = new Vector2(0.5f, ArtworkPivotY);
            var fitter = artwork.gameObject.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;

            var border = Stretch(NewImage("Cadre", root.transform, Color.white), 0);
            border.GetComponent<Image>().sprite = LoadSprite(CardBorderPath);
            border.GetComponent<Image>().raycastTarget = false;

            // Effets en cours, en bas de l'illustration, sur un bandeau sombre affiché seulement s'il y en a
            var statusBackground = NewImage("Fond des effets", root.transform, new Color(0, 0, 0, 0.6f));
            statusBackground.GetComponent<Image>().raycastTarget = false;
            TopBand(statusBackground, 123, 18, inset: 18);
            var status = NewText("Effets", statusBackground, "", 11, TextAlignmentOptions.Center);
            status.color = new Color(1f, 0.85f, 0.5f);
            Stretch(status.rectTransform, 0);
            AutoSize(status, 8, 11);

            // PV dans le médaillon de gauche (entre sa position sur le cadre de la Forge et sur celui des Plaines)
            var hp = NewText("PV", root.transform, "PV", 16, TextAlignmentOptions.Center, FontStyles.Bold);
            hp.color = CardInk;
            hp.lineSpacing = -30;
            hp.textWrappingMode = TextWrappingModes.NoWrap;
            hp.overflowMode = TextOverflowModes.Overflow;
            Anchor(hp.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0.5f, 0.5f), new Vector2(31, -30), new Vector2(38, 38));

            // Nom dans le bandeau orange
            var nameLabel = NewText("Nom", root.transform, "Nom", 16, TextAlignmentOptions.Center, FontStyles.Bold);
            nameLabel.color = CardInk;
            // Entre les deux losanges du bandeau
            TopBand(nameLabel.rectTransform, 161, 37, inset: 42);
            AutoSize(nameLabel, 9, 16);

            // Compétences sur le parchemin du bas
            var skills = new GameObject("Compétences", typeof(RectTransform)).GetComponent<RectTransform>();
            skills.SetParent(root.transform, false);
            skills.anchorMin = new Vector2(0, 0);
            skills.anchorMax = new Vector2(1, 0);
            skills.pivot     = new Vector2(0.5f, 0);
            skills.anchoredPosition = new Vector2(0, 9);
            skills.sizeDelta = new Vector2(-28, 3 * SkillRowHeight + 2 * 3);
            // Lignes de hauteur fixe, depuis le haut du parchemin, quel que soit le nombre de compétences
            var layout = skills.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 3;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var rows = Enumerable.Range(0, 3).Select(i => BuildSkillRow(skills, i)).ToArray();

            var view = root.AddComponent<MonsterCardView>();
            Assign(view, "_button", button);
            Assign(view, "_highlight", highlight.GetComponent<Image>());
            Assign(view, "_frame", frame.GetComponent<Image>());
            Assign(view, "_artwork", artwork.GetComponent<Image>());
            Assign(view, "_artworkFitter", fitter);
            Assign(view, "_border", border.GetComponent<Image>());
            Assign(view, "_defaultBorder", border.GetComponent<Image>().sprite);
            Assign(view, "_nameLabel", nameLabel);
            Assign(view, "_hpLabel", hp);
            Assign(view, "_statusLabel", status);
            Assign(view, "_statusBackground", statusBackground.GetComponent<Image>());
            AssignArray(view, "_skillRows", rows);

            EnsureFolder("Assets/Prefabs/Combat");
            PrefabUtility.SaveAsPrefabAsset(root, CardPrefabPath);
            Object.DestroyImmediate(root);
        }

        private static Sprite LoadSprite(string path)
        {
            var sprite = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().FirstOrDefault();
            if (sprite == null) Debug.LogWarning($"[CombatSceneGenerator] Sprite introuvable : {path}");
            return sprite;
        }

        private static SkillRowView BuildSkillRow(Transform parent, int index)
        {
            var row = NewImage($"Compétence {index + 1}", parent, Color.black);
            row.gameObject.AddComponent<LayoutElement>().preferredHeight = SkillRowHeight;
            var button = row.gameObject.AddComponent<Button>();
            button.targetGraphic = row.GetComponent<Image>();
            button.transition = Selectable.Transition.None;

            var label = NewText("Nom", row, "Compétence", 12, TextAlignmentOptions.MidlineLeft);
            Stretch(label.rectTransform, 0);
            label.margin = new Vector4(5, 0, 50, 0);
            AutoSize(label, 9, 12);

            var counter = NewText("Compteur", row, "prête", 10, TextAlignmentOptions.MidlineRight);
            Stretch(counter.rectTransform, 0);
            counter.margin = new Vector4(0, 0, 5, 0);
            counter.color = new Color(0.85f, 0.85f, 0.85f);

            var view = row.gameObject.AddComponent<SkillRowView>();
            Assign(view, "_button", button);
            Assign(view, "_background", row.GetComponent<Image>());
            Assign(view, "_nameLabel", label);
            Assign(view, "_counterLabel", counter);
            return view;
        }

        // ── Scène ─────────────────────────────────────────────────────────

        private static void BuildScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            // Chargé après le changement de scène, qui décharge les objets chargés avant
            var cardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CardPrefabPath).GetComponent<MonsterCardView>();
            var camera = new GameObject("Main Camera", typeof(Camera)).GetComponent<Camera>();
            camera.tag = "MainCamera";
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Background;
            camera.orthographic = true;

            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

            var canvasGo = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            var canvas = canvasGo.transform;

            var background = Stretch(NewImage("Fond", canvas, Background), 0);
            background.GetComponent<Image>().raycastTarget = false;

            // Barre du haut : points, tour, fatigue
            var topBar = NewImage("Barre du haut", canvas, PanelColor);
            Anchor(topBar, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), Vector2.zero, new Vector2(0, 80));
            var score = NewText("Points", topBar, "", 26, TextAlignmentOptions.MidlineLeft, FontStyles.Bold);
            Anchor(score.rectTransform, new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, 0.5f), new Vector2(30, 0), new Vector2(520, 0));
            var turn = NewText("Tour", topBar, "", 32, TextAlignmentOptions.Center, FontStyles.Bold);
            Anchor(turn.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 1), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(760, 0));
            var fatigue = NewText("Fatigue", topBar, "", 24, TextAlignmentOptions.MidlineRight);
            fatigue.color = new Color(1f, 0.6f, 0.3f);
            Anchor(fatigue.rectTransform, new Vector2(1, 0), new Vector2(1, 1), new Vector2(1, 0.5f), new Vector2(-30, 0), new Vector2(520, 0));

            // Panneau de gauche : main adverse et consignes
            var enemyHand = NewText("Main adverse", canvas, "", 24, TextAlignmentOptions.TopLeft, FontStyles.Bold);
            enemyHand.color = new Color(1f, 0.6f, 0.55f);
            Anchor(enemyHand.rectTransform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(30, 330), new Vector2(330, 40));
            var hint = NewText("Consigne", canvas, "", 22, TextAlignmentOptions.TopLeft);
            Anchor(hint.rectTransform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(30, 40), new Vector2(330, 360));

            // Terrain
            var enemySlots  = Enumerable.Range(0, 3).Select(i => BuildSlot(canvas, Side.Enemy, i, 250)).ToArray();
            var playerSlots = Enumerable.Range(0, 3).Select(i => BuildSlot(canvas, Side.Player, i, -75)).ToArray();

            // Main du joueur
            var hand = new GameObject("Main du joueur", typeof(RectTransform)).GetComponent<RectTransform>();
            hand.SetParent(canvas, false);
            Anchor(hand, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-15, 12), new Vector2(1060, 290));
            var handLayout = hand.gameObject.AddComponent<HorizontalLayoutGroup>();
            handLayout.spacing = 14;
            handLayout.childAlignment = TextAnchor.MiddleCenter;
            handLayout.childControlWidth = handLayout.childControlHeight = false;
            handLayout.childForceExpandWidth = handLayout.childForceExpandHeight = false;

            var hidden = new GameObject("Cartes cachées", typeof(RectTransform));
            hidden.transform.SetParent(canvas, false);
            hidden.SetActive(false);

            // Panneau de droite : journal et bouton d'action
            var logPanel = NewImage("Journal", canvas, PanelColor);
            Anchor(logPanel, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-30, 60), new Vector2(400, 640));
            var logText = NewText("Texte", logPanel, "", 18, TextAlignmentOptions.TopLeft);
            Stretch(logText.rectTransform, 14);
            var log = logPanel.gameObject.AddComponent<CombatLog>();
            Assign(log, "_label", logText);

            var (actionButton, actionLabel) = NewButton("Bouton d'action", canvas, "Fin de phase");
            Anchor((RectTransform)actionButton.transform, new Vector2(1, 0), new Vector2(1, 0), new Vector2(1, 0), new Vector2(-80, 120), new Vector2(300, 70));

            // Fin du combat
            var endPanel = Stretch(NewImage("Fin du combat", canvas, new Color(0, 0, 0, 0.75f)), 0);
            var box = NewImage("Cadre", endPanel, PanelColor);
            Anchor(box, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(720, 360));
            var endTitle = NewText("Titre", box, "Victoire", 72, TextAlignmentOptions.Center, FontStyles.Bold);
            Anchor(endTitle.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, -30), new Vector2(0, 90));
            var endReason = NewText("Raison", box, "", 28, TextAlignmentOptions.Center);
            Anchor(endReason.rectTransform, new Vector2(0, 0.5f), new Vector2(1, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 10), new Vector2(-60, 80));
            var (replay, _) = NewButton("Rejouer", box, "Rejouer", color: SecondaryButtonColor);
            Anchor((RectTransform)replay.transform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-150, 40), new Vector2(260, 64));
            var (continueButton, continueLabel) = NewButton("Continuer", box, "Continuer");
            Anchor((RectTransform)continueButton.transform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(150, 40), new Vector2(260, 64));

            AddRightClickHint(canvas);
            AddCardDetailsPopup(canvas, cardPrefab);

            // Contrôleur
            var screen = canvasGo.AddComponent<CombatScreen>();
            AssignArray(screen, "_playerTeam", ContentGenerator.LoadTeam("EquipeA").ToArray<Object>());
            Assign(screen, "_encounter", AssetDatabase.LoadAssetAtPath<EncounterData>(EncounterPath));
            Assign(screen, "_cardPrefab", cardPrefab);            AssignArray(screen, "_playerSlots", playerSlots);
            AssignArray(screen, "_enemySlots", enemySlots);
            Assign(screen, "_playerHand", hand);
            Assign(screen, "_hiddenCards", hidden.transform);
            Assign(screen, "_scoreLabel", score);
            Assign(screen, "_turnLabel", turn);
            Assign(screen, "_fatigueLabel", fatigue);
            Assign(screen, "_enemyHandLabel", enemyHand);
            Assign(screen, "_hintLabel", hint);
            Assign(screen, "_log", log);
            Assign(screen, "_actionButton", actionButton);
            Assign(screen, "_actionButtonLabel", actionLabel);
            Assign(screen, "_endPanel", endPanel.gameObject);
            Assign(screen, "_endTitle", endTitle);
            Assign(screen, "_endReason", endReason);
            Assign(screen, "_replayButton", replay);
            Assign(screen, "_continueButton", continueButton);
            Assign(screen, "_continueLabel", continueLabel);

            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        private static SlotView BuildSlot(Transform canvas, Side side, int index, float y)
        {
            string name = side == Side.Player ? "Emplacement joueur" : "Emplacement adverse";
            var slot = NewImage($"{name} {index + 1}", canvas, new Color(1, 1, 1, 0.06f));
            Anchor(slot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                   new Vector2((index - 1) * SlotSpacing, y), SlotSize);
            var button = slot.gameObject.AddComponent<Button>();
            button.targetGraphic = slot.GetComponent<Image>();
            button.transition = Selectable.Transition.None;

            var mark = NewText("Marque", slot, "", 18, TextAlignmentOptions.Center, FontStyles.Bold);
            mark.color = new Color(1f, 0.82f, 0.25f);
            Anchor(mark.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 0), new Vector2(0, 4), new Vector2(0, 24));

            var anchor = new GameObject("Carte", typeof(RectTransform)).GetComponent<RectTransform>();
            anchor.SetParent(slot, false);
            anchor.sizeDelta = Vector2.zero;

            var view = slot.gameObject.AddComponent<SlotView>();
            var so = new SerializedObject(view);
            so.FindProperty("_side").enumValueIndex = (int)side;
            so.FindProperty("_index").intValue = index;
            so.ApplyModifiedPropertiesWithoutUndo();
            Assign(view, "_button", button);
            Assign(view, "_background", slot.GetComponent<Image>());
            Assign(view, "_markLabel", mark);
            Assign(view, "_cardAnchor", anchor);
            return view;
        }

    }
}
