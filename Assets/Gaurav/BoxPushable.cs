using UnityEngine;

public class BoxPushable : MonoBehaviour
{
    private Rigidbody2D rb;
    private bool isBeingPushed = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // Box cant move by default
        rb.bodyType = RigidbodyType2D.Static;
    }

    public void StartPush(float direction, float pushSpeed)
    {
        isBeingPushed = true;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.linearVelocity = new Vector2(direction * pushSpeed, rb.linearVelocity.y);
    }

    public void StopPush()
    {
        isBeingPushed = false;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Static;
    }
}