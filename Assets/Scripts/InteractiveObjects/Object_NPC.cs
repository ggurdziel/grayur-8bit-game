using UnityEngine;

public class Object_NPC : MonoBehaviour, IInteractable
{
    [Header("Dialogue")]
    [SerializeField] private DialogueLineSO firstDialogueLine;

    [Header("Wandering")]
    [SerializeField] private bool canWander = true;
    [SerializeField] private float moveSpeed = 1.5f;

    [Header("Walk Schedule")]
    [SerializeField] private float minWalkTime = 1.5f;
    [SerializeField] private float maxWalkTime = 3f;

    [Header("Idle")]
    [SerializeField] private float minIdleTime = 1f;
    [SerializeField] private float maxIdleTime = 3f;

    [Header("Collision")]
    [SerializeField] private LayerMask buildingLayer;
    [SerializeField] private float flipCooldown = 0.2f;

    protected UI ui;
    protected Inventory_NPC npcInventory;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;

    private Vector2 moveDir;
    private float timer;
    private bool isIdle = true;
    private int scheduleIndex = 0;
    private float lastFlipTime;

    protected virtual void Awake()
    {
        ui = FindFirstObjectByType<UI>();
        npcInventory = GetComponent<Inventory_NPC>();

        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        sr = GetComponentInChildren<SpriteRenderer>();
    }

    private void Start()
    {
        StartIdle();
    }

    private void Update()
    {
        if (!canWander)
            return;

        timer -= Time.deltaTime;

        if (timer <= 0)
            NextScheduleStep();

        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        if (!canWander || rb == null)
            return;

        rb.linearVelocity = moveDir * moveSpeed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (IsInBuildingLayer(collision.gameObject))
        {
            FlipDirection();
        }
    }

    private bool IsInBuildingLayer(GameObject obj)
    {
        return ((1 << obj.layer) & buildingLayer) != 0;
    }

    private void FlipDirection()
    {
        if (Time.time - lastFlipTime < flipCooldown)
            return;

        lastFlipTime = Time.time;

        if (moveDir == Vector2.zero)
            return;

        moveDir = -moveDir;

        if (rb != null)
        {
            rb.linearVelocity = moveDir * moveSpeed;
            rb.MovePosition(rb.position + moveDir * 0.05f);
        }

        UpdateAnimation();
    }

    private void NextScheduleStep()
    {
        scheduleIndex++;

        switch (scheduleIndex % 8)
        {
            case 0:
                StartWalking(Vector2.right);
                break;

            case 1:
                StartIdle();
                break;

            case 2:
                StartWalking(Vector2.up);
                break;

            case 3:
                StartIdle();
                break;

            case 4:
                StartWalking(Vector2.left);
                break;

            case 5:
                StartIdle();
                break;

            case 6:
                StartWalking(Vector2.down);
                break;

            case 7:
                StartIdle();
                break;
        }
    }

    private void StartIdle()
    {
        isIdle = true;
        moveDir = Vector2.zero;
        timer = Random.Range(minIdleTime, maxIdleTime);

        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }

    private void StartWalking(Vector2 direction)
    {
        isIdle = false;
        moveDir = direction.normalized;
        timer = Random.Range(minWalkTime, maxWalkTime);
    }

    private void UpdateAnimation()
    {
        if (anim == null)
            return;

        anim.SetBool("isMoving", !isIdle);

        if (isIdle)
            return;

        if (Mathf.Abs(moveDir.x) > Mathf.Abs(moveDir.y))
        {
            anim.SetInteger("direction", 1);

            if (sr != null)
                sr.flipX = moveDir.x < 0;
        }
        else if (moveDir.y > 0)
        {
            anim.SetInteger("direction", 2);
        }
        else if (moveDir.y < 0)
        {
            anim.SetInteger("direction", 0);
        }
    }

    public virtual void Interact(Player player)
    {
        Debug.Log("NPC interacted");

        if (player == null)
            return;

        Inventory_Player playerInventory = player.GetComponent<Inventory_Player>();

        if (playerInventory != null)
        {
            Inventory_Item selectedItem = playerInventory.GetSelectedItem();

            if (selectedItem != null && selectedItem.itemData != null)
            {
                if (npcInventory != null && npcInventory.CanAddItem(selectedItem))
                {
                    npcInventory.AddItem(selectedItem);
                    playerInventory.RemoveSelectedItem();

                    Debug.Log("Gave " + selectedItem.itemData.itemName + " to NPC.");
                    return;
                }
            }
        }

        if (ui == null)
        {
            Debug.LogError("UI not found in scene.");
            return;
        }

        ui.OpenDialogueUI(firstDialogueLine);
    }
}