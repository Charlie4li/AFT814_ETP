using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private PlayerInputActions playerInputActions;
    private InputAction ia_Movement;
    private InputAction ia_Interaction;
    private float movementInput;
    private SpriteRenderer spriteRenderer;
    private float speed;
    private Vector2 lastPosition;

    [Header("Rigid Body")]
    public Rigidbody2D rbPlayer;

    [Header("Movement Related")]
    public float movementSpeed = 5;
    public float jumpForce = 10f;
    public float launchForce = 15f; // Adjustable launch force

    [Header("Ground Check Related")]
    public Vector2 BoxSize;
    public float castDistance = 2;
    public LayerMask groundLayer;

    [Header("VFX")]
    public GameObject launchVFXPrefab; // Assign your VFX prefab in inspector
    public Transform vfxSpawnPoint; // Where the VFX should spawn

    private bool isFacingRight = true;

    private void Awake()
    {
        playerInputActions = new PlayerInputActions();
        spriteRenderer = GetComponent<SpriteRenderer>();

        ia_Movement = playerInputActions.Walking.Movement;
        ia_Movement.Enable();

        ia_Interaction = playerInputActions.Walking.Interaction;
        ia_Interaction.started += IA_InteractionStarted;
        ia_Interaction.Enable();

        playerInputActions.Walking.Jump.started += IA_JumpStarted;
        playerInputActions.Walking.Jump.Enable();
    }

    private void Start()
    {
        lastPosition = transform.position;
    }

    private void FixedUpdate()
    {
        // Handle movement in FixedUpdate for physics
        MovePlayer();
    }

    void Update()
    {
        movementInput = ia_Movement.ReadValue<float>();
        FlipSpriteHorizontallyBasedOnInput();
        CalculateSpeed();
    }

    private void MovePlayer()
    {
        if (IsTouchingGround())
        {
            rbPlayer.linearVelocity = new Vector2(movementInput * movementSpeed, rbPlayer.linearVelocity.y);
        }
        else
        {
            // Allow some air control but less than on ground
            rbPlayer.linearVelocity = new Vector2(movementInput * movementSpeed * 0.7f, rbPlayer.linearVelocity.y);
        }
    }

    public bool IsTouchingGround()
    {
        RaycastHit2D hit = Physics2D.BoxCast(transform.position, BoxSize, 0, -transform.up, castDistance, groundLayer);
        return hit.collider != null;
    }

    public bool IsFacingLeft()
    {
        return !isFacingRight;
    }

    private void IA_JumpStarted(InputAction.CallbackContext context)
    {
        JumpExecution();
    }

    private void IA_InteractionStarted(InputAction.CallbackContext context)
    {
        InteractionExecution();
    }

    public void JumpExecution()
    {
        if (IsTouchingGround())
        {
            rbPlayer.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
        }
    }

    public void InteractionExecution()
    {
        // Launch the player backwards
        Vector2 launchDirection = isFacingRight ? Vector2.left : Vector2.right;
        rbPlayer.AddForce(launchDirection * launchForce, ForceMode2D.Impulse);

        // Spawn VFX
        if (launchVFXPrefab != null && vfxSpawnPoint != null)
        {
            Instantiate(launchVFXPrefab, vfxSpawnPoint.position, Quaternion.identity);
        }
    }

    private void CalculateSpeed()
    {
        float distance = Vector2.Distance(lastPosition, transform.position);
        speed = distance / Time.deltaTime;
        lastPosition = transform.position;
    }

    private void FlipSpriteHorizontallyBasedOnInput()
    {
        if (movementInput < 0 && isFacingRight)
        {
            spriteRenderer.flipX = true;
            isFacingRight = false;
        }
        else if (movementInput > 0 && !isFacingRight)
        {
            spriteRenderer.flipX = false;
            isFacingRight = true;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position - transform.up * castDistance, BoxSize);
    }

    private void OnDestroy()
    {
        // Clean up input actions
        ia_Movement.Disable();
        ia_Interaction.Disable();
        playerInputActions.Walking.Jump.Disable();
    }
}