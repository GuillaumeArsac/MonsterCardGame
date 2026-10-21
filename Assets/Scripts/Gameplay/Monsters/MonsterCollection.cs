using System;
using System.Collections.Generic;
using System.Linq;
using MonsterCardGame.Core;
using MonsterCardGame.Core.Services;

namespace MonsterCardGame.Gameplay.Monsters
{
    /// <summary>
    /// Les monstres du joueur et son équipe de combat (5 monstres choisis dans la collection).
    /// Référence : docs/progression.md (Collection).
    /// </summary>
    public class MonsterCollection : IService
    {
        private readonly List<OwnedMonster> _monsters = new();
        private readonly List<OwnedMonster> _team = new();

        public IReadOnlyList<OwnedMonster> Monsters => _monsters;
        public IReadOnlyList<OwnedMonster> Team     => _team;

        /// <summary>Vrai quand l'équipe a ses 5 monstres et peut combattre.</summary>
        public bool IsTeamComplete => _team.Count == GameRules.TeamSize;

        /// <summary>Déclenché à chaque changement de la collection ou de l'équipe.</summary>
        public event Action Changed;

        /// <summary>Nouvelle partie : la collection de départ, et une équipe faite de ses premiers monstres.</summary>
        public void StartNewGame(IEnumerable<MonsterCardData> starters)
        {
            _monsters.Clear();
            _team.Clear();
            foreach (var card in starters) _monsters.Add(new OwnedMonster(card));
            _team.AddRange(_monsters.Take(GameRules.TeamSize));
            Changed?.Invoke();
        }

        /// <summary>Recharge une collection sauvegardée. Les index d'équipe invalides sont ignorés.</summary>
        public void Load(IEnumerable<OwnedMonster> monsters, IEnumerable<int> teamIndices)
        {
            _monsters.Clear();
            _team.Clear();
            _monsters.AddRange(monsters);
            foreach (int i in teamIndices.Distinct())
                if (i >= 0 && i < _monsters.Count && _team.Count < GameRules.TeamSize)
                    _team.Add(_monsters[i]);
            Changed?.Invoke();
        }

        /// <summary>Nouveau monstre, par exemple gagné par une quête secondaire.</summary>
        public void Add(OwnedMonster monster)
        {
            _monsters.Add(monster ?? throw new ArgumentNullException(nameof(monster)));
            Changed?.Invoke();
        }

        public bool IsInTeam(OwnedMonster monster) => _team.Contains(monster);

        /// <summary>Ajoute un monstre de la collection à l'équipe. Faux si l'équipe est complète.</summary>
        public bool AddToTeam(OwnedMonster monster)
        {
            if (!_monsters.Contains(monster)) throw new ArgumentException($"{monster?.Name} n'est pas dans la collection");
            if (_team.Contains(monster) || IsTeamComplete) return false;
            _team.Add(monster);
            Changed?.Invoke();
            return true;
        }

        public bool RemoveFromTeam(OwnedMonster monster)
        {
            if (!_team.Remove(monster)) return false;
            Changed?.Invoke();
            return true;
        }

        /// <summary>Index des monstres de l'équipe dans la collection, pour la sauvegarde.</summary>
        public IEnumerable<int> TeamIndices => _team.Select(m => _monsters.IndexOf(m));

        /// <summary>Prévient les écouteurs qu'un monstre a changé (compétence ou boost reçu).</summary>
        public void NotifyMonsterChanged() => Changed?.Invoke();
    }
}
