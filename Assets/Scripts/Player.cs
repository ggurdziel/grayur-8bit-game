using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{
    public static Player instance;

    public Animator anim {get; private set; }

    public Rigidbody2D rb { get; private set; }

    public PlayerInputSet input { get; private set; }
    private StateMachine stateMachine;

    public Player_IdleState idleState { get; private set; }
    public Player_MoveState moveState { get; private set; }
    public Player_BasicAttackState basicAttackState { get; private set; }


    private bool facingRight = false; // to keep track of which direction the player is facing
    public int facingDir { get; private set; } = -1; // 1 for right, -1 for left

    // Always exactly up, down, left, right, or zero (never diagonal).
    public Vector2 moveInput { get; private set; }

    private const float inputDeadzone = 0.3f;


    [Header ("Movement details")]
    public float moveSpeed = 2.5f;
    public float sprintSpeed = 4f;
    public float maxSprintTime = 5f;
    public float sprintCooldown = 4f;
    private static readonly string[] moveStates = { "playerMoveDown", "playerMoveSide", "playerMoveUp" };
    private int lastAnimKey = -2;   // -1 = idle, 0/1/2 = direction

    public bool isSprintHeld { get; private set; }
    public bool canSprint => sprintCooldownTimer <= 0f && sprintTimeRemaining > 0f;

    private float sprintCooldownTimer;
    private float sprintTimeRemaining;


    private IInteractable currentInteractable;
    private Inventory_Player inventory;


    private void Awake()
    {
        instance = this;
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();

        input = new PlayerInputSet();
        stateMachine = new StateMachine();
        inventory = GetComponent<Inventory_Player>();

        idleState = new Player_IdleState(this, stateMachine, "idle");
        moveState = new Player_MoveState(this, stateMachine, "move");
        basicAttackState = new Player_BasicAttackState(this, stateMachine, "attack");

        sprintTimeRemaining = maxSprintTime;
    }


    private void OnEnable()
    {
        if (input == null)
            return;
        input.Enable();

        input.Player.Movement.performed += ctx => moveInput = ToCardinal(ctx.ReadValue<Vector2>());
        input.Player.Movement.canceled += ctx => moveInput = Vector2.zero;
        
        input.Player.Sprint.performed += ctx => isSprintHeld = true;
        input.Player.Sprint.canceled += ctx => isSprintHeld = false;

        input.Player.Interact.performed += ctx =>
        {
            if (currentInteractable != null)
            {
                currentInteractable.Interact(this);
            }
        };

        //input.Player.Attack.performed += ctx => stateMachine.ChangeState(basicAttackState);

        input.Player.Hotbar1.performed += ctx => inventory.SelectHotbarSlot(0);
        input.Player.Hotbar2.performed += ctx => inventory.SelectHotbarSlot(1);
        input.Player.Hotbar3.performed += ctx => inventory.SelectHotbarSlot(2);
        input.Player.Hotbar4.performed += ctx => inventory.SelectHotbarSlot(3);
        input.Player.Hotbar5.performed += ctx => inventory.SelectHotbarSlot(4);

        input.Player.DropItem.performed += ctx => DropSelectedItem();
    }


    private void OnDisable()
    {
        if (input != null)
            input.Disable();
    }


    public void Start()
    {
        stateMachine.Initialize(idleState);
    }


    private void Update()
    {
        bool wantsToSprint = isSprintHeld && moveInput != Vector2.zero;

        if (IsSprinting(wantsToSprint))
        {
            HandleSprint();
        }
        else
        {
            HandleCooldown();
        }

        if (stateMachine != null && stateMachine.currentState != null)
        {
            stateMachine.UpdateActiveState();
        }
    }


    public void CallAnimationTrigger()
    {
        stateMachine.currentState.CallAnimationTrigger();
    }


    // Snaps any input to exactly one of: up, down, left, right, or zero.
    // When two keys are held, the newer one wins, so you can turn without letting go.
    private Vector2 ToCardinal(Vector2 raw)
    {
        float ax = Mathf.Abs(raw.x);
        float ay = Mathf.Abs(raw.y);

        if (ax < inputDeadzone && ay < inputDeadzone)
            return Vector2.zero;

        bool horizontal;

        if (Mathf.Abs(ax - ay) < 0.1f)
        {
            // Diagonal: switch away from the axis we're already moving on.
            bool wasHorizontal = Mathf.Abs(moveInput.x) > 0f;
            bool wasVertical = Mathf.Abs(moveInput.y) > 0f;

            if (wasHorizontal) horizontal = false;
            else if (wasVertical) horizontal = true;
            else horizontal = false;   // both pressed at once: prefer up/down
        }
        else
        {
            horizontal = ax > ay;
        }

        return horizontal
            ? new Vector2(Mathf.Sign(raw.x), 0f)
            : new Vector2(0f, Mathf.Sign(raw.y));
    }


    public void SetVelocity(float xVelocity, float yVelocity)
    {
        rb.linearVelocity = new Vector2(xVelocity, yVelocity);
        HandleFlip(xVelocity);
    }


    private void HandleFlip(float xVelocity)
    {
        if (xVelocity > 0 && !facingRight)
        {
            Flip();
        }
        else if (xVelocity < 0 && facingRight)
        {
            Flip();
        }
    }


    public void Flip()
    {
        transform.Rotate(0, 180, 0);
        facingRight = !facingRight;
        facingDir *= -1;
    }
    


    private bool IsSprinting(bool wantsToSprint)
    {
        return wantsToSprint && sprintCooldownTimer <= 0f && sprintTimeRemaining > 0f;
    }


    private void HandleSprint()
    {
        sprintTimeRemaining -= Time.deltaTime;

        if (sprintTimeRemaining <= 0f)
        {
            sprintTimeRemaining = 0f;
            StartSprintCooldown();
        }
    }


    private void HandleCooldown()
    {
        if (sprintCooldownTimer > 0f)
        {
            sprintCooldownTimer -= Time.deltaTime;

            if (sprintCooldownTimer <= 0f)
            {
                sprintCooldownTimer = 0f;
                sprintTimeRemaining = maxSprintTime;
            }
        }
    }

    private void StartSprintCooldown()
    {
        sprintCooldownTimer = sprintCooldown;
    }


    public void UpdateAnimation(Vector2 move)
    {
        bool isMoving = move != Vector2.zero;
        anim.SetBool("move", isMoving);

        int dir = 1;                              // side
        if (move.y > 0) dir = 2;                  // up
        else if (move.y < 0) dir = 0;             // down

        if (isMoving)
        {
            anim.SetInteger("direction", dir);
        }

        // Don't interrupt the throw clip.
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("playerThrow"))
            return;

        // Only jump clips when what we should be showing actually changes.
        int key = isMoving ? dir : -1;
        if (key == lastAnimKey)
            return;

        lastAnimKey = key;
        anim.Play(isMoving ? moveStates[dir] : "playerIdle", 0, 0f);
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        IInteractable interactable = collision.GetComponent<IInteractable>();
        if (interactable != null)
        {
            currentInteractable = interactable;
        }
    }


    private void OnTriggerExit2D(Collider2D collision)
    {
        IInteractable interactable = collision.GetComponent<IInteractable>();
        if (interactable != null && currentInteractable == interactable)
        {
            currentInteractable = null;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        IInteractable interactable = collision.collider.GetComponentInParent<IInteractable>();
        if (interactable != null)
        {
            currentInteractable = interactable;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        IInteractable interactable = collision.collider.GetComponentInParent<IInteractable>();
        if (interactable != null && currentInteractable == interactable)
        {
            currentInteractable = null;
        }
    }


    private void DropSelectedItem()
    {
        if (inventory == null)
            return;

        Inventory_Item selectedItem = inventory.GetSelectedItem();

        if (selectedItem == null || selectedItem.itemData == null)
        {
            Debug.Log("No item selected to drop.");
            return;
        }

        if (selectedItem.itemData.worldPrefab == null)
        {
            Debug.LogWarning("Selected item has no world prefab assigned.");
            return;
        }

        Vector3 spawnOffset = new Vector3(facingDir * 0.75f, 0.2f, 0f);

        GameObject droppedObject = Instantiate(
            selectedItem.itemData.worldPrefab,
            transform.position + spawnOffset,
            Quaternion.identity
        );

        Rigidbody2D droppedRb = droppedObject.GetComponent<Rigidbody2D>();
        if (droppedRb != null)
        {
            Vector2 throwDirection = new Vector2(facingDir, 0.25f).normalized;
            droppedRb.AddForce(throwDirection * 4f, ForceMode2D.Impulse);
        }

        inventory.RemoveSelectedItem();
    }
}