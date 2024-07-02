using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MyFunctions;
using TMPro;

public abstract class BasicCharacter : MonoBehaviour, IAttackable, ICanAttack
{
    [ReadOnly] public int level;
    [ReadOnly] public int attack;
    [ReadOnly, SerializeField] protected int defense;
    [ReadOnly, SerializeField] protected float speed;
    [ReadOnly, SerializeField]protected int health = 1;
    [ReadOnly] public int maxHealth;
    

    protected BasicCharacterSO basicCharacterSO;

    [HideInInspector] public Animator anim;
    [HideInInspector] public Rigidbody2D rb;
    protected SpriteRenderer sr;

    [HideInInspector]public CharacterType characterType { get; set; }

    [HideInInspector] public GameObject graphicsObject;

    protected Sprite defaultSprite;

    protected AttackType attackObjectType;

    protected HealthBar healthBar;
    private Vector3 healthBarScale;

    protected Collision2D collisionAttackTarget;

    protected bool dead;

    [ReadOnly] public float speedMultiplier = 1f;

    public virtual void Start()
    {
        bool graphicsExists = false;
        speedMultiplier = Mathf.Clamp(speedMultiplier, 1, Mathf.Infinity);
        foreach (Transform t in GetComponentsInHierarchy<Transform>(transform))
        {
            if (t.CompareTag(graphicsObject.tag))
            {
                graphicsExists = true;
                break;
            }
        }

        if (!graphicsExists)
        {
            GameObject graphics = Instantiate(graphicsObject, transform);
            graphics.transform.localPosition = Vector3.zero;
            anim = GetComponentInHierarchy<Animator>(transform);
            sr = GetComponentInHierarchy<SpriteRenderer>(transform);
            defaultSprite = sr.sprite;
        }
        else
        {
            sr.sprite = defaultSprite;
        }

        rb = GetComponentInHierarchy<Rigidbody2D>(transform);

        characterType = basicCharacterSO.characterType;
        if (!graphicsExists)
        {
            UpgradeStats();
            DetermineLevel();
            DetermineStats();
            DetermineHealthStats();
        }     
        if (healthBar == null)
        {         
            CreateHealthBar(basicCharacterSO);
        }
        attackObjectType = DetermineAttackType(basicCharacterSO.attackSO);
    }

    public int GetAttack()
    {
        return attack;
    }

    public int GetLevel()
    {
        return level;
    }

    private void UpgradeStats()
    {
        int uAttack = basicCharacterSO.baseAttack;
        int uDefense = basicCharacterSO.baseDefense;
        int uHealth = basicCharacterSO.baseHealth;

        if (characterType == CharacterType.Enemy)
        {
            basicCharacterSO.adjustedBaseHealth = uHealth;
            basicCharacterSO.adjustedBaseAttack = uAttack;
            basicCharacterSO.adjustedBaseDefense = uDefense;
            return;
        }

        PlayerSO playerSO = (PlayerSO)basicCharacterSO;

        //All stat boost
        for (int i = 0; i < playerSO.allSkillBoosts.Length; i++)
        {
            if (SavedDataManager.instance.IsSkillPurchased("statBoostAll" + (i + 1)))
            {
                uHealth += playerSO.allSkillBoosts[i].x;
                uAttack += playerSO.allSkillBoosts[i].y;
                uDefense += playerSO.allSkillBoosts[i].z;
            }
            else
            {
                break;
            }
        }

        //Health
        for (int i = 0; i < playerSO.healthSkillBoosts.Length; i++)
        {
            if (SavedDataManager.instance.IsSkillPurchased("healthBoost" + (i + 1)))
            {
                uHealth += playerSO.healthSkillBoosts[i];
            }
            else
            {
                break;
            }
        }

        //Attack
        for (int i = 0; i < playerSO.attackSkillBoosts.Length; i++)
        {
            if (SavedDataManager.instance.IsSkillPurchased("attackBoost" + (i + 1)))
            {
                uAttack += playerSO.attackSkillBoosts[i];
            }
            else
            {
                break;
            }
        }

        //Defense
        for (int i = 0; i < playerSO.defenseSkillBoosts.Length; i++)
        {
            if (SavedDataManager.instance.IsSkillPurchased("defenseBoost" + (i + 1)))
            {
                uDefense += playerSO.defenseSkillBoosts[i];
            }
            else
            {
                break;
            }
        }

        playerSO.adjustedBaseAttack = uAttack;
        playerSO.adjustedBaseDefense = uDefense;
        playerSO.adjustedBaseHealth = uHealth;
    }

