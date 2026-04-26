using UnityEngine;

public class ChappleProjectile : MonoBehaviour
{
    public float speed = 10f;
    public float lifeTime = 1f;  // changed to 1 seconds
    private Vector2 direction;

    [SerializeField] private LayerMask groundLayer;

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // moves straight in the set direction every frame
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Debug.Log("Player Hit!");
            // collision.GetComponent<PlayerHealth>()?.TakeDamage(1); // optional
            Destroy(gameObject);
        }
        else if ((groundLayer & (1 << collision.gameObject.layer)) != 0)
        {
            Destroy(gameObject);
        }
    }
}