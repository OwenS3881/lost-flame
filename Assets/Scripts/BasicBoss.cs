using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MyFunctions;
using Pathfinding;

public abstract class BasicBoss : MonoBehaviour, ICanAttack, IAttackable
{
    [SerializeField] protected BossAttackData[] attacks;

    protected BossAttackData currentAttack;

    protected bool collisionAttackActive;

    protected Rigidbody2D rb;
    protected Animator anim;

    protected Vector2 dashDirection;

    public CharacterType characterType { get; set; }

    protected bool dead;

    protected Vector2 directionToPlayer;
    protected Vector2 shootDirection;
    protected float distanceToPlayer;

    protected Player player;
    protected Transform playerCenter;

    [HideInInspector] public Transform firePoint;
    protected List<Transform> multiFirePoints = new List<Transform>();

    [ReadOnly] public EnemyState state = EnemyState.Moving;

    [SerializeField] protected bool isInvincible;

    [SerializeField] protected bool introHappening;

    [Header("Stats")]
    public int level;
    [SerializeField] protected int speed;

    [SerializeField] protected int baseAttack;
    [SerializeField] protected int baseDefense;
    [SerializeField] protected int baseHealth;

    [SerializeField, ReadOnly] protected float baseStatAverage;

    [Range(0, 15)]
    [SerializeField] protected int healthIV;
    [Range(0, 15)]
    [SerializeField] protected int attackIV;
    [Range(0, 15)]
    [SerializeField] protected int defenseIV;

    [Tooltip("Amount of time between each of the boss's attacks")]
    [SerializeField] protected float cooldown;

    [ReadOnly] public int attack;
    [SerializeField, ReadOnly] protected int defense;
    [SerializeField, ReadOnly] protected int health = 1;
    [ReadOnly] public int maxHealth;

    protected float currentCooldown;

    [Header("Pathfinding")]
    [SerializeField] protected bool usesPathfinding;
    [SerializeField] protected float nextWaypointDistance;
    [SerializeField] protected float pathUpdateRate;
    [SerializeField] protected float stopMovingDistance;
    [SerializeField] protected float tooFarFromPlayerDistance;
    protected Path path;
    protected int currentWaypoint = 0;
    protected bool reachedEndOfPath = false;
    protected Seeker seeker;

    [Header("Health Bar")]
    [SerializeField] protected GameObject healthBarPrefab;
    [SerializeField] protected string bossName;

    protected HealthBar healthBar;

    [SerializeField] protected Color nameColor = Color.white;
    [SerializeField] protected Color backgroundColor = Color.white;
    [SerializeField] protected Color borderColor = Color.white;

    [Header("Death")]
    [SerializeField] protected GameObject deathEffect;

    [Header("Contact Damage")]
    [SerializeField] protected BossAttackData defaultCollision;

    protected virtual void OnValidate()
    {
        baseStatAverage = (baseHealth + baseAttack + baseDefense) / 3.0f;
    }

    protected virtual void Start()
    {
        state = EnemyState.Moving;
        characterType = CharacterType.Enemy;

        maxHealth = health;

        currentCooldown = cooldown;

        rb = GetComponentInHierarchy<Rigidbody2D>(transform);
        anim = GetComponentInHierarchy<Animator>(transform);

        seeker = GetComponent<Seeker>();
        if (usesPathfinding) InvokeRepeating("UpdatePath", 0f, pathUpdateRate);

        if (Player.instance != null) player = Player.instance;
        if (player != null) playerCenter = player.firePoint;

        foreach (Transform t in GetComponentsInHierarchy<Transform>(transform))
        {
            if (t.CompareTag("FirePoint"))
            {
                firePoint = t;
            }
            else if (t.CompareTag("MultiFirePoint"))
            {
                multiFirePoints.Add(t);
            }
        }

        for (int i = 0; i < attacks.Length; i++)
        {
            CreateAttack(attacks[i]);
        }

        if (defaultCollision.attackSO != null) CreateAttack(defaultCollision);

        DetermineStats();
        DetermineHealthStats();

        CreateHealthBar();

        level *= SavedDataManager.instance.PrestigeLevel + 1;
    }

