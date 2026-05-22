using TMPro;
using UnityEngine;
using MonsterCardGame.Gameplay.Cards;
using MonsterCardGame.Gameplay.Combat;

namespace MonsterCardGame.UI.Combat.Board
{
    /// <summary>
    /// Affichage 2D d'une carte (prefab) : nom, coût, mana généré, illustration,
    /// couleur de bord par région, charges d'équipement, highlight de sélection.
    /// Réutilisable par la main, les zones alliés, le cimetière et le loot.
    /// Aucune logique de layout/animation : voir <see cref="CardController"/> pour la main.
    /// </summary>
    public class CardView2D : MonoBehaviour
    {
        [Header("Renderers")]
        [SerializeField] private SpriteRenderer _frameRenderer;
        [SerializeField] private SpriteRenderer _artworkRenderer;

        [Header("Labels")]
        [SerializeField] private TMP_Text _nameLabel;
        [SerializeField] private TMP_Text _costLabel;
        [SerializeField] private TMP_Text _manaGenLabel;
        [SerializeField] private TMP_Text _chargeLabel;

        [Header("Décorations")]
        [SerializeField] private GameObject _selectionHighlight;

        [Header("Couleurs par région")]
        [SerializeField] private Color _colorPlaines  = new(0.55f, 0.75f, 0.45f);
        [SerializeField] private Color _colorMontagne = new(0.65f, 0.55f, 0.45f);
        [SerializeField] private Color _colorMarais   = new(0.45f, 0.55f, 0.40f);
        [SerializeField] private Color _colorRuines   = new(0.60f, 0.55f, 0.50f);
        [SerializeField] private Color _colorForet    = new(0.35f, 0.60f, 0.40f);
        [SerializeField] private Color _colorOcean    = new(0.40f, 0.55f, 0.70f);
        [SerializeField] private Color _colorAucune   = new(0.55f, 0.55f, 0.55f);

        public CardData Data { get; private set; }

        public void Bind(CardData data)
        {
            Data = data;

            if (_nameLabel != null) _nameLabel.text = data.CardName;
            if (_costLabel != null) _costLabel.text = data.ManaCost.ToString();

            if (_manaGenLabel != null)
            {
                bool hasMana = data.ManaGenerated > 0;
                _manaGenLabel.gameObject.SetActive(hasMana);
                if (hasMana) _manaGenLabel.text = $"+{data.ManaGenerated}";
            }

            if (_chargeLabel != null) _chargeLabel.gameObject.SetActive(false);

            if (_artworkRenderer != null) _artworkRenderer.sprite = data.Artwork;
            if (_frameRenderer != null)   _frameRenderer.color   = ColorForRegion(data.Region);

            SetHighlight(false);
        }

        /// <summary>Affiche les données runtime d'un allié (charges d'équipement pour l'instant).</summary>
        public void RefreshInstance(AlliedInstance instance)
        {
            if (_chargeLabel == null) return;

            if (instance.Data.CardType == CardType.Equipement)
            {
                _chargeLabel.gameObject.SetActive(true);
                _chargeLabel.text = instance.DEF.ToString();
            }
            else
            {
                _chargeLabel.gameObject.SetActive(false);
            }
        }

        public void SetHighlight(bool on)
        {
            if (_selectionHighlight != null) _selectionHighlight.SetActive(on);
        }

        private Color ColorForRegion(Region region) => region switch
        {
            Region.Plaines  => _colorPlaines,
            Region.Montagne => _colorMontagne,
            Region.Marais   => _colorMarais,
            Region.Ruines   => _colorRuines,
            Region.Foret    => _colorForet,
            Region.Ocean    => _colorOcean,
            _               => _colorAucune
        };
    }
}
