using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{
    public Animator anim {get; private set; }

    public Rigidbody2D rb { get; private set; }

    public PlayerInputSet input { get; private set; }
    private StateMachine stateMachine;

    public Player_IdleState idleState { get; private set; }
    public Player_MoveState moveState { get; private set; }


    private bool facingRight = false; // to keep track of which direction the player is facing
    public int facingDir { get; private set; } = -1; // 1 for right, -1 for left
    public Vector2 moveInput { get; private set; }


    [Header ("Movement details")]
    public float moveSpeed = 2.5f;
    public float sprintSpeed = 4f;
    public float maxSprintTime = 5f;
    public float sprintCooldown = 4f;

    public bool isSprintHeld { get; private set; }
    public bool canSprint => sprintCooldownTimer <= 0f && sprintTimeRemaining > 0f;

    private float sprintCooldownTimer;
    private float sprintTimeRemaining;

    private void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();

        input = new PlayerInputSet();
        stateMachine = new StateMachine();

        idleState = new Player_IdleState(this, stateMachine, "idle");
        moveState = new Player_MoveState(this, stateMachine, "move");

        sprintTimeRemaining = maxSprintTime;
    }


    private void OnEnable()
    {
        if (input == null)
            return;
        input.Enable();

        input.Player.Movement.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        input.Player.Movement.canceled += ctx => moveInput = Vector2.zero;
        
        input.Player.Sprint.performed += ctx => isSprintHeld = true;
        input.Player.Sprint.canceled += ctx => isSprintHeld = false;
    }


    private void OnDisable()
    {
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

        stateMachine.UpdateActiveState();
    }


    public void CallAnimationTrigger()
    {
        stateMachine.currentState.CallAnimationTrigger();
    }
    


    public void SetVelocity(float xVelocity, float yVelocity)
    {
        rb.linearVelocity = new Vector2(xVelocity, yVelocity);
        HandleFlip(xVelocity);
        Debug.Log("SetVelocity called: " + rb.linearVelocity);
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
        Debug.Log("Flipped. facingRight = " + facingRight);
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
        anim.SetBool("isMoving", isMoving);

        if (!isMoving)
            return;

        // prioritize up/down over side
        if (move.y > 0)
        {
            anim.SetInteger("direction", 2); // up
        }
        else if (move.y < 0)
        {
            anim.SetInteger("direction", 0); // down
        }
        else
        {
            anim.SetInteger("direction", 1); // side
        }
    }

}
