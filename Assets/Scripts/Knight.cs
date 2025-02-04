using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class Knight : MonoBehaviour
{
    public static Knight Instance { get; set; }

    [SerializeField] private float speed = 4.0f;
    [SerializeField] private float jumpForce = 7.5f;
    [SerializeField] private float rollForce = 6.0f;
    [SerializeField] private float edgeGrabCooldown = 0.5f;
    [SerializeField] private float respawnDelay = 2.0f;

    [Header("Combat Parameters")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float hitDelay = 0.1f;
    [SerializeField] private float dieDelay = 0.3f;
    [SerializeField] private float rollShieldTime = 0.5f;

    [Header("UI Elements")]
    private Image healthBar;
    private Image healthBarCover;
    private TextMeshProUGUI soulsText;
    private TextMeshProUGUI HealthNumber;

    [FormerlySerializedAs("maxHealh")]
    [Header("Health Parameters")]
    [SerializeField] private float maxHealth = 100.0f;
    private const float startingHealth = 100.0f;
    private float _currentHealth;

    [Header("Sounds")] 
    [SerializeField] private AudioClip[] footStepSounds;
    private int footStepIndex = 0;
    [SerializeField] public AudioSource _footstepAudioSource;
    

    private Animator _animator;
    private KnightCombat _knightCombat;
    private Rigidbody2D _body2d;
    private Sensor_Knight _groundSensor;
    private KnighEdgeSensor _edgeSensor;
    private KnighEdgeSensor _aboveEdgeSensor;
    private KnighEdgeSensor _behindEdgeSensor;

    private bool _grounded;
    private bool _rolling;
    private bool _isDead;
    private bool _rollShield;
    private bool _edgeGrabbing;
    private bool _canGrabEdge = true;
    private int _facingDirection = 1;
    private int _soulsCount = 0;

    private float _delayToIdle;
    private float _rollDuration = 0.5f;
    private float _rollTimer;
    private float _edgeTimer;
    private float _rollCurrentTime;
    
    private static readonly int Grounded = Animator.StringToHash("Grounded");
    private static readonly int AirSpeedY = Animator.StringToHash("AirSpeedY");
    private static readonly int Death = Animator.StringToHash("Death");
    private static readonly int Block = Animator.StringToHash("Block");
    private static readonly int IdleBlock = Animator.StringToHash("IdleBlock");
    private static readonly int Roll = Animator.StringToHash("Roll");
    private static readonly int JumpS = Animator.StringToHash("Jump");
    private static readonly int AnimState = Animator.StringToHash("AnimState");
    private static readonly int Hurt = Animator.StringToHash("Hurt");
    private static readonly int EdgeGrab = Animator.StringToHash("EdgeGrab");

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);

            // Initialize Components
            InitializeComponents();

            // Subscribe to scene loaded event
            SceneManager.sceneLoaded += Instance.OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
        
        if (Instance._currentHealth <= 0)
        {
            Instance._currentHealth = Instance.maxHealth;
        }
        InitializeComponents();

        updateHealthUI();
        updateSoulUI();
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        InitializeUI();
        _isDead = false;
        updateSoulUI();
        updateHealthUI();
        
        _animator.Play("Fall", 0, 0f);
        HandleAnimations();
        var spawnPoint = GameObject.Find("SpawnPoint");
        Instance.transform.position = spawnPoint.transform.position;
    }

    private void InitializeUI()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            Debug.Log("in ui initialize");
            healthBar = canvas.transform.Find("HealthBar Filled")?.GetComponent<Image>();
            healthBarCover = canvas.transform.Find("Healthbar Background")?.GetComponent<Image>();
            soulsText = canvas.transform.Find("SoulCount")?.GetComponent<TextMeshProUGUI>();
            HealthNumber = canvas.transform.Find("HealthNumber")?.GetComponent<TextMeshProUGUI>();

            if (healthBar == null) Debug.LogError("HealthBar not found in Canvas.");
            if (healthBarCover == null) Debug.LogError("HealthBarCover not found in Canvas.");
            if (soulsText == null) Debug.LogError("SoulsText not found in Canvas.");
            if (HealthNumber == null) Debug.LogError("HealthNumber not found in Canvas.");
        }
        else
        {
            Debug.LogError("Canvas not found in the scene.");
        }
    }



    private void InitializeComponents()
    {
        _animator = GetComponent<Animator>();
        _body2d = GetComponent<Rigidbody2D>();
        _knightCombat = GetComponent<KnightCombat>();
        // _audioSource = GetComponent<AudioSource>();

        _groundSensor = transform.Find("GroundSensor")?.GetComponent<Sensor_Knight>();
        _edgeSensor = transform.Find("EdgeSensor")?.GetComponent<KnighEdgeSensor>();
        _aboveEdgeSensor = transform.Find("AboveEdgeSensor")?.GetComponent<KnighEdgeSensor>();
        _behindEdgeSensor = transform.Find("BehindEdgeSensor")?.GetComponent<KnighEdgeSensor>();

        // UI elements will be initialized separately on scene load
        InitializeUI();
    }


    private void Update()
    {
        SceneChangeCheat();
        if (!_isDead)   
        {
            HandleTimers();
            HandleGroundCheck();
            HandleEdgeGrab();
            HandleMovement();
            HandleAnimations();
            CheckRespawn();
        }
    }

    private void SceneChangeCheat()
    {
        if (Input.GetKeyDown("]"))
        {
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(currentSceneIndex + 1);
        }
        if (Input.GetKeyDown("["))
        {
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(currentSceneIndex - 1);
        }
    }

    private void HandleEdgeGrab()
    {
        if (!Instance._edgeGrabbing && _edgeSensor.isGround && !_aboveEdgeSensor.isGround && !_behindEdgeSensor.isGround && Instance._canGrabEdge)
        {
            Instance._edgeGrabbing = true;
            _body2d.velocity = Vector2.zero;
            _body2d.gravityScale = 0;
            _animator.SetTrigger(EdgeGrab);
        }
        
        if (Instance._edgeGrabbing && Input.GetKeyDown(KeyCode.Space))
        {
            Instance._edgeGrabbing = false;
            Instance._edgeTimer = 0f;
            Instance._canGrabEdge = false;
            _body2d.gravityScale = 2.75f;
            _animator.SetTrigger(JumpS);
            _body2d.velocity = new Vector2(_body2d.velocity.x, jumpForce);
        }
    }

    private void HandleTimers()
    {
        Instance._rollTimer += Time.deltaTime;
        Instance._edgeTimer += Time.deltaTime;
        if (Instance._edgeTimer > edgeGrabCooldown) Instance._canGrabEdge = true;
        if (Instance._rollTimer > rollShieldTime) Instance._rollShield = false;
        if (Instance._rollTimer > _rollDuration) Instance._rolling = false;
    }

    private void HandleGroundCheck()
    {
        if (!Instance._grounded && _groundSensor.State())
        {
            Instance._grounded = true;
            _animator.SetBool(Grounded, Instance._grounded);
        }
        else if (Instance._grounded && !_groundSensor.State())
        {
            Instance._grounded = false;
            _animator.SetBool(Grounded, Instance._grounded);
        }
    }

    private void HandleMovement()
    {
        if (Instance._edgeGrabbing) return;
        float inputX = Input.GetAxis("Horizontal");

        if (inputX != 0) FlipCharacter(inputX);

        if (!Instance._rolling)
            _body2d.velocity = new Vector2(inputX * speed, _body2d.velocity.y);

        if (_body2d.velocity.x != 0 && _grounded)
        {
            if (!_footstepAudioSource.isPlaying)
            {
                _footstepAudioSource.clip = footStepSounds[footStepIndex % footStepSounds.Length];
                footStepIndex++;
                _footstepAudioSource.Play();
            }
        }
        else
        {
            _footstepAudioSource.Stop();
        }

        _animator.SetFloat(AirSpeedY, _body2d.velocity.y);
    }

    private void FlipCharacter(float direction)
    {
        bool facingRight = direction > 0;
        GetComponent<SpriteRenderer>().flipX = !facingRight;

        if ((Instance._facingDirection == 1 && !facingRight) || (Instance._facingDirection == -1 && facingRight))
        {
            Vector3 attackPos = attackPoint.localPosition;
            attackPos.x *= -1;
            attackPoint.localPosition = attackPos;
            
            Vector3 edgePos = _edgeSensor.transform.localPosition;
            edgePos.x *= -1;
            _edgeSensor.transform.localPosition = edgePos;
        
            Vector3 aboveEdgePos = _aboveEdgeSensor.transform.localPosition;
            aboveEdgePos.x *= -1;
            _aboveEdgeSensor.transform.localPosition = aboveEdgePos;
        
            Vector3 behindEdgePos = _behindEdgeSensor.transform.localPosition;
            behindEdgePos.x *= -1;
            _behindEdgeSensor.transform.localPosition = behindEdgePos;
        }

        Instance._facingDirection = facingRight ? 1 : -1;
    }

    private void HandleAnimations()
    {
        // if (Input.GetMouseButtonDown(1) && !Instance._rolling) StartBlocking();
        // else if (Input.GetMouseButtonUp(1)) StopBlocking();
        if (Input.GetKeyDown("left shift") && Instance._grounded && !Instance._rolling && !Instance._edgeGrabbing) StartRoll();
        else if (Input.GetKeyDown("space") && Instance._grounded && !Instance._rolling && !Instance._rolling) Jump();
        else if (Mathf.Abs(Input.GetAxis("Horizontal")) > Mathf.Epsilon && !Instance._edgeGrabbing) Run();
        else if(!Instance._edgeGrabbing) Idle();
    }

    private void StartBlocking()
    {
        _animator.SetTrigger(Block);
        _animator.SetBool(IdleBlock, true);
    }

    private void StopBlocking() => _animator.SetBool(IdleBlock, false);

    private void StartRoll()
    {
        Instance._rolling = true;
        Instance._rollShield = true;
        Instance._rollTimer = 0.0f;
        _animator.SetTrigger(Roll);
        _body2d.velocity = new Vector2(Instance._facingDirection * rollForce, _body2d.velocity.y);
    }

    private void Jump()
    {
        _animator.SetTrigger(JumpS);
        Instance._grounded = false;
        _animator.SetBool(Grounded, false);
        _body2d.velocity = new Vector2(_body2d.velocity.x, jumpForce);
        _groundSensor.Disable(0.2f);
    }

    private void Run()
    {
        Instance._delayToIdle = 0.05f;
        _animator.SetInteger(AnimState, 1);
    }

    private void Idle()
    {
        Instance._delayToIdle -= Time.deltaTime;
        if (Instance._delayToIdle < 0) _animator.SetInteger(AnimState, 0);
    }

    private void CheckRespawn()
    {
        if (transform.position.y < -10.0f)
            transform.position = Vector3.zero;
    }

    public void TakeDamage(int damage)
    {
        if (Instance._rollShield || _isDead) return;

        Instance._currentHealth -= damage;
        HealthNumber.text = _currentHealth.ToString();
        healthBar.fillAmount = Instance._currentHealth / Instance.maxHealth;

        if (Instance._currentHealth <= 0) Die();
        else _animator.SetTrigger(Hurt);
    }

    private void Die()
    {
        _animator.SetTrigger(Death);
        _isDead = true;
        
        StartCoroutine(RespawnAfterDeath());
    }
    
    private IEnumerator RespawnAfterDeath()
    {
        yield return new WaitForSeconds(respawnDelay);
        SceneManager.LoadScene(0);
    }

    public bool UpdateSouls(int amount)
    {
        if(soulsText == null)
            InitializeUI();
        Debug.Log("tried to update souls");
        if (Instance._soulsCount + amount >= 0)
        {
            Instance._soulsCount += amount;
            soulsText.text = Instance._soulsCount.ToString();
            return true;
        }

        return false;
    }
    
    public void SetKnightGameObject(GameObject newKnight)
    {
        this.gameObject.transform.position = newKnight.transform.position; // Optional: Sync position
    }


    public void UpdateDamage(int damage)
    {
        if(healthBar == null)
            InitializeUI();
        _knightCombat.attackDamage += damage;
    }

    public void UpdateHealth(int health)
    {
        Instance.maxHealth += health;
        Instance._currentHealth = Instance.maxHealth;
        updateHealthUI();
    }

    private void updateHealthUI()
    {
        float healthPercent = Instance.maxHealth / startingHealth;
        healthBar.rectTransform.localScale = new(healthPercent, 1f, 1f);
        healthBarCover.rectTransform.localScale = new(1f, 1f, 1f);
        
        float healthCoverSize = healthBar.rectTransform.sizeDelta.x * healthBar.rectTransform.localScale.x + 20;
        healthBarCover.rectTransform.sizeDelta = new(healthCoverSize,healthBarCover.rectTransform.sizeDelta.y); 
        
        HealthNumber.text = _currentHealth.ToString();
        healthBar.fillAmount = Instance._currentHealth / Instance.maxHealth;
        
        Debug.Log("Updated health ui : ");
        Debug.Log(healthPercent);
        Debug.Log(healthBarCover.rectTransform.sizeDelta);
        Debug.Log(healthBar.rectTransform.localScale);
        Debug.Log(healthCoverSize);
    }

    private void updateSoulUI()
    {
        soulsText.text = Instance._soulsCount.ToString();
    }

    public bool CanAttack()
    {
        return !_rolling && !_edgeGrabbing && !_isDead;
    }
}
