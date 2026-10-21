namespace MonsterCardGame.Gameplay.Skills
{
    /// <summary>Type d'une compétence. Référence : docs/competences.md.</summary>
    public enum SkillType
    {
        Active,
        Passive
    }

    /// <summary>Rôle d'un monstre ou rôle visé par une compétence. Pour le design et l'IA, jamais affiché.</summary>
    public enum MonsterRole
    {
        Carry,
        Support,
        Tank
    }

    /// <summary>Cible d'une compétence. Seuls les monstres sur le terrain peuvent être ciblés.</summary>
    public enum SkillTarget
    {
        /// <summary>N'importe quel monstre, allié ou ennemi, choisi par le joueur.</summary>
        AnyMonster,
        /// <summary>Un ennemi, choisi par le joueur.</summary>
        Enemy,
        /// <summary>Un allié, y compris ce monstre, choisi par le joueur.</summary>
        Ally,
        /// <summary>Un allié autre que ce monstre, choisi par le joueur.</summary>
        OtherAlly,
        /// <summary>Ce monstre.</summary>
        Self,
        /// <summary>Tous les ennemis : ne choisit pas de cible, ignore la Provocation.</summary>
        AllEnemies,
        /// <summary>Tous les alliés, y compris ce monstre.</summary>
        AllAllies,
        /// <summary>Aucun monstre (par exemple Chasseur de trophées, qui vise un emplacement).</summary>
        None
    }

    /// <summary>Moment où s'applique une compétence passive.</summary>
    public enum PassiveTrigger
    {
        /// <summary>Tant que le monstre est sur le terrain (Provocation, Carapace).</summary>
        Always,
        /// <summary>Au début de chaque phase du camp de ce monstre (Aura apaisante).</summary>
        StartOfOwnPhase,
        /// <summary>Au début du combat, avant la mise en place (Chasseur de trophées).</summary>
        StartOfCombat
    }
}
