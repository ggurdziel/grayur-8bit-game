using UnityEngine;

public class Object_Animal : MonoBehaviour
{
    [SerializeField] private float speed = 1f;
    [SerializeField] private LayerMask blockingLayer;
    [SerializeField] private float walkTime = 3f;
    [SerializeField] private float idleTime = 1.5f;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;

    private Vector2 moveDir;
    private float timer;
    private bool isIdle = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        sr = GetComponentInChildren<SpriteRenderer>();
    }

    private void Start()
    {
        if (sr != null)
            sr.flipX = false; // default graphic faces left

        StartIdle();
    }

    private void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            if (isIdle)
                StartWalking();
            else
                StartIdle();
        }
    }

    private void FixedUpdate()
    {
        if (rb == null)
            return;

        rb.linearVelocity = moveDir * speed;
    }

    private void StartIdle()
    {
        isIdle = true;
        moveDir = Vector2.zero;
        timer = idleTime;

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        if (anim != null)
            anim.SetBool("isMoving", false);
    }

    private void StartWalking()
    {
        isIdle = false;
        timer = walkTime;

        int r = Random.Range(0, 4);

        if (r == 0)
            moveDir = Vector2.left;
        else if (r == 1)
            moveDir = Vector2.right;
        else if (r == 2)
            moveDir = Vector2.up;
        else
            moveDir = Vector2.down;

        if (sr != null && moveDir.x != 0)
            sr.flipX = moveDir.x > 0; // sprite faces left by default

        if (anim != null)
            anim.SetBool("isMoving", true);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & blockingLayer) != 0)
        {
            StartIdle();
        }
    }
}