    protected abstract void StateLogic();

    protected void CreateHealthBar()
    {
        GameObject hbc = Instantiate(healthBarPrefab, healthBarPrefab.transform.position, Quaternion.identity);

        healthBar = hbc.GetComponentInChildren<HealthBar>();
        healthBar.SetMaxHealth(maxHealth);
        healthBar.SetHealth(health);

        hbc.GetComponentInChildren<BossHealthBarSetup>().Setup(bossName, nameColor, backgroundColor, borderColor);
    }

    public void DetermineStats()
    {
        DetermineStats(baseAttack, baseDefense, speed);
    }

    public void DetermineStats(int a, int d)
    {
        DetermineStats(a, d, speed);
    }

    public void DetermineStats(int a, int d, int s)
    {
        speed = s;
        attack = CalculateStat(a, StatType.Attack);
        defense = CalculateStat(d, StatType.Defense);
    }

    protected void DetermineHealthStats()
    {
        DetermineHealthStats(baseHealth);
    }

    protected void DetermineHealthStats(int h)
    {
        health = CalculateStat(h, StatType.Health);
        maxHealth = health;
    }

    protected int CalculateStat(int baseStat, StatType type)
    {
        int iv = DetermineIV(type);
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

    protected int DetermineIV(StatType type)
    {
        if (type == StatType.Attack)
        {
            return attackIV;
        }
        else if (type == StatType.Defense)
        {
            return defenseIV;
        }
        else if (type == StatType.Health)
        {
            return healthIV;
        }
        else
        {
            return -1;
        }
    }

    public int GetAttack()
    {
        return attack;
    }

    public int GetLevel()
    {
        return level;
    }

    protected void StopMoving()
    {
        if (anim.GetBool("isMoving"))
        {
            rb.velocity = Vector2.zero;
            anim.SetBool("isMoving", false);
            anim.speed = 1;
        }
    }

    protected BossAttackData GetAttackFromName(string name)
    {
        foreach (BossAttackData b in attacks)
        {
            if (b.attackName.Equals(name)) return b;
        }
        Debug.LogError("No boss attack was found with name: " + name);
        return null;
    }

    protected string GetRandomAnimation()
    {
        int totalProbability = 0;
        foreach (BossAttackData b in attacks)
        {
            totalProbability += b.probability;
        }

        float random = UnityEngine.Random.Range(0f, (float)totalProbability);

        int runningSum = 0;
        foreach (BossAttackData b in attacks)
        {
            runningSum += b.probability;
            if (runningSum > random)
            {
                return b.attackName;
            }
        }
        return null;
    }

    public abstract void Attack(string name);

    public void PlayAttackAnimation(string name)
    {
        state = EnemyState.Attacking;
        anim.Play(name);
    }

    public virtual void FinishAttack()
    {
        state = EnemyState.Moving;
        currentCooldown = cooldown;
        currentAttack = null;
        collisionAttackActive = false;
    }

    protected void Dash()
    {
        DashAttack da = currentAttack.attackObject as DashAttack;
        da.Dash(rb, dashDirection);
    }

    protected void Shockwave()
    {
        (currentAttack.attackObject as ShockwaveAttack).CreateShockwave(firePoint.position);
    }

    protected IEnumerator Projectile(Vector2 dir, ProjectileAttack pa)
    {
        yield return new WaitForSeconds(currentAttack.attackSO.attackStartDelay);

        GameObject a = pa.Shoot(dir.normalized, firePoint.position);
        if (a != null)
        {
            rb.AddForce(-dir.normalized * pa.knockback * rb.mass, ForceMode2D.Impulse);

            if (dir.x > 0)
            {
                SetScale(transform, 0, Mathf.Abs(transform.localScale.x));
            }
            else if (dir.x < 0)
            {
                SetScale(transform, 0, -Mathf.Abs(transform.localScale.x));
            }

            while (true)
            {
                yield return new WaitForSeconds(0.01f);
                if (a == null)
                {
                    continue;
                }
                Collider2D[] attackColliders = a.GetComponentsInChildren<Collider2D>();
                if (attackColliders.Length == 0)
                {
                    yield return new WaitForSeconds(0.01f);
                    continue;
                }
                Collider2D[] myColliders = GetComponentsInHierarchy<Collider2D>(transform);
                foreach (Collider2D myCollider in myColliders)
                {
                    foreach (Collider2D c in attackColliders)
                    {
                        Physics2D.IgnoreCollision(c, myCollider);
                        c.isTrigger = false;
                    }
                }

                break;
            }
        }
    }

    protected IEnumerator MultiProjectile(MultiProjectileAttack ma, bool targetPlayer)
    {
        Vector2[] dirs = new Vector2[multiFirePoints.Count];

        if (!targetPlayer)
        {
            for (int i = 0; i < multiFirePoints.Count; i++)
            {
                dirs[i] = (multiFirePoints[i].position - firePoint.position).normalized;
            }
        }
        else
        {
            for (int i = 0; i < multiFirePoints.Count; i++)
            {
                dirs[i] = (playerCenter.position - multiFirePoints[i].position).normalized;
            }
        }

        yield return new WaitForSeconds(currentAttack.attackSO.attackStartDelay);

        GameObject[] projectiles = ma.ShootMulti(dirs, multiFirePoints.ToArray());

        if (projectiles == null) yield break;

        Collider2D[] myColliders = GetComponentsInHierarchy<Collider2D>(transform);

        for (int i = 0; i < projectiles.Length; i++)
        {
            if (projectiles[i] == null)
            {
                continue;
            }

            Collider2D[] attackColliders = projectiles[i].GetComponentsInChildren<Collider2D>();
            if (attackColliders.Length == 0)
            {
                yield return new WaitForSeconds(0.01f);
                i--;
                continue;
            }

            foreach (Collider2D myCollider in myColliders)
            {
                foreach (Collider2D c in attackColliders)
                {
                    Physics2D.IgnoreCollision(c, myCollider);
                    c.isTrigger = false;
                }
            }
        }
    }

    protected IEnumerator CollisionAttackCoroutine()
    {
        float duration = anim.GetCurrentAnimatorStateInfo(0).length;

        yield return new WaitForSeconds(currentAttack.attackSO.attackStartDelay);

        collisionAttackActive = true;

        float t = 0;

        while (t < duration)
        {
            t += Time.deltaTime;
            yield return null;
        }

        collisionAttackActive = false;
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && collisionAttackActive && currentAttack != null)
        {
            (currentAttack.attackObject as CollisionAttack).OnCollide(collision);
        }

        if (collision.gameObject.CompareTag("Player") && !collisionAttackActive && currentAttack == null && defaultCollision.attackSO != null)
        {
            (defaultCollision.attackObject as CollisionAttack).OnCollide(collision);
        }
    }

