using UnityEngine;

public class PlayerPush : MonoBehaviour
{
    [Header("Push")]
    public float pushRange = 1.2f;
    public float pushSpeed = 3f;
    public LayerMask boxLayer;

    private BoxPushable currentBox;
    private bool isPushing = false;
    private MotherMovement motherMovement;

    void Start()
    {
        motherMovement = GetComponent<MotherMovement>();

        if (motherMovement == null)
        {
            Debug.LogWarning("PlayerPush requires MotherMovement — disabling.");
            enabled = false;
        }
    }

    void Update()
    {
        // Detect box in front
        Vector2 facingDir = new Vector2(Mathf.Sign(transform.localScale.x), 0);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, facingDir, pushRange, boxLayer);

        if (hit.collider != null)
        {
            BoxPushable box = hit.collider.GetComponent<BoxPushable>();

            if (box != null)
            {
                // Press P to start pushing
                if (Input.GetKeyDown(KeyCode.P))
                {
                    currentBox = box;
                    isPushing = true;
                    float dir = Mathf.Sign(transform.localScale.x);
                    box.StartPush(dir, pushSpeed);
                }

                // Release P to stop pushing
                if (Input.GetKeyUp(KeyCode.P))
                {
                    if (currentBox != null)
                    {
                        currentBox.StopPush();
                        currentBox = null;
                    }
                    isPushing = false;
                }
            }
        }
        else
        {
            // No box in range — stop pushing
            if (currentBox != null)
            {
                currentBox.StopPush();
                currentBox = null;
            }
            isPushing = false;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Vector2 facingDir = new Vector2(Mathf.Sign(transform.localScale.x), 0);
        Gizmos.DrawLine(transform.position, (Vector2)transform.position + facingDir * pushRange);
    }
}