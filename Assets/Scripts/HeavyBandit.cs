using UnityEngine;
using System.Collections;
using System.Diagnostics;
using DefaultNamespace;
using Debug = UnityEngine.Debug;

public class HeavyBandit : MonoBehaviour, IDamageable
{
    [SerializeField] public int maxHealth = 100;
    private int _health;
    [SerializeField] public float      m_speed = 4.0f;
    [SerializeField] public float      m_jumpForce = 7.5f;
    [SerializeField] public float      hitDelay = 0.1f;
    [SerializeField] public float      DieDelay = 0.3f;

    private Animator            m_animator;
    private Rigidbody2D         m_body2d;
    private Sensor_Bandit       m_groundSensor;
    private bool                m_grounded = false;
    private bool                m_combatIdle = false;
    private bool                m_isDead = false;
    private float _gotHitTime = 0;
    private bool _gotHit = false;
    private bool _died = false;

    // Use this for initialization
    void Start ()
    {
        _health = maxHealth;
        m_animator = GetComponent<Animator>();
        m_body2d = GetComponent<Rigidbody2D>();
        m_groundSensor = transform.Find("GroundSensor").GetComponent<Sensor_Bandit>();
    }
	
	// Update is called once per frame
	void Update ()
    {
        _gotHitTime += Time.deltaTime;
        // get hit after a delay
        if (_gotHit && _gotHitTime > hitDelay)
        {
            Debug.Log("got hit " + _gotHitTime + "  " + hitDelay);
            _gotHit = false;
            m_animator.SetTrigger("Hurt");
        }
        //die after the delay
        if (_died && _gotHitTime > DieDelay)
        {
            _gotHit = false;
            _died = false;
            Die();
        }
    }

    public void TakeDamage(int damage)
    {
        if (!_died)
        {
            _health -= damage;
            _gotHit = true;
            _gotHitTime = 0;
            if (_health <= 0)
            {
                _died = true;
            }
        }
    }

    private void Die()
    {
        m_animator.SetTrigger("Death");
    }
}
