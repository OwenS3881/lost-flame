using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MyFunctions;

public class XeroSword : CollisionAttack, IAttackable
{
    private XeroBoss xero;
    private Transform defaultPoint;

    private int index;

    private Rigidbody2D rb;

    private string currentAttack;
    private bool isAttacking;
    public bool IsAttacking
    {
        get { return isAttacking; }
        set
        {
            isAttacking = value;
            if (xero != null) xero.UpdateIsAttacking();
        }
    }

    public CharacterType characterType { get ; set; }

    [SerializeField] private float returnToDefaultSpeed;

    [Header("Stab Attacks Fields")]    
    [SerializeField] private float stabLength;
    [SerializeField] private Vector2 doubleStabSpeedMultiplier = Vector2.one;
    [SerializeField] private Vector2 quickStabSpeedMultiplier = Vector2.one;
    [SerializeField] private float quickStabStaggerTime;
    private float stabTime;
    private bool isStabReturning;
    private Vector2 stabTarget;

    [Header("Spin Attack")]    
    [SerializeField] private float spinLength;
    [SerializeField] private float spinRotateSpeed;
    private float spinTime;

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
        characterType = CharacterType.Enemy;
    }

    public void Initialize(XeroBoss x, Transform dp, int i)
    {
        xero = x;
        defaultPoint = dp;
        index = i;
    }

    public void SetAttackSO(CollisionAttackSO so)
    {
        basicAttackSO = so;
        Awake();
    }

    public void Attack(string attackName, float delay)
    {
        StartCoroutine(AttackCoroutine(attackName, delay));
    }

    IEnumerator AttackCoroutine(string attackName, float delay)
    {
        if (attackName.Equals("QuickStab"))
        {
            yield return new WaitForSeconds(quickStabStaggerTime * index);
        }

        if (attackName.Equals("DoubleStab") || attackName.Equals("QuickStab"))
        {
            stabTarget = Player.instance.transform.position;
        }

        yield return new WaitForSeconds(delay);

        currentAttack = attackName;
        IsAttacking = true;

        
    }

    public void EndAttack()
    {
        currentAttack = "";
        IsAttacking = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            OnCollide(collision);
        }
    }

    private void DefaultMotion()
    {
        //transform.position = Vector3.MoveTowards(transform.position, defaultPoint.position, returnToDefaultSpeed * Time.deltaTime);
        
        //dir is intentionally not normalized
        Vector2 dir = defaultPoint.position - transform.position;
        rb.AddForce(dir * rb.mass * returnToDefaultSpeed, ForceMode2D.Force);

        transform.up = Vector3.up;
    }

    private void StateLogic()
    {
        if (!IsAttacking)
        {
            DefaultMotion();
        }
        else
        {
            if (currentAttack.Equals("DoubleStab") || currentAttack.Equals("QuickStab"))
            {
                Stab();
            }
            else if (currentAttack.Equals("Spin"))
            {
                Spin();
            }
        }
    }

    private void Spin()
    {
        //dir is intentionally not normalized
        Vector2 dir = defaultPoint.position - transform.position;
        rb.AddForce(dir * rb.mass * returnToDefaultSpeed, ForceMode2D.Force);

        transform.Rotate(new Vector3(0, 0, spinRotateSpeed * Time.deltaTime));

        spinTime += Time.deltaTime;

        if (spinTime > spinLength)
        {
            spinTime = 0f;
            EndAttack();
            xero.FinishAttack();
        }
    }

    private void Stab()
    {
        transform.position = Vector3.Lerp(defaultPoint.position, stabTarget, stabTime);

        transform.up = Vector3.Lerp(Vector3.up, (Vector3)stabTarget - defaultPoint.position, stabTime);

        if (!isStabReturning)
        {
            if (currentAttack.Equals("DoubleStab"))
            { 
                stabTime += Time.deltaTime * doubleStabSpeedMultiplier.x;
            }
            else if (currentAttack.Equals("QuickStab"))
            {
                stabTime += Time.deltaTime * quickStabSpeedMultiplier.x;
            }
        }
        else
        {
            if (currentAttack.Equals("DoubleStab"))
            {
                stabTime -= Time.deltaTime * doubleStabSpeedMultiplier.x;
            }
            else if (currentAttack.Equals("QuickStab"))
            {
                stabTime -= Time.deltaTime * quickStabSpeedMultiplier.x;
            }
        }

        if ((!isStabReturning && stabTime > stabLength) || cooldownTimer > 0)
        {
            isStabReturning = true;
        }
        else if (isStabReturning && stabTime < 0)
        {
            isStabReturning = false;
            stabTime = 0f;
            EndAttack();
            xero.FinishAttack();
        }
    }

    protected override void Update()
    {
        StateLogic();
        base.Update();
    }

    public void OnAttacked(BasicAttack attack)
    {
        Debug.Log("Hit Sword");
        CinemachineShake.instance.StopShaking();
    }

    public void OnAttacked(int attackerLevel, int power, int attackerAttack, CharacterType attacker)
    {
        Debug.Log("Hit Sword");
        CinemachineShake.instance.StopShaking();
    }
}
