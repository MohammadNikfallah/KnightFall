using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using Level_6;
using UnityEngine;
using UnityEngine.UI;

public class Boss : MonoBehaviour, IDamageable, IEdgeDetecter
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 300;
    private float currentHealth;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2.0f;
    [SerializeField] private float jumpForce = 7.5f;

    [Header("Combat Settings")]
    [SerializeField] private float hitReactionDelay = 0.1f;
    [SerializeField] private float deathDelay = 0.3f;

    [Header("AI Settings")]
    [SerializeField] private float detectionRange = 5f;  
    [SerializeField] private float edgeWaitTime = 2f;
    [SerializeField] private float returnTime = 3f;
    private Transform player;

    private Animator animator;
    private Rigidbody2D rigidBody;
    private SpriteRenderer spriteRenderer;
    private BossCombat bossCombat;
    private Image healthBar;

    private bool isGrounded = false;
    public bool isDead;
    private bool receivedHit;
    private bool awaitingDeath;
    private float hitTimer;
    private float returnTimer;

    private bool isChasing;
    private bool atEdge;
    private Vector2 startingPosition;
    private int moveDirection = -1; // 1 = Right, -1 = Left

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
        bossCombat = GetComponent<BossCombat>();
        animator = GetComponent<Animator>();
        rigidBody = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        Canvas canvas = FindObjectOfType<Canvas>();
        healthBar = canvas.transform.Find("Boss HealthBar Filled")?.GetComponent<Image>();
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
            animator.SetTrigger("Hurt");
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
        if (!atEdge && !bossCombat.playerIsClose)
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
        
        // Apply movement
        rigidBody.velocity = new Vector2(movement, rigidBody.velocity.y);

        // Set animation state
        if (movement != 0)
        {
            animator.SetInteger("AnimState", 1);
            UpdateFacingDirection(movement);
        }
        else
        {
            animator.SetInteger("AnimState", 0);
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
        // Flip the sprite
        // transform.localScale = new Vector3(moveDirection, 1, 1);
        spriteRenderer.flipX = moveDirection == 1;

        // Flip all children to maintain relative positions
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
            rigidBody.velocity = Vector2.zero; // Stop movement
            StartCoroutine(WaitAtEdge());
        }
    }

    private IEnumerator WaitAtEdge()
    {
        yield return new WaitForSeconds(edgeWaitTime); // Wait at the edge

        // Reverse movement direction
        moveDirection *= -1;
        FlipCharacter(); // Ensure the bandit flips correctly
        atEdge = false;
        returnTimer = 0;
    }

    public void TakeDamage(int damage)
    {
        if (!isDead)
        {
            currentHealth -= damage;
            receivedHit = true;
            hitTimer = 0f;
            healthBar.fillAmount = currentHealth / maxHealth;

            if (currentHealth <= 0)
            {
                awaitingDeath = true;
                isDead = true;
            }
        }
    }

    public bool IsAlive()
    {
        return !isDead;
    }

    private void PerformDeath()
    {
        animator.SetTrigger("Death");
    }
}
