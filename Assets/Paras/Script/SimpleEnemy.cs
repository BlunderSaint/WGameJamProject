using UnityEngine;

public class SimpleEnemy : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float range = 8f;
    public LayerMask mask;

    public Transform wallCheck;
    public Transform ledgeCheck;

    [Header("Attack Settings")]
    public int damageAmount = 10;
    public float attackCooldown = 1.0f;

    private Rigidbody2D rb;
    private int direction = 1;
    private GameObject[] players;

    private Vector2 lastKnownPos;
    private float currentSearchTime;
    private bool isSearching = false;
    private bool isChasing = false;
    private float _nextAttackTime;

    [Header("Patrol Points")]
    public Transform pointA;
    public Transform pointB;
    private Transform currentPatrolTarget;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        players = GameObject.FindGameObjectsWithTag("Player");
        currentPatrolTarget = pointA;  
    }

    void Update()
    {
        GameObject visiblePlayer = GetVisiblePlayer();

        if (visiblePlayer != null)
        {
            // STATE: CHASE
            isChasing = true;
            isSearching = false;
            lastKnownPos = visiblePlayer.transform.position;
            MoveToward(lastKnownPos.x);

            // FIXED: Attack Logic with Cooldown
            if (Vector2.Distance(transform.position, lastKnownPos) < 1.2f)
            {
                if (Time.time >= _nextAttackTime)
                {
                    //visiblePlayer.GetComponent<Health>()?.TakeDamage(damageAmount);
                    //_nextAttackTime = Time.time + attackCooldown;
                }
            }
        }
        else if (isChasing)
        {
            // STATE: JUST LOST PLAYER -> START SEARCHING
            isChasing = false;
            isSearching = true;
            currentSearchTime = 5f;
        }

        if (isSearching)
        {
            Search();
        }
        else if (!isChasing)
        {
            Patrol();
        }
    }

    void MoveToward(float targetX)
    {
        direction = (targetX > transform.position.x) ? 1 : -1;
        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
        transform.localScale = new Vector3(direction, 1, 1);
    }

    void Search()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        currentSearchTime -= Time.deltaTime;

        if (currentSearchTime <= 0)
        {
            isSearching = false;
        }
    }

    void Patrol()
    {
        // 1. Use Mathf.Abs to only check horizontal distance (ignores if points are too high/low)
        if (Mathf.Abs(transform.position.x - currentPatrolTarget.position.x) < 0.5f)
        {
            // Swap the target
            currentPatrolTarget = (currentPatrolTarget == pointA) ? pointB : pointA;
        }

        // 2. Always move toward the CURRENT target
        direction = (currentPatrolTarget.position.x > transform.position.x) ? 1 : -1;

        // 3. Apply velocity and flip the sprite
        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
        transform.localScale = new Vector3(direction, 1, 1);
    }

    GameObject GetVisiblePlayer()
    {
        foreach (GameObject p in players)
        {
            if (CanSeeTarget(p.transform)) return p;
        }
        return null;
    }

    bool CanSeeTarget(Transform target)
    {
        float distance = Vector2.Distance(transform.position, target.position);
        if (distance < range)
        {
            RaycastHit2D hit = Physics2D.Linecast(transform.position, target.position, mask);
            return hit.collider == null;
        }
        return false;
    }


    private void OnDrawGizmosSelected()
    {
        // 1. Draw the detection range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);

        // 2. Draw the patrol points and a path line
        if (pointA != null && pointB != null)
        {
            Gizmos.color = Color.green;

            // Draw little spheres at the exact point locations
            Gizmos.DrawSphere(pointA.position, 0.2f);
            Gizmos.DrawSphere(pointB.position, 0.2f);

            // Draw a line connecting them so you can see the patrol path
            Gizmos.DrawLine(pointA.position, pointB.position);
        }
    }
}