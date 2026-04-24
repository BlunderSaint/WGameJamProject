using UnityEngine;
using System.Collections;

public abstract class EnemyBase : MonoBehaviour
{
    [Header("Base Stats")]
    public float maxHealth = 100f;
    protected float currentHealth;
    public float contactDamage = 10f;
    public float knockbackForce = 5f; // (kept for future use if needed)

    protected Rigidbody2D rb;
    protected SpriteRenderer sprite;
    protected Animator anim;
    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        currentHealth = maxHealth;
    }

    /* ===================== DAMAGE FROM PLAYER ===================== */

    public virtual void TakeDamage(float damage)
    {
        currentHealth -= damage;
        Debug.Log($"{gameObject.name} took {damage} damage. Health: {currentHealth}");

        if (anim != null)
            anim.SetTrigger("Hurt");

        StartCoroutine(FlashRed());

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    /* ===================== DEAL DAMAGE TO PLAYER ===================== */

    //protected virtual void OnTriggerEnter2D(Collider2D collision)
    //{
    //    // This WILL print if the physics engine registers ANY overlap
    //    Debug.Log("<color=red>PHYSICS HIT:</color> Enemy touched by: " + collision.gameObject.name +
    //              " | Tag: " + collision.gameObject.tag +
    //              " | Layer: " + LayerMask.LayerToName(collision.gameObject.layer));

    //    if (collision.CompareTag("Player"))
    //    {
    //        if (playerCombat != null)
    //        {
    //            playerCombat.TakeDamage(contactDamage, transform.position);
    //        }
    //        else
    //        {
    //            Debug.LogError("Enemy has no PlayerCombat reference!");
    //        }
    //    }
    //}

    /* ===================== DEATH ===================== */

    protected virtual void Die()
    {
        if (anim != null)
        {
            anim.SetTrigger("Die");

            // Disable enemy behavior
            this.enabled = false;

            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.bodyType = RigidbodyType2D.Kinematic;
            }

            Collider2D col = GetComponent<Collider2D>();
            if (col != null)
                col.enabled = false;

            // Destroy after death animation
            Destroy(gameObject, 1f);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /* ===================== HIT FEEDBACK ===================== */

    private IEnumerator FlashRed()
    {
        if (sprite == null)
            yield break;

        Color original = sprite.color;
        sprite.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        sprite.color = original;
    }
}