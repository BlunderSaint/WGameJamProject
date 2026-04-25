using UnityEngine;

public class DaughterMovement : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float airControl = 0.5f;

    [Header("Jump")]
    public float jumpForce = 7f;
    public float jumpCutMultiplier = 0.5f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Ladder")]
    public float climbSpeed = 3f;
    public LayerMask ladderLayer;

    [Header("Crouch")]
    public float crouchSpeed = 2f;

    private Rigidbody2D rb;
    private Collider2D playerCollider;
    private CapsuleCollider2D capsuleCollider;
    private float moveInput;
    private float climbInput;
    private bool isGrounded;
    private bool isFacingRight = true;
    private bool isOnLadder = false;
    private bool isClimbing = false;
    private bool isCrouching = false;

    private float originalColliderHeight;
    private float originalColliderOffset;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();

        if (capsuleCollider != null)
        {
            originalColliderHeight = capsuleCollider.size.y;
            originalColliderOffset = capsuleCollider.offset.y;
        }
    }

    void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");
        climbInput = Input.GetAxisRaw("Vertical");

        // Ground check
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // Ladder check
        isOnLadder = Physics2D.OverlapCircle(transform.position, 0.2f, ladderLayer);

        // Start climbing
        if (isOnLadder && Mathf.Abs(climbInput) > 0)
        {
            isClimbing = true;
            IgnoreGroundColliders(true);
        }

        // Stop climbing if no ladder above when going up
        if (isClimbing && climbInput > 0)
        {
            RaycastHit2D ladderAbove = Physics2D.Raycast(transform.position, Vector2.up, 0.3f, ladderLayer);
            if (ladderAbove.collider == null)
            {
                isClimbing = false;
                rb.gravityScale = 1f;
                IgnoreGroundColliders(false);
            }
        }

        // Allow entering ladder from top by pressing down
        if (climbInput < 0 && !isOnLadder)
        {
            RaycastHit2D ladderBelow = Physics2D.Raycast(transform.position, Vector2.down, 1f, ladderLayer);
            if (ladderBelow.collider != null)
            {
                isClimbing = true;
                IgnoreGroundColliders(true);
            }
        }

        // Stop climbing when leaving ladder
        if (!isOnLadder && !isClimbing)
        {
            rb.gravityScale = 1f;
            IgnoreGroundColliders(false);
        }

        // Restore when fully off ladder going down
        if (!isOnLadder && isClimbing && rb.linearVelocity.y < -0.1f)
        {
            isClimbing = false;
            rb.gravityScale = 1f;
            IgnoreGroundColliders(false);
        }

        // Crouch
        if (Input.GetKeyDown(KeyCode.C) && isGrounded && !isClimbing)
        {
            isCrouching = true;
            SetCrouchCollider(true);
        }
        if (Input.GetKeyUp(KeyCode.C))
        {
            isCrouching = false;
            SetCrouchCollider(false);
        }

        // Cancel crouch if not grounded
        if (!isGrounded && isCrouching)
        {
            isCrouching = false;
            SetCrouchCollider(false);
        }

        // Jump
        if (Input.GetKeyDown(KeyCode.Space) && (isGrounded || isClimbing))
        {
            isCrouching = false;
            SetCrouchCollider(false);
            isClimbing = false;
            rb.gravityScale = 1f;
            IgnoreGroundColliders(false);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }

        // Cut jump
        if (Input.GetKeyUp(KeyCode.Space) && rb.linearVelocity.y > 0 && !isClimbing)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
        }

        // Flip
        if (moveInput > 0 && !isFacingRight) Flip();
        else if (moveInput < 0 && isFacingRight) Flip();
    }

    void FixedUpdate()
    {
        if (isClimbing)
        {
            rb.gravityScale = 0f;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, climbInput * climbSpeed);
        }
        else
        {
            rb.gravityScale = 1f;

            float speed;
            if (isCrouching)
                speed = crouchSpeed;
            else if (Input.GetKey(KeyCode.LeftShift))
                speed = runSpeed;
            else
                speed = walkSpeed;

            float control = isGrounded ? 1f : airControl;
            rb.linearVelocity = new Vector2(moveInput * speed * control, rb.linearVelocity.y);
        }
    }

    void SetCrouchCollider(bool crouching)
    {
        if (capsuleCollider == null) return;

        if (crouching)
        {
            capsuleCollider.size = new Vector2(capsuleCollider.size.x, originalColliderHeight / 2f);
            capsuleCollider.offset = new Vector2(capsuleCollider.offset.x, originalColliderOffset - originalColliderHeight / 4f);
        }
        else
        {
            capsuleCollider.size = new Vector2(capsuleCollider.size.x, originalColliderHeight);
            capsuleCollider.offset = new Vector2(capsuleCollider.offset.x, originalColliderOffset);
        }
    }

    void IgnoreGroundColliders(bool ignore)
    {
        Collider2D[] groundColliders = Physics2D.OverlapAreaAll(
            new Vector2(-1000, -1000),
            new Vector2(1000, 1000),
            groundLayer
        );

        foreach (Collider2D ground in groundColliders)
        {
            Physics2D.IgnoreCollision(playerCollider, ground, ignore);
        }
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}