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
    public float moveSpeed;
    public float dashDuration = .25f;
    public float dashSpeed = 20;


    [Header ("Collision detection")]
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private LayerMask whatIsGround;

    public bool groundDetected { get; private set; } // property to get the ground detected;


    private void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();

        input = new PlayerInputSet();
        stateMachine = new StateMachine();

        idleState = new Player_IdleState(this, stateMachine, "idle");
        moveState = new Player_MoveState(this, stateMachine, "move");
    }


    private void OnEnable()
    {
        if (input == null)
            return;
        input.Enable();

        input.Player.Movement.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        input.Player.Movement.canceled += ctx => moveInput = Vector2.zero;
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
        Debug.Log("xVelocity: " + xVelocity);
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
    

}
