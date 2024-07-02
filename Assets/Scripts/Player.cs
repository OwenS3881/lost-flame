using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MyFunctions;
using Cinemachine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Player : BasicCharacter
{

    private bool mobileReplace = true;
    public static Player instance { get; private set; }

    private Joystick movementJoystick;
    [SerializeField] public PlayerSO playerSO;
    [Range(0,1)]
    public float joystickActivationRange;
    private GameObject[] attackUIObjects;
    private bool projectileAttackActive;
    private Vector2 lastProjectileDirection;
    public Transform firePoint;
    private float experienceMultiplier = 1;
    private float passiveHealPercent;

    private GameObject[] projectileAimDots;
    private bool projectileDotsActive;

    //WebGL Only
    private Vector2 mouseDirection;
    private bool webGLMovementActive = true;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogError("More than one player in the scene");
        }

        AstarPath.active.Scan(AstarPath.active.data.gridGraph);

        if (!PlayerPrefs.HasKey("MovementKey"))
        {
            PlayerPrefs.SetInt("MovementKey", (int)KeyCode.W);
        }

        if (!PlayerPrefs.HasKey("AttackKey"))
        {
            PlayerPrefs.SetInt("AttackKey", (int)KeyCode.Space);
        }
    }

    // Start is called before the first frame update
    public override void Start()
    {
        graphicsObject = playerSO.graphicsObject;
        basicCharacterSO = (BasicCharacterSO)playerSO;
        base.Start();
        FindObjectOfType<CinemachineTargetGroup>().AddMember(transform, 1, 3);

        GameObject mj = GameObject.FindGameObjectWithTag("MovementJoystick");
        if (mj != null) movementJoystick = mj.GetComponent<Joystick>();
        
        UpgradeExperienceMultiplier();
        UpgardePassiveHealPercent();

        attackUIObjects = new GameObject[0];

        if (mobileReplace)
        {
            GameObject abilityCanvas = GameObject.FindGameObjectWithTag("AbilityCanvas");

            if (abilityCanvas != null)
            {
                attackUIObjects = playerSO.attackSO.CreateUI(abilityCanvas);
            }

            SetUpUI();
        }

        SetupProjectileAim();

        GameObject crown = null;
        foreach (Transform t in GetComponentsInChildren<Transform>(true))
        {
            if (t.CompareTag("PlayerCrown"))
            {
                crown = t.gameObject;
                break;
            }
        }
        crown.SetActive(SavedDataManager.instance.PrestigeLevel > 0);
    }

    private void UpgradeExperienceMultiplier()
    {
        experienceMultiplier = 1;

        for (int i = 0; i < playerSO.experienceMultiplierUpgrades.Length; i++)
        {
            if (SavedDataManager.instance.IsSkillPurchased("experienceBoost" + (i + 1)))
            {
                experienceMultiplier += playerSO.experienceMultiplierUpgrades[i];
            }
            else
            {
                return;
            }
        }
    }

    private void UpgardePassiveHealPercent()
    {
        if (SavedDataManager.instance.IsSkillPurchased("passiveHealUnlock"))
        {
            passiveHealPercent = playerSO.defaultPassiveHealPercent;
        }

        for (int i = 0; i < playerSO.passiveHealUpgrades.Length; i++)
        {
            if (SavedDataManager.instance.IsSkillPurchased("passiveHealUpgrade" + (i + 1)))
            {
                passiveHealPercent += playerSO.passiveHealUpgrades[i];
            }
            else
            {
                return;
            }
        }
    }

    private void SetUpUI()
    {
        if (attackUIObjects.Length < 1) return;

        if (attackObjectType == AttackType.Dash || attackObjectType == AttackType.Melee || attackObjectType == AttackType.Shockwave)
        {
            attackUIObjects[0].GetComponentInChildren<Button>().onClick.AddListener(delegate { Attack(); });
        }
    }

    public override void Attack()
    {
        if (attackObjectType == AttackType.Projectile)
        {
            GameObject a = GetComponentInChildren<ProjectileAttack>().Shoot(lastProjectileDirection.normalized, firePoint.position);
            if (a != null)
            {
                StartCoroutine(ProjectileIgnore(a, 0.01f));
                CinemachineShake.instance.ShakeCamera(2f, 0.1f, false);
                rb.AddForce(-lastProjectileDirection.normalized * GetComponentInChildren<ProjectileAttack>().knockback * rb.mass, ForceMode2D.Impulse);
            }
        }
        else if (attackObjectType == AttackType.Dash)
        {
            bool success = mobileReplace ? GetComponentInChildren<DashAttack>().Dash(rb, movementJoystick.Direction.normalized) : GetComponentInChildren<DashAttack>().Dash(rb, mouseDirection);
            if (success)
            {
                DashAttackSO daSO = (DashAttackSO)playerSO.attackSO;
                CinemachineShake.instance.ShakeCamera(1.5f, daSO.dashTime, false);
            }
        }
        else if (attackObjectType == AttackType.Collision)
        {
            if (collisionAttackTarget == null) return;

            GetComponentInChildren<CollisionAttack>().OnCollide(collisionAttackTarget);
        }
        else if (attackObjectType == AttackType.Melee)
        {
            GetComponentInChildren<MeleeAttack>().Slash();
        }
        else if (attackObjectType == AttackType.Shockwave)
        {
            GetComponentInChildren<ShockwaveAttack>().CreateShockwave(firePoint.position);
        }
        else if (attackObjectType == AttackType.Boomerang)
        {
            GameObject a = GetComponentInChildren<BoomerangAttack>().Throw(lastProjectileDirection.normalized, firePoint);
            if (a != null)
            {
                StartCoroutine(ProjectileIgnore(a, 0.01f));
                CinemachineShake.instance.ShakeCamera(2f, 0.1f, false);
                rb.AddForce(-lastProjectileDirection.normalized * GetComponentInChildren<BoomerangAttack>().knockback * rb.mass, ForceMode2D.Impulse);
            }
        }
    }

    IEnumerator ProjectileIgnore(GameObject a, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (a == null)
        {
            yield break;
        }
        Collider2D[] attackColliders = a.GetComponentsInChildren<Collider2D>();
        if (attackColliders.Length == 0)
        {
            StartCoroutine(ProjectileIgnore(a, 0.01f));
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
    }

    public override void Die()
    {
        if (dead) return;

        dead = true;

        Collider2D[] myColliders = GetComponentsInHierarchy<Collider2D>(transform);
        foreach (Collider2D myCollider in myColliders)
        {
            myCollider.enabled = false;
        }

        CinemachineShake.instance.ShakeCamera(10f, 100f, false);

        DisableInteractiveUI();

        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        anim.Play("Player-death");
        AudioManager.instance.Play("PlayerDeath");
        Invoke("InvokeDeadAnimation", 0.01f);
        if (playerSO.deathEffect != null)
        {
            Instantiate(playerSO.deathEffect, transform.position, Quaternion.identity);
        }      
        Invoke("LoadDeath", 1f);
    }

    private void InvokeDeadAnimation()
    {
        if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Player-death"))
        {
            Invoke("InvokeDeadAnimation", 0.01f);
            return;
        }

        Invoke("DeadAnimation", anim.GetCurrentAnimatorStateInfo(0).length);
    }

    private void DeadAnimation()
    {
        anim.Play("Player-dead");
        anim.enabled = false;
    }

    private void LoadDeath()
    {
        LevelLoader.instance.LoadScene("Death", "Death");
    }

    public void DisableInteractiveUI()
    {
        if (movementJoystick != null && mobileReplace)
        {
            movementJoystick.gameObject.SetActive(false);
        }
        else
        {
            webGLMovementActive = false;
        }

        for (int i = 0; i < attackUIObjects.Length; i++)
        {
            attackUIObjects[i].SetActive(false);
        }
        for (int i = 0; i < BasicAbility.instances.Count; i++)
        {
            BasicAbility.instances[i].Disable();
        }
        
        if (DungeonMap.instance != null) DungeonMap.instance.gameObject.SetActive(false);
    }

    public void EnableInteractiveUI()
    {
        if (movementJoystick != null && mobileReplace)
        {
            movementJoystick.gameObject.SetActive(true);
        }
        else
        {
            webGLMovementActive = true;
        }

        for (int i = 0; i < attackUIObjects.Length; i++)
        {
            attackUIObjects[i].SetActive(true);
        }
        for (int i = 0; i < BasicAbility.instances.Count; i++)
        {
            BasicAbility.instances[i].Enable();
        }

        if (DungeonMap.instance != null) DungeonMap.instance.gameObject.SetActive(true);
    }

    private void ProjectileAttackCode()
    {
        if (attackUIObjects.Length < 1 && mobileReplace) return;

        if (!projectileAttackActive)
        {
            if (projectileDotsActive) ProjectileDotsSetActive(false);

            if (!mobileReplace)
            {
                if (Input.GetKey((KeyCode)PlayerPrefs.GetInt("AttackKey")))
                {
                    projectileAttackActive = true;
                    lastProjectileDirection = mouseDirection;
                }
                return;
            }

            if (attackUIObjects[0].GetComponent<Joystick>().Direction.magnitude >= joystickActivationRange)
            {
                projectileAttackActive = true;
                lastProjectileDirection = attackUIObjects[0].GetComponent<Joystick>().Direction.normalized;
                return;
            }

            return;
        }

        ProjectileAim();

        if (!mobileReplace)
        {
            if (Input.GetKey((KeyCode)PlayerPrefs.GetInt("AttackKey")))
            {
                lastProjectileDirection = mouseDirection;
            }
            else
            {
                projectileAttackActive = false;
            }
            return;
        }

        if (attackUIObjects[0].GetComponent<Joystick>().Direction.magnitude < joystickActivationRange && attackUIObjects[0].GetComponent<Joystick>().Direction.magnitude > joystickActivationRange/2)
        {
            projectileAttackActive = false;
            return;
        }
        else if (attackUIObjects[0].GetComponent<Joystick>().Direction.magnitude < joystickActivationRange)
        {
            Attack();
            projectileAttackActive = false;
        }
        else
        {
            lastProjectileDirection = attackUIObjects[0].GetComponent<Joystick>().Direction.normalized;
        }
    }

    private void SetupProjectileAim()
    {
        if (attackUIObjects.Length < 1 && mobileReplace) return;

        if (projectileAimDots != null || !SavedDataManager.instance.IsSkillPurchased("projectileAimUnlock")) return;

        projectileAimDots = new GameObject[playerSO.numberOfDots];
        for (int i = 0; i < projectileAimDots.Length; i++)
        {
            projectileAimDots[i] = Instantiate(playerSO.projectileAimDot, transform);
        }
        ProjectileDotsSetActive(false);
    }

    private void ProjectileAim()
    {
        if (attackUIObjects.Length < 1 && mobileReplace) return;

        if (!SavedDataManager.instance.IsSkillPurchased("projectileAimUnlock")) return;

        if (!projectileDotsActive) ProjectileDotsSetActive(true);

        Vector2 dir = mobileReplace ? attackUIObjects[0].GetComponent<Joystick>().Direction.normalized : mouseDirection;

        for (int i = 0; i < projectileAimDots.Length; i++)
        {
            projectileAimDots[i].transform.position = (Vector2)firePoint.position + (dir * playerSO.distanceBetweenDots * (i + 1));
        }
    }

    private void ProjectileDotsSetActive(bool active)
    {
        if (attackUIObjects.Length < 1 && mobileReplace) return;

        if (!SavedDataManager.instance.IsSkillPurchased("projectileAimUnlock")) return;

        for (int i = 0; i < projectileAimDots.Length; i++)
        {
            projectileAimDots[i].SetActive(active);
        }
        projectileDotsActive = active;
    }

    private void MovementCode()
    {
        if (movementJoystick.gameObject.activeSelf && movementJoystick.Direction.magnitude >= joystickActivationRange)
        {
            if (rb.velocity.Equals(Vector2.zero))
            {
                rb.velocity = movementJoystick.Direction.normalized * speed;
            }
            else if (rb.velocity.magnitude < speed)
            {
                rb.AddForce(movementJoystick.Direction.normalized * speed * rb.mass, ForceMode2D.Impulse);
            }
            anim.SetBool("isMoving", true);
            anim.speed = speed;
            if (rb.velocity.x > 0)
            {
                SetScale(transform, 0, Mathf.Abs(transform.localScale.x));
            }
            else if (rb.velocity.x < 0)
            {
                SetScale(transform, 0, -Mathf.Abs(transform.localScale.x));
            }
        }
        else if (anim.GetBool("isMoving"))
        {
            rb.velocity = Vector2.zero;
            anim.SetBool("isMoving", false);
            anim.speed = 1;
        }
    }

    private void WebGLMovementCode()
    {
        if (Input.GetKey((KeyCode)PlayerPrefs.GetInt("MovementKey")))
        {
            if (rb.velocity.Equals(Vector2.zero))
            {
                rb.velocity = mouseDirection * speed;
            }
            else if (rb.velocity.magnitude < speed)
            {
                rb.AddForce(mouseDirection * speed * rb.mass, ForceMode2D.Impulse);
            }
            anim.SetBool("isMoving", true);
            anim.speed = speed;
            if (rb.velocity.x > 0)
            {
                SetScale(transform, 0, Mathf.Abs(transform.localScale.x));
            }
            else if (rb.velocity.x < 0)
            {
                SetScale(transform, 0, -Mathf.Abs(transform.localScale.x));
            }

            return;
        }
        else if (anim.GetBool("isMoving"))
        {
            rb.velocity = Vector2.zero;
            anim.SetBool("isMoving", false);
            anim.speed = 1;
        }

        if (mouseDirection.x > 0)
        {
            SetScale(transform, 0, Mathf.Abs(transform.localScale.x));
        }
        else if (mouseDirection.x < 0)
        {
            SetScale(transform, 0, -Mathf.Abs(transform.localScale.x));
        }
    }

    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        base.OnCollisionEnter2D(collision);
        if (attackObjectType == AttackType.Collision && GetComponentInHierarchy<BasicEnemy>(collision.transform) != null)
        {
            Attack();
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (attackObjectType == AttackType.Dash && GetComponentInChildren<DashAttack>().isDashing)
        {

            if (SavedDataManager.instance.IsSkillPurchased("multiDash") && GetComponentInHierarchy<IAttackable>(collision.transform) != null)
            {
                DashAttack da = GetComponentInChildren<DashAttack>();
                StartCoroutine(IgnoreCollisionFor(da.dashTime - da.dashTimeCounter, collision.transform));
                GetComponentInHierarchy<IAttackable>(collision.transform).OnAttacked(da);
                return;
            }

            GetComponentInChildren<DashAttack>().DashCollision(collision);
        }
    }

    IEnumerator IgnoreCollisionFor(float t, Transform collision)
    {
        if (t <= 0) yield break;

        Collider2D[] collisionColliders = GetComponentsInHierarchy<Collider2D>(collision);
        Collider2D[] myColliders = GetComponentsInHierarchy<Collider2D>(transform);

        foreach (Collider2D myCollider in myColliders)
        {
            foreach (Collider2D c in collisionColliders)
            {
                Physics2D.IgnoreCollision(c, myCollider);
            }
        }

        yield return new WaitForSeconds(t);

        if (collisionColliders[0] == null) yield break;

        foreach (Collider2D myCollider in myColliders)
        {
            foreach (Collider2D c in collisionColliders)
            {
                Physics2D.IgnoreCollision(c, myCollider, false);
            }
        }
    }

    public override void OnAttacked(BasicAttack attack)
    {
        base.OnAttacked(attack);
        CinemachineShake.instance.ShakeCamera(3f, 0.2f, false);
        for (int i = 0; i < BasicAbility.instances.Count; i++)
        {
            BasicAbility.instances[i].OnPlayerAttacked();
        }
    }

    public override void OnAttacked(int attackerLevel, int power, int attackerAttack, CharacterType attacker)
    {
        base.OnAttacked(attackerLevel, power, attackerAttack, attacker);
        CinemachineShake.instance.ShakeCamera(3f, 0.2f, false);
        for (int i = 0; i < BasicAbility.instances.Count; i++)
        {
            BasicAbility.instances[i].OnPlayerAttacked();
        }
    }

    public void GainExperience(int amount)
    {
        int oldLevel = level;

        SavedDataManager.instance.PlayerExperience += Mathf.RoundToInt(amount * experienceMultiplier);
        level = ExperienceToLevel(SavedDataManager.instance.PlayerExperience);

        PlayerExperienceBar.instance.UpdateExperience(SavedDataManager.instance.PlayerExperience);

        if (oldLevel != level)
        {
            DetermineStats();
            BasicAttack a = GetComponentInChildren<BasicAttack>();
            a.attack = attack;
            a.level = level;
        }
    }

    public void ReceiveDamgeDealt(int damage)
    {
        for (int i = 0; i < BasicAbility.instances.Count; i++)
        {
            BasicAbility.instances[i].ReceiveDamgeDealt(damage);
        }

        float passive = (passiveHealPercent / 100) * maxHealth;

        if (passive >= 1)
        {
            AddHealth((int)passive);
        }
        else if (UnityEngine.Random.Range(0.0f, 1.0f) < passive)
        {
            AddHealth(1);
        }
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (attackObjectType == AttackType.Projectile || attackObjectType == AttackType.Boomerang)
        {
            ProjectileAttackCode();
        }

        if (attackObjectType != AttackType.Dash || !GetComponentInChildren<DashAttack>().isDashing)
        {
            if (mobileReplace)
            {
                MovementCode();
            }
            else if (webGLMovementActive)
            {
                WebGLMovementCode();
            }
            else
            {
                anim.SetBool("isMoving", false);
            }
        }
        else
        {
            anim.SetBool("isMoving", false);
        }
    }

    protected override void Update()
    {
        base.Update();

        if (!mobileReplace)
        {
            mouseDirection = ((Vector2)Input.mousePosition - new Vector2(Screen.width / 2, Screen.height / 2)).normalized;
            movementJoystick.gameObject.SetActive(false);

            if (Input.GetKeyUp((KeyCode)PlayerPrefs.GetInt("AttackKey")) && webGLMovementActive)
            {
                Attack();
            }
        }

        if (Input.GetKeyDown(KeyCode.Space)) Attack();
    }
}