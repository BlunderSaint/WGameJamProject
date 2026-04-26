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

    [Header("UI")]
    public Slider healthSlider;

    void Start()
    {
        currentHealth = maxHealth;
        sr = GetComponentInChildren<SpriteRenderer>();

        healthSlider = GetComponentInChildren<Slider>();

        if (healthSlider != null)
        {
            healthSlider.minValue = 0;
            healthSlider.maxValue = maxHealth;
            healthSlider.value = maxHealth;   // start full
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        Debug.Log($"Mother hit! HP left: {currentHealth}");

        UpdateSlider();  // 🔄 update UI on every hit

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

    void UpdateSlider()
    {
        if (healthSlider == null) return;

        healthSlider.value = currentHealth;

        // 🎨 Color changes based on HP remaining
        if (healthSlider.fillRect != null)
        {
            Image fill = healthSlider.fillRect.GetComponent<Image>();
            float percent = (float)currentHealth / maxHealth;

            if (percent > 0.5f)
                fill.color = Color.green;
            else if (percent > 0.25f)
                fill.color = Color.yellow;
            else
                fill.color = Color.red;
        }
    }

    IEnumerator FlashRed()
    {
        if (sr == null) yield break;
        Color original = sr.color;
        sr.color = Color.red;
        yield return new WaitForSeconds(0.2f);
        sr.color = original;
    }

    void Die()
    {
        Debug.Log("Mother Died! GAME OVER");
        Time.timeScale = 0f;
    }
}