    private void DetermineLevel()
    {
        if (characterType == CharacterType.Player)
        {
            level = ExperienceToLevel(SavedDataManager.instance.PlayerExperience);
        }
        else if (characterType == CharacterType.Enemy)
        {
            float floor = EndDungeonManager.instance.dungeonLevel;
            float playerLevel = ExperienceToLevel(SavedDataManager.instance.PlayerExperience);
            float rank = SavedDataManager.instance.GetCurrentDungeonRank();

            
            float tempLevel = UnityEngine.Random.Range(playerLevel - 3, playerLevel + 3);

            tempLevel = Mathf.Clamp(tempLevel, (rank * 10f / 5f), (rank * 10f * 2f));

            tempLevel *= Mathf.Pow(1.2f, floor);        
            
            if (playerLevel <= 6 && SavedDataManager.instance.PrestigeLevel <= 0)
            {
                tempLevel = Mathf.Clamp(tempLevel, 0, 6);
            }

            tempLevel *= SavedDataManager.instance.PrestigeLevel + 1;

            level = (int)tempLevel;
        }
    }

    public void DetermineStats()
    {
        DetermineStats(basicCharacterSO.adjustedBaseAttack, basicCharacterSO.adjustedBaseDefense, basicCharacterSO.speed);
    }

    public void DetermineStats(int a, int d)
    {
        DetermineStats(a, d, basicCharacterSO.speed);
    }

    public void DetermineStats(int a, int d, float s)
    {       
        speed = s * speedMultiplier;
        attack = CalculateStat(a, StatType.Attack);
        defense = CalculateStat(d, StatType.Defense);       
    }

    protected void DetermineHealthStats()
    {
        DetermineHealthStats(basicCharacterSO.adjustedBaseHealth);
    }

    protected void DetermineHealthStats(int h)
    {
        health = CalculateStat(h, StatType.Health);
        maxHealth = health;
    }

    protected int ExperienceToLevel(int experience)
    {
        //int newLevel = (int)CubeRoot((float)experience);
        int newLevel = (int)(experience / 1000f);

        if (newLevel == 0)
        {
            return 1;
        }
        else
        {
            return newLevel;
        }
    }

    private int CalculateStat(int baseStat, StatType type)
    {
        int iv = DetermineIV(characterType == CharacterType.Player, type);
        int result = 0;
        if (type == StatType.Health)
        {
            result = baseStat + iv;
            result *= 2;
            result *= level;
            result /= 100;
            result += level;
            result += 10;
            return result;
        }
        else
        {
            result = baseStat + iv;
            result *= 2;
            result *= level;
            result /= 100;
            result += 5;
            return result;
        }
    }

    private int DetermineIV(bool isPlayer, StatType type)
    {
        if (isPlayer)
        {
            PlayerSO p = (PlayerSO)basicCharacterSO;
            if (type == StatType.Attack)
            {
                return p.attackIV;
            }
            else if (type == StatType.Defense)
            {
                return p.defenseIV;
            }
            else if (type == StatType.Health)
            {
                return p.healthIV;
            }
            else
            {
                return -1;
            }
        }
        else
        {
            return UnityEngine.Random.Range(0, 16);
        }
    }

