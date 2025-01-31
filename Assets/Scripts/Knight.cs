using UnityEngine;
using TMPro;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class Knight : MonoBehaviour
{
    public static Knight Instance { get; private set; }

    [SerializeField] private float speed = 4.0f;
    [SerializeField] private float jumpForce = 7.5f;
    [SerializeField] private float rollForce = 6.0f;
    [SerializeField] private float edgeGrabCooldown = 0.5f;

    [Header("Combat Parameters")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float hitDelay = 0.1f;
    [SerializeField] private float dieDelay = 0.3f;
    [SerializeField] private float rollShieldTime = 0.5f;

    [Header("UI Elements")]
    [SerializeField] private Image healthBar;
    [SerializeField] private TextMeshProUGUI soulsText;

    [Header("Health Parameters")]
    [SerializeField] private float maxHealth = 100.0f;
    private float _currentHealth;

    private Animator _animator;
    private KnightCombat _knightCombat;
    private Rigidbody2D _body2d;
    private Sensor_Knight _groundSensor;
    private KnighEdgeSensor _edgeSensor;
    private KnighEdgeSensor _aboveEdgeSensor;
    private KnighEdgeSensor _behindEdgeSensor;

    private bool _grounded;
    private bool _rolling;
    private bool _rollShield;
    private bool _edgeGrabbing;
    private bool _canGrabEdge = true;
    private int _facingDirection = 1;
    private int _soulsCount;

    private float _delayToIdle;
    private float _rollDuration = 8.0f / 14.0f;
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
            DontDestroyOnLoad(gameObject);

            // Initialize only once
            InitializeComponents();

            // Ensure health persists and is not reset when reloading scenes
            if (_currentHealth <= 0)
                _currentHealth = maxHealth;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeComponents()
    {
        _animator = GetComponent<Animator>();
        _body2d = GetComponent<Rigidbody2D>();
        _knightCombat = GetComponent<KnightCombat>();

        _groundSensor = transform.Find("GroundSensor").GetComponent<Sensor_Knight>();
        _edgeSensor = transform.Find("EdgeSensor").GetComponent<KnighEdgeSensor>();
        _aboveEdgeSensor = transform.Find("AboveEdgeSensor").GetComponent<KnighEdgeSensor>();
        _behindEdgeSensor = transform.Find("BehindEdgeSensor").GetComponent<KnighEdgeSensor>();
    }

    private void Update()
    {
        HandleTimers();
        HandleGroundCheck();
        HandleEdgeGrab();
        HandleMovement();
        HandleAnimations();
        CheckRespawn();
    }
    
    private void HandleEdgeGrab()
    {
        if (!_edgeGrabbing && _edgeSensor.isGround && !_aboveEdgeSensor.isGround && !_behindEdgeSensor.isGround && _canGrabEdge)
        {
            _edgeGrabbing = true;
            _body2d.velocity = Vector2.zero;
            _body2d.gravityScale = 0;
            _animator.SetTrigger(EdgeGrab);
        }
        
        if (_edgeGrabbing && Input.GetKeyDown(KeyCode.Space))
        {
            _edgeGrabbing = false;
            _edgeTimer = 0f;
            _canGrabEdge = false;
            _body2d.gravityScale = 2.75f;
            _animator.SetTrigger(JumpS);
            _body2d.velocity = new Vector2(_body2d.velocity.x, jumpForce);
        }
    }

    private void HandleTimers()
    {
        _rollTimer += Time.deltaTime;
        _edgeTimer += Time.deltaTime;
        if (_edgeTimer > edgeGrabCooldown) _canGrabEdge = true;
        if (_rollTimer > rollShieldTime) _rollShield = false;
        if (_rolling) _rollCurrentTime += Time.deltaTime;
        if (_rollCurrentTime > _rollDuration) _rolling = false;
    }

    private void HandleGroundCheck()
    {
        if (!_grounded && _groundSensor.State())
        {
            _grounded = true;
            _animator.SetBool(Grounded, _grounded);
        }
        else if (_grounded && !_groundSensor.State())
        {
            _grounded = false;
            _animator.SetBool(Grounded, _grounded);
        }
    }

    private void HandleMovement()
    {
        if (_edgeGrabbing) return;
        float inputX = Input.GetAxis("Horizontal");

        if (inputX != 0) FlipCharacter(inputX);

        if (!_rolling)
            _body2d.velocity = new Vector2(inputX * speed, _body2d.velocity.y);

        _animator.SetFloat(AirSpeedY, _body2d.velocity.y);
    }

    private void FlipCharacter(float direction)
    {
        bool facingRight = direction > 0;
        GetComponent<SpriteRenderer>().flipX = !facingRight;

        if ((_facingDirection == 1 && !facingRight) || (_facingDirection == -1 && facingRight))
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

        _facingDirection = facingRight ? 1 : -1;
    }

    private void HandleAnimations()
    {
        if (Input.GetKeyDown("e") && !_rolling) _animator.SetTrigger(Death);
        else if (Input.GetMouseButtonDown(1) && !_rolling) StartBlocking();
        else if (Input.GetMouseButtonUp(1)) StopBlocking();
        else if (Input.GetKeyDown("space") && _grounded && !_rolling) Jump();
        else if (Mathf.Abs(Input.GetAxis("Horizontal")) > Mathf.Epsilon) Run();
        else Idle();
    }

    private void StartBlocking()
    {
        _animator.SetTrigger(Block);
        _animator.SetBool(IdleBlock, true);
    }

    private void StopBlocking() => _animator.SetBool(IdleBlock, false);

    private void StartRoll()
    {
        _rolling = true;
        _rollShield = true;
        _rollTimer = 0.0f;
        _animator.SetTrigger(Roll);
        _body2d.velocity = new Vector2(_facingDirection * rollForce, _body2d.velocity.y);
    }

    private void Jump()
    {
        _animator.SetTrigger(JumpS);
        _grounded = false;
        _animator.SetBool(Grounded, false);
        _body2d.velocity = new Vector2(_body2d.velocity.x, jumpForce);
        _groundSensor.Disable(0.2f);
    }

    private void Run()
    {
        _delayToIdle = 0.05f;
        _animator.SetInteger(AnimState, 1);
    }

    private void Idle()
    {
        _delayToIdle -= Time.deltaTime;
        if (_delayToIdle < 0) _animator.SetInteger(AnimState, 0);
    }

    private void CheckRespawn()
    {
        if (transform.position.y < -10.0f)
            transform.position = Vector3.zero;
    }

    public void TakeDamage(int damage)
    {
        if (_rollShield) return;

        _currentHealth -= damage;
        healthBar.fillAmount = _currentHealth / maxHealth;

        if (_currentHealth <= 0) Die();
        else _animator.SetTrigger(Hurt);
    }

    private void Die() => _animator.SetTrigger(Death);

    public bool UpdateSouls(int amount)
    {
        if (_soulsCount + amount > 0)
        {
            _soulsCount += amount;
            soulsText.text = _soulsCount.ToString();
            return true;
        }

        return false;
    }

    public void UpdateDamage(int damage) => _knightCombat.attackDamage += damage;

    public void UpdateHealth(int health)
    {
        maxHealth += health;
        _currentHealth = maxHealth;
    }
}
