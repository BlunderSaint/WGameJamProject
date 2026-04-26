using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SimpleEnemy : MonoBehaviour
{
    [Header("Movement Speeds")]
    public float walkSpeed = 1.5f;
    public float runSpeed = 3.5f;

    [Header("Detection")]
    public float range = 8f;
    public LayerMask mask;
    public float detectionTime = 2f;

    private float detectionMeter = 0f;
    private bool isAlerted = false;

    [Header("UI")]
    public Slider detectionSlider;

    private Rigidbody2D rb;
    private int direction = 1;
    private GameObject[] players;
    private Vector2 lastKnownPos;

    [Header("Patrol Points")]
    public Transform pointA;
    public Transform pointB;
    private Transform currentPatrolTarget;

    [Header("Health")]
    public int maxHealth = 3;
    private int currentHealth;
    public SpriteRenderer sr;

    private bool canDamage = true;

    void Start()
    {

        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();

        // Find both Mother and Daughter by their tags
        GameObject mother = GameObject.FindGameObjectWithTag("Mother");
        GameObject daughter = GameObject.FindGameObjectWithTag("Daughter");

        // Add whichever ones exist in the scene
        if (mother != null && daughter != null)
            players = new GameObject[] { mother, daughter };
        else if (mother != null)
            players = new GameObject[] { mother };
        else if (daughter != null)
            players = new GameObject[] { daughter };

        currentPatrolTarget = pointA;

        detectionSlider = GetComponentInChildren<Slider>();

        if (detectionSlider != null)
        {
            detectionSlider.minValue = 0;
            detectionSlider.maxValue = detectionTime;
            detectionSlider.value = 0;
            detectionSlider.gameObject.SetActive(false);
        }
    }

    IEnumerator FlashRed()
    {
        if (sr == null) yield break;  // safety check

        Color originalColor = sr.color;
        sr.color = Color.red;
        yield return new WaitForSeconds(0.2f);
        sr.color = originalColor;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log($"Enemy hit! HP left: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();  // destroy immediately — coroutine never runs
        }
        else
        {
            StartCoroutine(FlashRed());  // ✅ only flash if still alive
        }
    }

    void Die()
    {
        Debug.Log("Enemy Died!");
        Destroy(gameObject);
    }
    void Update()
    {
        GameObject visiblePlayer = GetVisiblePlayer();

        // ================= DETECTION =================
        if (visiblePlayer != null)
        {
            detectionMeter += Time.deltaTime;
            detectionMeter = Mathf.Clamp(detectionMeter, 0, detectionTime);

            lastKnownPos = visiblePlayer.transform.position;

            if (detectionMeter >= detectionTime)
                isAlerted = true;
        }
        else
        {
            detectionMeter -= Time.deltaTime;
            detectionMeter = Mathf.Clamp(detectionMeter, 0, detectionTime);

            if (detectionMeter <= 0)
                isAlerted = false;
        }

        // ================= MOVEMENT =================
        if (isAlerted)
        {
            MoveToward(lastKnownPos.x, runSpeed);
        }
        else
        {
            Patrol();
        }

        // ================= UI =================
        if (detectionSlider != null)
        {
            // Show / Hide
            detectionSlider.gameObject.SetActive(detectionMeter > 0 || isAlerted);

            // Value — always reflect real detectionMeter
            detectionSlider.value = detectionMeter;

            // 🎨 Color change
            if (detectionSlider.fillRect != null)
            {
                Image fill = detectionSlider.fillRect.GetComponent<Image>();
                float percent = detectionMeter / detectionTime;

                if (percent < 0.5f)
                    fill.color = Color.green;
                else if (percent < 1f)
                    fill.color = Color.yellow;
                else
                    fill.color = Color.red; // covers both fully detected AND alerted
            }
        }
    }

    void MoveToward(float targetX, float speed)
    {
        direction = (targetX > transform.position.x) ? 1 : -1;
        rb.linearVelocity = new Vector2(direction * speed, rb.linearVelocity.y);
        transform.localScale = new Vector3(direction, 1, 1);
    }

    void Patrol()
    {
        if (Mathf.Abs(transform.position.x - currentPatrolTarget.position.x) < 0.5f)
        {
            currentPatrolTarget = (currentPatrolTarget == pointA) ? pointB : pointA;
        }

        direction = (currentPatrolTarget.position.x > transform.position.x) ? 1 : -1;
        rb.linearVelocity = new Vector2(direction * walkSpeed, rb.linearVelocity.y);
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
            float dirToPlayer = Mathf.Sign(target.position.x - transform.position.x);
            float facingDir = Mathf.Sign(transform.localScale.x);

            if (dirToPlayer == facingDir)
            {
                RaycastHit2D hit = Physics2D.Linecast(transform.position, target.position, mask);
                return hit.collider == null;
            }
        }
        return false;
    }

   
    // Inside SimpleEnemy.cs

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Mother"))
        {
            PlayerHealth health = collision.gameObject.GetComponent<PlayerHealth>();
            if (health != null)
            {
                // ✅ Damage and Knockback happen INSTANTLY
                health.TakeDamage(1, transform.position);
            }
        }

        if (collision.gameObject.CompareTag("Daughter"))
        {
            Time.timeScale = 0f; // Game Over
        }
    }

    // ================= GIZMOS =================
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);

        if (pointA != null && pointB != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(pointA.position, 0.2f);
            Gizmos.DrawSphere(pointB.position, 0.2f);
            Gizmos.DrawLine(pointA.position, pointB.position);
        }
    }
}