public enum CharacterType
{
    Player,
    Enemy
}

public enum AttackType
{
    Null,
    Projectile,
    Dash,
    Collision,
    Melee,
    MultiProjectile,
    Shockwave,
    Boomerang
}

public enum EnemyState
{
    Idle,
    Moving,
    Attacking,
    Retreating
}

public enum StatType
{
    Attack,
    Defense,
    Health,
    Speed,
    Null
}

public enum EndDungeonItemType
{
    Attack,
    UNUSED,
    Ability,
    StatBoost
}

public enum SkillTreeNodeBuyResult
{
    Success,
    TooExpensive,
    NotAvailable
}