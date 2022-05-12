using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public static int maxEnemy = 2;
    public static Dictionary<int, Enemy> enemies = new Dictionary<int, Enemy>();
    private static int nextEnemyId = 1;
    public int id;
    public EnemyState state;
    public Player target;
    public CharacterController controller;
    public Transform shootOrigin;
    public float gravity = -9.81f;
    public float patrolSpeed = 2;
    public float chaseSpeed = 8;
    public float healt;
    public float maxHealt = 100;
    public float detectionRange = 40;
    public float shootRange = 15;
    public float patrolDuration = 3;
    public float shootAccuracy = 0.1f;
    public float idleDuration = 1;

    private bool isPatrolRoutineRunning;
    private float yVelocity = 0;
    private void Start()
    {
        id = nextEnemyId;
        nextEnemyId++;
        enemies.Add(id, this);

        ServerSend.SpawnEnemy(this);

        state = EnemyState.patrol;
        gravity *= Time.fixedDeltaTime * Time.fixedDeltaTime;
        patrolSpeed *= Time.fixedDeltaTime;
        chaseSpeed *= Time.fixedDeltaTime;
    }

    private void FixedUpdate()
    {
        switch (state)
        {
            case EnemyState.idle:
                LookPlayer();
                break;
            case EnemyState.patrol:
                if (!LookPlayer())
                {
                    Patrol();
                }
                break;
            case EnemyState.chase:
                Chase();
                break;
            case EnemyState.attack:
                Attack();
                break;
            default:

                break;
        }
        if(transform.position.y<-10){
            TakeDamage(100);
        }
    }

    bool LookPlayer()
    {
        foreach (Client _client in Server.clients.Values)
        {
            if (_client.player != null)
            {
                Vector3 _enemytoPlayer = _client.player.transform.position - transform.position;
                if (_enemytoPlayer.magnitude <= detectionRange)
                {
                    if (Physics.Raycast(shootOrigin.position, _enemytoPlayer, out RaycastHit hit, detectionRange))
                    {
                        if (hit.collider.CompareTag("Player"))
                        {
                            target = hit.collider.GetComponent<Player>();
                            if (isPatrolRoutineRunning)
                            {
                                isPatrolRoutineRunning = false;
                                StopCoroutine(PatrolStart());
                            }
                            state = EnemyState.chase;
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }

    void Patrol()
    {
        if (!isPatrolRoutineRunning)
        {
            StartCoroutine(PatrolStart());
        }
        Move(transform.forward, patrolSpeed);
    }
    void Chase()
    {
        if (canSeeTarget())
        {
            Vector3 _enemyToPlayer = target.transform.position - transform.position;
            if (_enemyToPlayer.magnitude <= shootRange)
            {
                state = EnemyState.attack;
            }
            else
            {
                Move(_enemyToPlayer, chaseSpeed);
            }
        }
        else
        {
            target = null;
            state = EnemyState.patrol;
        }
    }


    void Attack()
    {
        if (canSeeTarget())
        {
            Vector3 _enemyToPlayer = target.transform.position - transform.position;
            transform.forward = new Vector3(_enemyToPlayer.x, 0f, _enemyToPlayer.z);
            if (_enemyToPlayer.magnitude <= shootRange)
            {
                Shoot(_enemyToPlayer);
            }
            else
            {
                Move(_enemyToPlayer, chaseSpeed);
            }
        }
        else
        {
            target = null;
            state = EnemyState.patrol;
        }
    }

    IEnumerator PatrolStart()
    {
        isPatrolRoutineRunning = true;
        Vector2 _randomPatrolDirection = Random.insideUnitCircle.normalized;

        transform.forward = new Vector3(_randomPatrolDirection.x, 0f, _randomPatrolDirection.y);
        yield return new WaitForSeconds(patrolDuration);
        state = EnemyState.idle;
        yield return new WaitForSeconds(idleDuration);
        state = EnemyState.patrol;
        isPatrolRoutineRunning = false;
    }

    void Move(Vector3 direction, float speed)
    {
        direction.y = 0;
        transform.forward = direction;
        Vector3 _move = transform.forward * speed;
        if (controller.isGrounded)
        {
            yVelocity = 0;
        }
        yVelocity += gravity;
        _move.y = yVelocity;
        controller.Move(_move);
        ServerSend.EnemyPosition(this);
    }

    void Shoot(Vector3 _direction)
    {
        if (Physics.Raycast(shootOrigin.position, _direction, out RaycastHit hit, shootRange))
        {
            if (hit.collider.CompareTag("Player"))
            {
                if (Random.value <= shootAccuracy)
                {
                    hit.collider.GetComponent<Player>().TakeDamage(5f);
                }
            }
        }
    }

    public void TakeDamage(float _damage)
    {
        healt -= _damage;
        if (healt <= 0)
        {
            healt = 0;
            enemies.Remove(id);
            Destroy(gameObject);
        }
        ServerSend.EnemyHealt(this);
    }

    bool canSeeTarget()
    {
        if (target == null)
            return false;

        if (Physics.Raycast(shootOrigin.position, target.transform.position - transform.position, out RaycastHit hit, detectionRange))
        {
            if (hit.collider.CompareTag("Player"))
            {
                return true;
            }
        }
        return false;
    }

}

public enum EnemyState
{
    idle,
    patrol,
    chase,
    attack
}
