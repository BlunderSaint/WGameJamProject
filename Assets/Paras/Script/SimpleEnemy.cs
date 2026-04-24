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

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        players = GameObject.FindGameObjectsWithTag("Player");
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
        // 1. Check if we reached the current target (within 0.5 units)
        if (Vector2.Distance(transform.position, currentPatrolTarget.position) < 0.5f)
        {
            // Swap the target
            currentPatrolTarget = (currentPatrolTarget == pointA) ? pointB : pointA;
        }

        // 2. Decide which way to walk based on the target's position
        direction = (currentPatrolTarget.position.x > transform.position.x) ? 1 : -1;

        // 3. Optional: Keep the ledge safety check just in case you place a point off a cliff!
        bool atLedge = !Physics2D.OverlapCircle(ledgeCheck.position, 0.1f, mask);
        if (atLedge)
        {
            // Force them back if they almost fall off
            currentPatrolTarget = (currentPatrolTarget == pointA) ? pointB : pointA;
            direction *= -1;
        }

        // 4. Apply movement and flip sprite
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
}