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

    [Header("Rope")]
    public float ropeClimbSpeed = 2.5f;
    public LayerMask ropeLayer;

    [Header("Crouch")]
    public float crouchSpeed = 2f;

    private Rigidbody2D rb;
    private Collider2D playerCollider;
    private CapsuleCollider2D capsuleCollider;
    private Animator animator;

    private float moveInput;
    private float climbInput;

    private bool isGrounded;
    private bool isFacingRight = false;

    private bool isOnLadder = false;
    private bool isClimbing = false;

    private bool isOnRope = false;
    private bool isClimbingRope = false;

    private bool isCrouching = false;

    private float originalColliderHeight;
    private float originalColliderOffset;

    // 🔥 Animator Parameters
    private static readonly int ParamIsWalk = Animator.StringToHash("isWalking");
    private static readonly int ParamIsRun = Animator.StringToHash("isRunning");
    private static readonly int ParamIsClimb = Animator.StringToHash("isClimbing");
    private static readonly int ParamIsClimbRope = Animator.StringToHash("isClimbingRope");
    private static readonly int ParamClimbSpeed = Animator.StringToHash("climbSpeed");
    private static readonly int ParamIsCrouch = Animator.StringToHash("isCrouching");
    private static readonly int ParamIsJump = Animator.StringToHash("isJumping");

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        animator = GetComponent<Animator>();

        transform.localScale = new Vector3(1, 1, 1);

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

        // ================= GROUND =================
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // ================= LADDER =================
        isOnLadder = Physics2D.OverlapCircle(transform.position, 0.2f, ladderLayer);

        if (isOnLadder && Mathf.Abs(climbInput) > 0 && !isClimbingRope)
        {
            isClimbing = true;
            transform.position = new Vector3(transform.position.x, transform.position.y, -1f);
            IgnoreGroundColliders(true);
        }

        if (!isOnLadder && isClimbing)
        {
            isClimbing = false;
            transform.position = new Vector3(transform.position.x, transform.position.y, 0f);
            rb.gravityScale = 1f;
            IgnoreGroundColliders(false);
        }

        // ================= ROPE =================
        isOnRope = Physics2D.OverlapCircle(transform.position, 0.2f, ropeLayer);

        if (isOnRope && Mathf.Abs(climbInput) > 0 && !isClimbing)
        {
            isClimbingRope = true;
            IgnoreGroundColliders(true);
        }

        if (!isOnRope && isClimbingRope)
        {
            isClimbingRope = false;
            rb.gravityScale = 1f;
            IgnoreGroundColliders(false);
        }

        // ================= CROUCH (TOGGLE) =================
        if (Input.GetKeyDown(KeyCode.C) && isGrounded)
        {
            isCrouching = !isCrouching;
            SetCrouchCollider(isCrouching);
        }

        // ================= JUMP =================
        if (Input.GetKeyDown(KeyCode.Space) && (isGrounded || isClimbing || isClimbingRope))
        {
            isCrouching = false;
            SetCrouchCollider(false);

            isClimbing = false;
            isClimbingRope = false;

            rb.gravityScale = 1f;
            IgnoreGroundColliders(false);

            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }

        if (Input.GetKeyUp(KeyCode.Space) && rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
        }

        // ================= FLIP =================
        if (!isClimbingRope)
        {
            if (moveInput > 0 && !isFacingRight) Flip();
            else if (moveInput < 0 && isFacingRight) Flip();
        }

        // ================= ANIMATION =================
        UpdateAnimations();
    }

    void UpdateAnimations()
    {
        if (animator == null) return;

        bool isMoving = Mathf.Abs(moveInput) > 0.1f;

        // ================= JUMP — always checked first =================
        bool jumping = !isGrounded && !isClimbing && !isClimbingRope;

        animator.SetBool(ParamIsJump, jumping);

        if (jumping)
        {
            animator.speed = 1f;
            animator.SetBool(ParamIsWalk, false);
            animator.SetBool(ParamIsRun, false);
            animator.SetBool(ParamIsCrouch, false);
            animator.SetBool(ParamIsClimb, false);
            animator.SetBool(ParamIsClimbRope, false);
            return;
        }

        // ================= CLIMB =================
        animator.SetBool(ParamIsClimb, isClimbing);
        animator.SetBool(ParamIsClimbRope, isClimbingRope);

        if (isClimbing || isClimbingRope)
        {
            float climbSpeedValue = Mathf.Abs(climbInput);
            animator.SetFloat(ParamClimbSpeed, climbSpeedValue);
            animator.speed = (climbSpeedValue > 0.1f) ? 1f : 0f;

            animator.SetBool(ParamIsWalk, false);
            animator.SetBool(ParamIsRun, false);
            animator.SetBool(ParamIsCrouch, false);
            return;
        }

        // Reset speed
        animator.speed = 1f;

        // ================= CROUCH =================
        animator.SetBool(ParamIsCrouch, isCrouching);

        if (isCrouching)
        {
            animator.SetFloat("crouchSpeed", Mathf.Abs(moveInput));
            animator.SetBool(ParamIsWalk, false);
            animator.SetBool(ParamIsRun, false);
            return;
        }

        // ================= NORMAL =================
        bool isRunning = isGrounded && isMoving && Input.GetKey(KeyCode.LeftShift);
        bool isWalking = isGrounded && isMoving && !isRunning;

        animator.SetBool(ParamIsRun, isRunning);
        animator.SetBool(ParamIsWalk, isWalking);
    }
    void FixedUpdate()
    {
        if (isClimbing)
        {
            rb.gravityScale = 0f;
            rb.linearVelocity = new Vector2(0, climbInput * climbSpeed);
             // Ensure character is in front of ladder
        }
        else if (isClimbingRope)
        {
            rb.gravityScale = 0f;
            rb.linearVelocity = new Vector2(moveInput * 1f, climbInput * ropeClimbSpeed);
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