    protected void CreateAttack(BossAttackData bossAttackData)
    {
        BasicAttack ba = bossAttackData.attackSO.attackObject;

        GameObject baObj = Instantiate(ba.gameObject, transform);
        baObj.GetComponent<BasicAttack>().basicAttackSO = bossAttackData.attackSO;

        if (ba is MultiProjectileAttack)
        {
            bossAttackData.type = AttackType.MultiProjectile;
        }
        else if (ba is ProjectileAttack)
        {
            bossAttackData.type = AttackType.Projectile;
        }
        else if (ba is DashAttack)
        {
            bossAttackData.type = AttackType.Dash;
        }
        else if (ba is CollisionAttack)
        {
            bossAttackData.type = AttackType.Collision;
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
            bossAttackData.type = AttackType.Melee;
        }
        else if (ba is ShockwaveAttack)
        {
            bossAttackData.type = AttackType.Shockwave;
        }
        else if (ba is BoomerangAttack)
        {
            bossAttackData.type = AttackType.Boomerang;
            throw new System.NotImplementedException("Ha Ha, I was lazy and didnt put in boomerang attack for boss");
        }
        else
        {
            Debug.LogError("Attack Type was Null on " + ba);
            bossAttackData.type = AttackType.Null;
        }

        bossAttackData.attackObject = baObj.GetComponent<BasicAttack>();
    }

