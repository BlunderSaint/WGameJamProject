using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 4;
    private int currentHealth;
    private SpriteRenderer sr;
    private bool isDead = false;

    [Header("Knockback")]
    public float knockbackForce = 20f; // 💥 20 is usually a good "punchy" value for Impulse
    private Rigidbody2D rb;

    // We need a small timer to stop the PlayerMovement script from fighting the knockback
    public float knockbackDuration = 0.2f;
    private bool isBeingKnockedBack = false;

    [Header("UI")]
    public Slider healthSlider;

    void Start()
    {
        currentHealth = maxHealth;
        sr = GetComponentInChildren<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        // 🛠️ FIX 1: Ensure Rigidbody is set to Dynamic and Collision is Continuous
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        healthSlider = GetComponentInChildren<Slider>();
        if (healthSlider != null)
        {
            healthSlider.minValue = 0;
            healthSlider.maxValue = maxHealth;
            healthSlider.value = maxHealth;
        }
    }

    public void TakeDamage(int damage, Vector2 enemyPosition)
    {
        if (isDead) return;

        currentHealth -= damage;
        UpdateSlider();

        // 💥 APPLY KNOCKBACK
        ApplyKnockback(enemyPosition);

        if (currentHealth <= 0)
        {
            isDead = true;
            Die();
        }
        else
        {
            StartCoroutine(FlashRed());
        }
    }

    private void ApplyKnockback(Vector2 enemyPosition)
    {
        if (rb != null)
        {
            // 🛠️ FIX 2: Stop any Coroutines that might reset movement prematurely
            StopCoroutine(nameof(KnockbackTimer));

            float xDir = (transform.position.x > enemyPosition.x) ? 1f : -1f;
            Vector2 knockbackDirection = new Vector2(xDir, 0f);

            // 🛠️ FIX 3: Force velocity to zero so the impulse is pure
            rb.linearVelocity = Vector2.zero;

            // Apply the force
            rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);

            // Start the timer that tells the movement script to "Wait"
            StartCoroutine(KnockbackTimer());
        }
    }

    private IEnumerator KnockbackTimer()
    {
        isBeingKnockedBack = true;
        yield return new WaitForSeconds(knockbackDuration);
        isBeingKnockedBack = false;
    }

    // This public bool is used by MotherMovement.cs
    public bool IsKnockedBack() => isBeingKnockedBack;

    void UpdateSlider()
    {
        if (healthSlider == null) return;
        healthSlider.value = currentHealth;
        if (healthSlider.fillRect != null)
        {
            Image fill = healthSlider.fillRect.GetComponent<Image>();
            float percent = (float)currentHealth / maxHealth;
            fill.color = percent > 0.5f ? Color.green : (percent > 0.25f ? Color.yellow : Color.red);
        }
    }

    IEnumerator FlashRed()
    {
        if (sr == null) yield break;
        sr.color = Color.red;
        yield return new WaitForSeconds(0.15f);
        sr.color = Color.white;
    }

    void Die()
    {
        Time.timeScale = 0f;
    }
}