    protected AttackType DetermineAttackType(BasicAttackSO so)
    {
        BasicAttack ba = so.attackObject;

        GameObject baObj = Instantiate(ba.gameObject, transform);
        baObj.GetComponent<BasicAttack>().basicAttackSO = so;

        if (ba is MultiProjectileAttack)
        {
            return AttackType.MultiProjectile;
        }
        else if (ba is ProjectileAttack)
        {        
            return AttackType.Projectile;
        }
        else if (ba is DashAttack)
        {
            return AttackType.Dash;
        }
        else if (ba is CollisionAttack)
        {
            return AttackType.Collision;
        }
        else if (ba is MeleeAttack)
        {          
            foreach (Transform t in GetComponentsInHierarchy<Transform>(transform))
            {
                if (t.CompareTag("MeleeSpawnPoint"))
                {
                    baObj.transform.SetParent(t);
                    baObj.transform.localPosition = ba.transform.localPosition;
                    baObj.transform.localScale = Vector3.one;
                    break;
                }
            }
            return AttackType.Melee;
        }
        else if (ba is ShockwaveAttack)
        {
            return AttackType.Shockwave;
        }
        else if (ba is BoomerangAttack)
        {
            return AttackType.Boomerang;
        }
        Debug.LogError("Attack Type was Null on " + ba);
        return AttackType.Null;
    }

    public abstract void Attack();

    public virtual void Die()
    {
        Destroy(gameObject);
    }

    public void AddHealth(int h)
    {
        SetHealth(health + h);
    }

    public void SetHealth(int h)
    {
        health = (int)Mathf.Clamp(h, 0, maxHealth);
        healthBar.SetHealth(health);
    }

    public int GetHealth()
    {
        return health;
    }

    protected void CreateHealthBar(BasicCharacterSO so)
    {
        foreach (Transform t in GetComponentsInHierarchy<Transform>(transform))
        {
            if (t.CompareTag("HealthBarSpawnPoint"))
            {
                GameObject hbc = Instantiate(so.healthBar, t);
                healthBarScale = so.healthBar.transform.localScale;
                SetGlobalScale(hbc.transform, healthBarScale);
                hbc.transform.localPosition = Vector3.zero;
                healthBar = hbc.GetComponentInChildren<HealthBar>();
                healthBar.SetMaxHealth(maxHealth);
                healthBar.SetHealth(health);

                if (characterType == CharacterType.Enemy)
                {
                    healthBar.GetComponentInChildren<TMP_Text>().text = "Lvl: " + (int)Mathf.Clamp(level - 4, 1, Mathf.Infinity);
                }

                break;
            }
        }
    }

    //This damage function is the algorithim used by Pokemon
    private int CalculateDamage(int attackerLevel, int power, int attackerAttack)
    {
        float damage;
        damage = 2.0f * attackerLevel / 5.0f + 2.0f;
        damage *= (float)power;
        damage *= (float)attackerAttack / (float)defense;
        damage /= 50.0f;
        damage += 2.0f;

        if (characterType == CharacterType.Enemy)
        {
            Player.instance.ReceiveDamgeDealt((int)damage);
        }

        return (int)damage;
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collisionAttackTarget = collision;
        }
    }

    public virtual void OnAttacked(BasicAttack attack)
    {
        if (attack.attacker == characterType)
        {
            return;
        }

        AudioManager.instance.PlayOneShot("Hurt");

        SetHealth(health - CalculateDamage(attack.level, attack.power, attack.attack));

        if (!dead)
        {
            anim.Play("Base Layer.Hurt");
        }
    }

    public virtual void OnAttacked(int attackerLevel, int power, int attackerAttack, CharacterType attacker)
    {
        if (attacker == characterType)
        {
            return;
        }

        AudioManager.instance.PlayOneShot("Hurt");

        SetHealth(health - CalculateDamage(attackerLevel, power, attackerAttack));

        if (!dead)
        {
            anim.Play("Base Layer.Hurt");
        }
    }

    protected virtual void Update()
    {
        SetGlobalScale(healthBar.transform, healthBarScale);
    }

    private void LateUpdate()
    {
        SetGlobalScale(healthBar.transform, healthBarScale);
    }

    protected virtual void FixedUpdate()
    {
        if (health <= 0)
        {
            Die();
        }
    }
}
