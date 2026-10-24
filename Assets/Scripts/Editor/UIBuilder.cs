using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using MonsterCardGame.UI.Combat;

namespace MonsterCardGame.Editor
{
    /// <summary>Outils communs aux générateurs d'écrans : création d'éléments Canvas et branchement des références.</summary>
    public static class UIBuilder
    {
        public static readonly Color Background  = new(0.08f, 0.09f, 0.11f);
        public static readonly Color PanelColor  = new(0.05f, 0.06f, 0.08f, 0.9f);
        public static readonly Color ButtonColor = new(0.55f, 0.40f, 0.12f);
        public static readonly Color SecondaryButtonColor = new(0.20f, 0.24f, 0.34f);

        /// <summary>Nouvelle scène avec une caméra, un EventSystem et un Canvas en 1920 × 1080. Retourne le Canvas.</summary>
        public static (Scene scene, GameObject canvas) NewCanvasScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camera = new GameObject("Main Camera", typeof(Camera)).GetComponent<Camera>();
            camera.tag = "MainCamera";
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Background;
            camera.orthographic = true;

            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

            var canvas = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvas.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            var background = Stretch(NewImage("Fond", canvas.transform, Background), 0);
            background.GetComponent<Image>().raycastTarget = false;

            return (scene, canvas);
        }

