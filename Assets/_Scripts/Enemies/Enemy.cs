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
    [SerializeField] private LayerMask playerMask;
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


    private bool laserActive;
    private float laserEndTime;
    private Vector3 laserEnd;

    [Header("Shooter Laser")]
    [SerializeField] private LineRenderer laser;
    [SerializeField] private float laserFollowSpeed = 6f;
    [SerializeField] private float laserActiveTime = 0.6f;

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
        player = GameObject.FindGameObjectWithTag("Player").transform;
        agent.updateUpAxis = false;
        agent.updateRotation = false;
        if (laser != null)
            laser.enabled = false;

        CurrentState = EnemyStates.Idle;
    }

    private void Update()
    {
        if (currentState == EnemyStates.Dead || player == null) return;

        float distance = Vector3.Distance(transform.position, new Vector3(player.position.x,player.position.y,0f));

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
        agent.SetDestination(new Vector3(player.position.x,player.position.y, 0f)); 
    }

    private void Attack()
    {
        agent.isStopped = true;
        laserEnd = Vector3.zero;
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
        else
        {
            if (laser != null) laser.enabled = false;
            laserActive = false;
            CurrentState = EnemyStates.Chase;
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
    // THEIR ATTACK METHODS

    private void MeleeExplode()
    {
        if (Time.time < lastAttackTime + attackCooldown) return;

        lastAttackTime = Time.time;

        GameManager.Instance.Health -= meleeDamage;


        Death(); // Self-destruct
    }

    private void ShootRay()
    {
        if (laser == null) return;

        Vector3 start = firePoint != null ? firePoint.position : transform.position;

        // Start laser ONLY if cooldown finished
        if (!laserActive && Time.time >= lastAttackTime + attackCooldown)
        {
            laserActive = true;
            laserEndTime = Time.time + laserActiveTime;
            laserEnd = start;
        }

        if (!laserActive)
        {
            laser.enabled = false;
            return;
        }

        // End laser → start cooldown
        if (Time.time >= laserEndTime)
        {
            laserActive = false;
            laser.enabled = false;
            lastAttackTime = Time.time; 
            return;
        }

        // Laser follows player with delay :00 
        Vector3 desiredDir = (player.position - start).normalized;
        Vector3 desiredEnd = start + desiredDir * shootRange;

        laserEnd = Vector3.Lerp(
            laserEnd,
            desiredEnd,
            Time.deltaTime * laserFollowSpeed
        );

        Vector2 rayDir = (laserEnd - start).normalized;

        RaycastHit2D hitInfo = Physics2D.Raycast(start, rayDir, shootRange, playerMask);

        // If it id hit something, adjust the end point of the laser
        Vector3 finalEnd =  start + (Vector3)rayDir * shootRange;

        DrawLaser(start, finalEnd);

        // Damaging the player over time (without time.delta Time its basically instant death lmao)
        if (hitInfo.collider != null && hitInfo.collider.CompareTag("Player"))
        {
            GameManager.Instance.Health -= rayDamage * Time.deltaTime;
        }
    }




    private void DrawLaser(Vector3 start, Vector3 end)
    {
        if (laser == null) return;

        laser.enabled = true;         
        laser.positionCount = 2;       
        laser.useWorldSpace = true;

        laser.SetPosition(0, new Vector3(start.x, start.y, 0f));
        laser.SetPosition(1, new Vector3(end.x, end.y, 0f));
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