using UnityEngine;

public class ChappleProjectile : MonoBehaviour
{
    public float speed = 10f;
    public float lifeTime = 1f;
    public int damage = 1;
    private Vector2 direction;

    [SerializeField] private LayerMask groundLayer;

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;

        // Face the direction of travel
        if (direction.x < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            transform.localScale = new Vector3(1, 1, 1);
        }

        // Auto-destroy after lifeTime expires
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // Move the projectile forward
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. Check if we hit an Enemy
        if (collision.CompareTag("Enemy"))
        {
            SimpleEnemy enemy = collision.GetComponent<SimpleEnemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }

            Destroy(gameObject); // Remove projectile on hit
        }
        // 2. Check if we hit the Ground (using Bitmask check)
        else if ((groundLayer.value & (1 << collision.gameObject.layer)) != 0)
        {
            Destroy(gameObject);
        }
    }
}