        /// <summary>Barre du haut avec un titre centré. Retourne la barre.</summary>
        public static (RectTransform bar, TextMeshProUGUI title) TopBar(Transform canvas, string title)
        {
            var bar = NewImage("Barre du haut", canvas, PanelColor);
            Anchor(bar, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), Vector2.zero, new Vector2(0, 80));
            var label = NewText("Titre", bar, title, 32, TextAlignmentOptions.Center, FontStyles.Bold);
            Anchor(label.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 1), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(900, 0));
            return (bar, label);
        }

        /// <summary>
        /// Panneau de détail des compétences, ouvert par un clic droit sur une carte.
        /// À ajouter en dernier, pour qu'il passe devant le reste de l'écran.
        /// </summary>
        /// <summary>Taille de la carte agrandie dans le panneau de détail.</summary>
        private const float DetailsCardScale = 1.6f;

        public static CardDetailsPopup AddCardDetailsPopup(Transform canvas, MonsterCardView cardPrefab)
        {
            // Le support reste actif (il écoute Échap) ; seul le fond s'affiche et se cache
            var holder = Stretch(NewEmpty("Détail des cartes", canvas), 0);
            var overlay = Stretch(NewImage("Fond", holder, new Color(0, 0, 0, 0.65f)), 0);

            // Panneau : la carte à gauche, le nom et les compétences à droite
            var panel = NewImage("Panneau", overlay, new Color(0.07f, 0.08f, 0.11f, 1f));
            Center(panel, Vector2.zero, new Vector2(1180, 0));
            var layout = panel.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(30, 30, 26, 26);
            layout.spacing = 34;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandWidth = layout.childForceExpandHeight = false;
            panel.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var cardSize = (Vector2)((RectTransform)cardPrefab.transform).sizeDelta * DetailsCardScale;
            var cardAnchor = NewEmpty("Carte", panel);
            var cardLayout = cardAnchor.gameObject.AddComponent<LayoutElement>();
            cardLayout.preferredWidth  = cardLayout.minWidth  = cardSize.x;
            cardLayout.preferredHeight = cardLayout.minHeight = cardSize.y;

            var column = NewEmpty("Compétences", panel);
            column.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;
            var columnLayout = column.gameObject.AddComponent<VerticalLayoutGroup>();
            columnLayout.spacing = 14;
            columnLayout.childAlignment = TextAnchor.UpperLeft;
            columnLayout.childControlWidth = columnLayout.childControlHeight = true;
            columnLayout.childForceExpandWidth = true;
            columnLayout.childForceExpandHeight = false;

            var title = NewText("Titre", column, "Monstre", 34, TextAlignmentOptions.Left, FontStyles.Bold);
            var body  = NewText("Texte", column, "", 22, TextAlignmentOptions.TopLeft);
            body.overflowMode = TextOverflowModes.Overflow;
            var footer = NewText("Fermer", column, "Clic ou Échap pour fermer", 16, TextAlignmentOptions.Left);
            footer.color = new Color(0.6f, 0.6f, 0.65f);

            var popup = holder.gameObject.AddComponent<CardDetailsPopup>();
            Assign(popup, "_root", overlay.gameObject);
            Assign(popup, "_titleLabel", title);
            Assign(popup, "_bodyLabel", body);
            Assign(popup, "_cardPrefab", cardPrefab);
            Assign(popup, "_cardAnchor", cardAnchor);
            return popup;
        }

        /// <summary>Rappel en bas à gauche de l'écran : le clic droit montre le détail des compétences.</summary>
        public static TextMeshProUGUI AddRightClickHint(Transform canvas)
        {
            var hint = NewText("Aide clic droit", canvas, "Clic droit sur une carte : détail de ses compétences", 18, TextAlignmentOptions.BottomLeft);
            hint.color = new Color(0.65f, 0.68f, 0.75f);
            // 330 px de large : reste dans la colonne de gauche, sans passer sous la main du joueur en combat
            Anchor(hint.rectTransform, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(30, 24), new Vector2(330, 60));
            return hint;
        }

        public static void AddSceneToBuildSettings(string path)
        {
            var scenes = EditorBuildSettings.scenes.ToList();
            if (scenes.Any(s => s.path == path)) return;
            scenes.Add(new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        public static RectTransform NewImage(string name, Transform parent, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = color;
            return (RectTransform)go.transform;
        }

        public static RectTransform NewEmpty(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        public static TextMeshProUGUI NewText(string name, Transform parent, string text, float size,
                                              TextAlignmentOptions alignment, FontStyles style = FontStyles.Normal)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var label = go.GetComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = size;
            label.alignment = alignment;
            label.fontStyle = style;
            label.color = Color.white;
            label.raycastTarget = false;
            label.textWrappingMode = TextWrappingModes.Normal;
            label.overflowMode = TextOverflowModes.Ellipsis;
            return label;
        }

        /// <summary>Réduit le texte jusqu'à <paramref name="min"/> pour qu'il tienne sur une ligne.</summary>
        public static void AutoSize(TextMeshProUGUI label, float min, float max)
        {
            label.enableAutoSizing = true;
            label.fontSizeMin = min;
            label.fontSizeMax = max;
            label.textWrappingMode = TextWrappingModes.NoWrap;
        }

        public static (Button, TMP_Text) NewButton(string name, Transform parent, string text, float fontSize = 28, Color? color = null)
        {
            var rect = NewImage(name, parent, color ?? ButtonColor);
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = rect.GetComponent<Image>();
            var label = NewText("Texte", rect, text, fontSize, TextAlignmentOptions.Center, FontStyles.Bold);
            Stretch(label.rectTransform, 0);
            label.margin = new Vector4(10, 0, 10, 0);
            return (button, label);
        }

        public static RectTransform Stretch(RectTransform rect, float inset)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(inset, inset);
            rect.offsetMax = new Vector2(-inset, -inset);
            return rect;
        }

        /// <summary>Bande horizontale en haut du parent, à <paramref name="top"/> pixels du bord.</summary>
        public static void TopBand(RectTransform rect, float top, float height, float inset = 6)
        {
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot     = new Vector2(0.5f, 1);
            rect.anchoredPosition = new Vector2(0, -top);
            rect.sizeDelta = new Vector2(-2 * inset, height);
        }

        public static void Anchor(RectTransform rect, Vector2 min, Vector2 max, Vector2 pivot, Vector2 position, Vector2 size)
        {
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.pivot = pivot;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        /// <summary>Place l'élément par rapport au centre de son parent.</summary>
        public static void Center(RectTransform rect, Vector2 position, Vector2 size)
            => Anchor(rect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), position, size);

        public static void Assign(Object target, string field, Object value)
        {
            var so = new SerializedObject(target);
            var property = so.FindProperty(field);
            if (property == null) throw new System.ArgumentException($"{target.GetType().Name} n'a pas de champ {field}");
            property.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void AssignArray(Object target, string field, Object[] values)
        {
            var so = new SerializedObject(target);
            var property = so.FindProperty(field);
            if (property == null) throw new System.ArgumentException($"{target.GetType().Name} n'a pas de champ {field}");
            property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder)) return;
            var parent = System.IO.Path.GetDirectoryName(folder)?.Replace('\\', '/');
            if (!string.IsNullOrEmpty(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, System.IO.Path.GetFileName(folder));
        }
    }
}
