using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using MonsterCardGame.Gameplay.Combat;

namespace MonsterCardGame.UI.Combat.Board
{
    /// <summary>
    /// Gère une rangée d'alliés (prefabs 2D) pour un camp. Réutilisable côté joueur
    /// et côté monstre : instancier deux fois et appeler <see cref="Sync"/> chaque frame
    /// avec la liste appropriée du contexte. Les clics sont publiés via events ;
    /// la logique (ciblage, attaque) est décidée par le binder.
    /// </summary>
    public class AlliesController : MonoBehaviour
    {
        [Header("Références")]
        [SerializeField] private AllyView _allyPrefab;
        [SerializeField] private Transform _anchor;
        [SerializeField] private Camera _camera;

        [Header("Layout (rangée)")]
        [SerializeField, Tooltip("Espacement horizontal entre alliés (unités world)")]
        private float _spacing = 2.2f;

        [SerializeField] private Vector3 _cardScale = Vector3.one;
        [SerializeField] private float _moveTime = 0.2f;

        public event Action<AlliedInstance> AllyClicked;
        public event Action<AlliedInstance> AllyRightClicked;

        private readonly List<AllyView> _views = new();

        private void Awake()
        {
            if (_camera == null) _camera = Camera.main;
        }

        private void Update() => HandleInput();

        // ── Synchronisation (diff par référence d'instance) ───────────────

        public void Sync(IReadOnlyList<AlliedInstance> allies)
        {
            bool structureChanged = _views.Count != allies.Count;
            if (!structureChanged)
            {
                for (int i = 0; i < allies.Count; i++)
                {
                    if (_views[i].Instance != allies[i]) { structureChanged = true; break; }
                }
            }

            if (structureChanged) Rebuild(allies);

            // Rafraîchit toujours l'état runtime (charges, sommeil).
            for (int i = 0; i < _views.Count; i++) _views[i].Refresh();
        }

        private void Rebuild(IReadOnlyList<AlliedInstance> allies)
        {
            var pool = new Dictionary<AlliedInstance, AllyView>();
            foreach (var v in _views)
                if (v != null && v.Instance != null) pool[v.Instance] = v;

            var ordered = new List<AllyView>(allies.Count);
            foreach (var instance in allies)
            {
                if (pool.TryGetValue(instance, out var existing))
                {
                    ordered.Add(existing);
                    pool.Remove(instance);
                }
                else
                {
                    var view = Instantiate(_allyPrefab, _anchor);
                    view.Bind(instance);
                    ordered.Add(view);
                }
            }

            // Reste du pool = alliés retirés du terrain.
            foreach (var leftover in pool.Values)
                if (leftover != null) Destroy(leftover.gameObject);

            _views.Clear();
            _views.AddRange(ordered);
            Layout();
        }

        private void Layout()
        {
            int count = _views.Count;
            for (int i = 0; i < count; i++)
            {
                float x = (i - (count - 1) * 0.5f) * _spacing;
                var target = new Vector3(x, 0f, i * 0.01f);

                var t = _views[i].transform;
                t.DOKill();
                t.localScale = _cardScale;
                t.DOLocalMove(target, _moveTime).SetEase(Ease.OutCubic);
            }
        }

        // ── Input (raycast) ───────────────────────────────────────────────

        private void HandleInput()
        {
            var mouse = Mouse.current;
            if (mouse == null || _camera == null) return;

            if (mouse.leftButton.wasPressedThisFrame)
            {
                var ally = AllyUnderCursor(mouse);
                if (ally != null) AllyClicked?.Invoke(ally);
            }
            else if (mouse.rightButton.wasPressedThisFrame)
            {
                var ally = AllyUnderCursor(mouse);
                if (ally != null) AllyRightClicked?.Invoke(ally);
            }
        }

        private AlliedInstance AllyUnderCursor(Mouse mouse)
        {
            Vector3 world = _camera.ScreenToWorldPoint(mouse.position.ReadValue());
            var hit = Physics2D.Raycast(world, Vector2.zero);
            if (hit.collider == null) return null;

            var view = hit.collider.GetComponentInParent<AllyView>();
            return (view != null && _views.Contains(view)) ? view.Instance : null;
        }

        // ── États visuels (pilotés par le binder) ─────────────────────────

        public void SetTargetable(AlliedInstance instance, bool on)
        {
            var v = Find(instance);
            if (v != null) v.SetTargetable(on);
        }

        public void ClearTargetable()
        {
            foreach (var v in _views) v.SetTargetable(false);
        }

        public void SetAttacker(AlliedInstance instance)
        {
            foreach (var v in _views) v.SetAttacker(v.Instance == instance);
        }

        public void ClearAttacker()
        {
            foreach (var v in _views) v.SetAttacker(false);
        }

        private AllyView Find(AlliedInstance instance)
        {
            foreach (var v in _views)
                if (v.Instance == instance) return v;
            return null;
        }

        private void OnDestroy()
        {
            foreach (var v in _views)
                if (v != null) v.transform.DOKill();
        }
    }
}
