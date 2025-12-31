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
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private Transform firePoint;

    [Header("Melee Specific")]
    [SerializeField] private float knockbackForce = 5f; 
    [SerializeField] private float knockbackRadius = 2.5f;
    [SerializeField] private int meleeDamage = 20;

    [Header("Projectile Specific")]
    [SerializeField] private int ProjectileDamage = 20;
    [SerializeField] private GameObject projectilePrefab;

    [Header("Shooter Laser")]
    [SerializeField] private LineRenderer laser;
    [SerializeField] private int rayDamage = 20;
    [SerializeField] private float laserFollowSpeed = 6f;
    [SerializeField] private float laserActiveTime = 0.6f;
    private bool laserActive;
    private float laserEndTime;
    private Vector3 laserEnd;


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
                    agent.isStopped = true;
                    agent.velocity = Vector3.zero; 
                    Attack();
                    break;

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

        if (currentState == EnemyStates.Chase)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
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

        if (enemyType == EnemyTypes.Melee)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            MeleeExplode(); 
        }
    }


    private void Staggered()
    {
        agent.isStopped = true;
    }

    // ENEMY TYPES HANDLING

    private void HandleMelee(float distance)
    {
        if (currentState == EnemyStates.Dead) return;

        if (distance <= attackRange)
        {
            CurrentState = EnemyStates.Attack;

            if (Time.time >= lastAttackTime + attackCooldown)
            {
                MeleeExplode();
            }
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
            CurrentState = EnemyStates.Chase;
        }
    }
    // THEIR ATTACK METHODS

    private void MeleeExplode()
    {
        if (Time.time < lastAttackTime + attackCooldown) return;

        lastAttackTime = Time.time;

        // making the self destruct effect
        Vector2 explosionPos = transform.position;

        // checking if player is within knockback radius
        Collider2D[] hits = Physics2D.OverlapCircleAll(explosionPos, knockbackRadius, playerMask);

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                Debug.Log("Hit Player with Melee Enemy Explosion");
                // do damage to player
                GameManager.Instance.Health -= meleeDamage;

                // applying knockback effect
                Rigidbody2D playerRb = hit.GetComponent<Rigidbody2D>();
                if (playerRb != null)
                {
                    Vector2 knockDir = (hit.transform.position - transform.position).normalized;
                    float knockDuration = 0.2f;

                    StartCoroutine(KnockbackPlayer(hit.GetComponent<Transform>(), knockDir, knockDuration));

                }
            }
        }
        // </3 SFX 

        Death();
    }

    private IEnumerator KnockbackPlayer(Transform target, Vector2 direction, float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            target.position += (Vector3)(direction * knockbackForce * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
        }
    }


    private void OnDrawGizmosSelected()
    {
        if (enemyType == EnemyTypes.Melee)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, knockbackRadius);
        }
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

        if (projectilePrefab == null || firePoint == null || player == null) return;

        Vector3 targetPos = player.position;
        GameObject bomb = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        Bomb bombScript = bomb.GetComponent<Bomb>();
        if (bombScript != null)
        {
            bombScript.LaunchTowards(targetPos);
        }
    }



    // DYING FUNCTION 

    public override void Death()
    {
        if (currentState == EnemyStates.Dead) return;

        CurrentState = EnemyStates.Dead;
        agent.isStopped = true;

        Destroy(gameObject, 0.5f);
        //base.Death();
    }
}