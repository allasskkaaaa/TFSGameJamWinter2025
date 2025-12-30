using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;


[RequireComponent(typeof(NavMeshAgent))] 
public class Enemy : Health
{
    public event Action<EnemyStates> OnStateChanged;

    [SerializeField] private EnemyStates currentState;
    public EnemyTypes enemyType;

    [Header("Detection")]
    [SerializeField] private float detectionRange = 15f;
    [SerializeField] private float attackRange = 2.5f;
    [SerializeField] private float shootRange = 10f;

    [Header("Combat")]
    [SerializeField] private int meleeDamage = 20;
    [SerializeField] private int rayDamage = 20;
    [SerializeField] private int ProjectileDamage = 20;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;

    [Header("Shooter Laser")]
    [SerializeField] private LineRenderer laser;
    [SerializeField] private float laserDuration = 0.05f;

    [Header("Audio")]
    public AudioClip[] noiseIdk;

    private NavMeshAgent agent;
    private Transform player;
    private float lastAttackTime;

    public EnemyStates CurrentState
    {
        get => currentState;
        set
        {
            if (currentState == value) return;

            currentState = value;
            OnStateChanged?.Invoke(currentState);
            Debug.Log($"{name} → {currentState}");

            switch (currentState)
            {
                case EnemyStates.Idle:
                    Idle();
                    break;
                case EnemyStates.Patrol:
                    Patrol();
                    break;
                case EnemyStates.Chase:
                    Chase();
                    break;
                case EnemyStates.Attack:
                    Attack();
                    break;
                case EnemyStates.Staggered:
                    Staggered();
                    break;
                case EnemyStates.Dead:
                    Death();
                    break;
            }
        }
    }

    public override void Start()
    {
        base.Start();

        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        CurrentState = EnemyStates.Idle;
    }

    private void Update()
    {
        if (currentState == EnemyStates.Dead || player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        switch (enemyType)
        {
            case EnemyTypes.Melee:
                HandleMelee(distance);
                break;

            case EnemyTypes.Shooter:
                HandleShooter(distance);
                break;

            case EnemyTypes.Projectile:
                HandleProjectile(distance);
                break;
        }
    }

    
    // ALL THE LOGIC WITH THE STATES

    private void Idle()
    {
        agent.isStopped = true;
    }

    private void Patrol()
    {
        // I'll put the patrol stuff later
    }

    private void Chase()
    {
        agent.isStopped = false;
        agent.SetDestination(player.position);
    }

    private void Attack()
    {
        agent.isStopped = true;
    }

    private void Staggered()
    {
        agent.isStopped = true;
    }

    // ENEMY TYPES HANDLING

    private void HandleMelee(float distance)
    {
        if (distance <= attackRange)
        {
            CurrentState = EnemyStates.Attack;
            MeleeExplode();
        }
        else if (distance <= detectionRange)
        {
            CurrentState = EnemyStates.Chase;
        }
        else
        {
            CurrentState = EnemyStates.Idle;
        }
    }

    private void HandleShooter(float distance)
    {
        if (distance <= shootRange)
        {
            CurrentState = EnemyStates.Attack;
            ShootRay();
        }
        else if (distance <= detectionRange)
        {
            CurrentState = EnemyStates.Chase;
        }
        else
        {
            CurrentState = EnemyStates.Idle;
        }
    }

    private void HandleProjectile(float distance)
    {
        if (distance <= shootRange)
        {
            CurrentState = EnemyStates.Attack;
            ShootProjectile();
        }
        else
        {
            CurrentState = EnemyStates.Idle;
        }
    }
    // THEIR ATTACK METHOSD

    private void MeleeExplode()
    {
        if (Time.time < lastAttackTime + attackCooldown) return;

        lastAttackTime = Time.time;

        GameManager.Instance.Health -= meleeDamage;


        Death(); // Self-destruct
    }

    private void ShootRay()
    {
        if (Time.time < lastAttackTime + attackCooldown) return;

        lastAttackTime = Time.time;

        Vector3 start = firePoint != null ? firePoint.position : transform.position;
        Vector3 direction = (player.position - start).normalized;

        Vector3 end = start + direction * shootRange;

        if (Physics.Raycast(start, direction, out RaycastHit hit, shootRange))
        {
            end = hit.point;

            if (hit.collider.CompareTag("Player"))
            {
                GameManager.Instance.Health -= meleeDamage;
            }
        }

        DrawLaser(start, end);
    }

    private void DrawLaser(Vector3 start, Vector3 end)
    {
        if (laser == null) return;

        laser.SetPosition(0, start);
        laser.SetPosition(1, end);

        StopAllCoroutines();
        StartCoroutine(LaserRoutine());
    }

    private IEnumerator LaserRoutine()
    {
        laser.enabled = true;
        yield return new WaitForSeconds(laserDuration);
        laser.enabled = false;
    }

    private void ShootProjectile()
    {
        if (Time.time < lastAttackTime + attackCooldown) return;

        lastAttackTime = Time.time;

        if (projectilePrefab == null || firePoint == null) return;

        Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
    }

    // DYING FUNCTION 

    public override void Death()
    {
        if (currentState == EnemyStates.Dead) return;

        CurrentState = EnemyStates.Dead;
        agent.isStopped = true;

        base.Death();
    }
}