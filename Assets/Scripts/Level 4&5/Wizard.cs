using System.Collections;
using DefaultNamespace;
using UnityEngine;

public class Wizard : MonoBehaviour, IDamageable, IEdgeDetecter
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2.0f;
    [SerializeField] private float jumpForce = 7.5f;

    [Header("Combat Settings")]
    [SerializeField] private float hitReactionDelay = 0.1f;
    [SerializeField] private float deathDelay = 0.3f;
    [SerializeField] private int soulReward = 150;

    [Header("AI Settings")]
    [SerializeField] private float detectionRange = 6f;
    [SerializeField] private float edgeWaitTime = 2f;
    [SerializeField] private float returnTime = 3f;
    private Transform player;

    [Header("UI Elements")]
    [SerializeField] private GameObject floatingTextPrefab;
    [SerializeField] private Vector3 floatingTextOffset;

    private Animator animator;
    private Rigidbody2D rigidBody;
    private SpriteRenderer spriteRenderer;
    private WizardCombat wizardCombat;

    private bool isGrounded = false;
    public bool isDead;
    private bool receivedHit;
    private bool awaitingDeath;
    private float hitTimer;
    private float returnTimer;

    private bool isChasing;
    private bool atEdge;
    private Vector2 startingPosition;
    public int moveDirection = 1;

    [Header("Visual Effects")]
    public Renderer characterRenderer;
    private Color originalColor;
    public Color hurtColor = Color.white;
    public float flashDuration = 0.2f;

    private void Start()
    {
        InitializeComponents();

        if (player == null)
        {
            GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");
            if (foundPlayer != null)
            {
                player = foundPlayer.transform;
            }
        }

        startingPosition = transform.position;
    }

    private void Update()
    {
        UpdateTimers();
        ProcessHitReaction();
        ProcessDeath();

        if (!isDead)
        {
            DetectPlayer();
            HandleMovement();
        }
    }

    private void InitializeComponents()
    {
        currentHealth = maxHealth;
        wizardCombat = GetComponent<WizardCombat>();
        animator = GetComponent<Animator>();
        rigidBody = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = characterRenderer.material.color;
        
    }

    private void UpdateTimers()
    {
        hitTimer += Time.deltaTime;
        returnTimer += Time.deltaTime;
    }

    private void ProcessHitReaction()
    {
        if (receivedHit && hitTimer > hitReactionDelay)
        {
            receivedHit = false;
            StartCoroutine(FlashHurtEffect());
        }
    }

    private void ProcessDeath()
    {
        if (awaitingDeath && hitTimer > deathDelay)
        {
            awaitingDeath = false;
            PerformDeath();
        }
    }

    private void DetectPlayer()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        isChasing = distanceToPlayer <= detectionRange;
    }

    private void HandleMovement()
    {
        float movement = 0;
        if (!atEdge && !wizardCombat.playerIsClose)
        {
            if (isChasing && returnTimer >= returnTime)
            {
                movement = (player.position.x > transform.position.x) ? moveSpeed : -moveSpeed;
            }
            else
            {
                float distanceToStart = transform.position.x - startingPosition.x;
                if (Mathf.Abs(distanceToStart) > 0.1f)
                {
                    movement = distanceToStart > 0 ? -moveSpeed : moveSpeed;
                }
            }
        }

        rigidBody.velocity = new Vector2(movement, rigidBody.velocity.y);

        if (movement != 0)
        {
            animator.SetInteger("animState", 1);
            UpdateFacingDirection(movement);
        }
        else
        {
            animator.SetInteger("animState", 0);
        }
    }

    private void UpdateFacingDirection(float movement)
    {
        int newDirection = moveDirection;
        if (movement > 0)
            newDirection = 1;
        if (movement < 0)
            newDirection = -1;

        if (newDirection != moveDirection)
        {
            moveDirection = newDirection;
            FlipCharacter();
        }
    }

    private void FlipCharacter()
    {
        spriteRenderer.flipX = moveDirection == -1;

        foreach (Transform child in transform)
        {
            child.localPosition = new Vector3(-child.localPosition.x, child.localPosition.y, child.localPosition.z);
        }
    }

    public void EdgeDetected()
    {
        if (!atEdge)
        {
            atEdge = true;
            rigidBody.velocity = Vector2.zero;
            StartCoroutine(WaitAtEdge());
        }
    }

    private IEnumerator WaitAtEdge()
    {
        yield return new WaitForSeconds(edgeWaitTime);

        moveDirection *= -1;
        FlipCharacter();
        atEdge = false;
        returnTimer = 0;
    }

    public void TakeDamage(int damage)
    {
        if (!isDead)
        {
            currentHealth -= damage;
            ShowFloatingText(damage);
            receivedHit = true;
            hitTimer = 0f;

            if (currentHealth <= 0)
            {
                awaitingDeath = true;
                isDead = true;
            }
        }
    }

    private IEnumerator FlashHurtEffect()
    {
        characterRenderer.material.color = hurtColor;
        yield return new WaitForSeconds(flashDuration);
        characterRenderer.material.color = originalColor;
    }

    private void ShowFloatingText(int damage)
    {
        if (floatingTextPrefab != null)
        {
            GameObject textObject = Instantiate(floatingTextPrefab, transform.position + floatingTextOffset, Quaternion.identity);
            textObject.transform.GetChild(0).GetComponent<TextMesh>().text = damage.ToString();
        }
    }

    private void PerformDeath()
    {
        animator.SetTrigger("Death");
        if (player != null)
        {
            player.GetComponent<Knight>().UpdateSouls(soulReward);
        }
    }
}