    protected void UpdatePath()
    {
        if (!seeker.IsDone() || state != EnemyState.Moving) return;

        seeker.StartPath(transform.position, playerCenter.transform.position, OnPathComplete);
    }

    protected void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
        else
        {
            Debug.LogWarning(p.errorLog);
        }
    }

    protected void Pathfinding()
    {
        if (path == null)
        {
            return;
        }

        if (currentWaypoint >= path.vectorPath.Count)
        {
            reachedEndOfPath = true;
            return;
        }
        else
        {
            reachedEndOfPath = false;
        }

        anim.SetBool("isMoving", true);
        anim.speed = speed;

        Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;

        if (rb.velocity.Equals(Vector2.zero))
        {
            rb.velocity = direction.normalized * speed;
        }
        else if (rb.velocity.magnitude < speed)
        {
            rb.AddForce(direction.normalized * speed * rb.mass, ForceMode2D.Impulse);
        }

        float distance = Vector2.Distance(transform.position, path.vectorPath[currentWaypoint]);

        if (distance < nextWaypointDistance)
        {
            currentWaypoint++;
        }
    }

    protected virtual void OnCollisionStay2D(Collision2D collision)
    {
        if (currentAttack != null && currentAttack.type == AttackType.Dash)
        {
            (currentAttack.attackObject as DashAttack).DashCollision(collision);
        }
    }

    //This damage function is the algorithim used by Pokemon
    protected int CalculateDamage(int attackerLevel, int power, int attackerAttack)
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

    public void OnAttacked(BasicAttack attack)
    {
        if (isInvincible) return;

        if (attack.attacker == characterType)
        {
            return;
        }

        AudioManager.instance.PlayOneShot("Hurt");

        SetHealth(health - CalculateDamage(attack.level, attack.power, attack.attack));

        if (!dead && state != EnemyState.Attacking)
        {
            anim.Play("Base Layer.Hurt");
        }
    }

    public void OnAttacked(int attackerLevel, int power, int attackerAttack, CharacterType attacker)
    {
        if (isInvincible) return;

        if (attacker == characterType)
        {
            return;
        }

        AudioManager.instance.PlayOneShot("Hurt");

        SetHealth(health - CalculateDamage(attackerLevel, power, attackerAttack));

        if (!dead && state != EnemyState.Attacking)
        {
            anim.Play("Base Layer.Hurt");
        }
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

    protected abstract void Die();

    protected virtual void DestroySelf()
    {
        Destroy(gameObject);
    }

    protected void InstantiateDeathEffect()
    {
        if (deathEffect != null) Instantiate(deathEffect, firePoint.position, Quaternion.identity);
    }

    protected virtual void Update()
    {
        if (state != EnemyState.Attacking && currentCooldown > 0 && !introHappening)
        {
            currentCooldown -= Time.deltaTime;
        }

        if (!dead && health <= 0)
        {
            Die();
        }
    }

    protected virtual void FixedUpdate()
    {
        if (playerCenter != null)
        {
            directionToPlayer = (playerCenter.position - transform.position).normalized;
            shootDirection = (playerCenter.position - firePoint.position).normalized;
            distanceToPlayer = Vector2.Distance(playerCenter.position, transform.position);
        }
        else
        {
            player = Player.instance;
            playerCenter = player.firePoint;
        }

        if (!introHappening)
        {
            StateLogic();
        }
    }
}
