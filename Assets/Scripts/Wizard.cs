using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;

public class Wizard : MonoBehaviour, IDamageable
{
    [SerializeField] public int maxHealth = 100;
    private int _health;
    
    public Renderer characterRenderer;
    private Animator            m_animator;
    private Rigidbody2D         m_body2d;
    private Sensor_Bandit       m_groundSensor;
    
    [SerializeField] public float      hitDelay = 0.1f;
    [SerializeField] public float      DieDelay = 0.3f;
    
    private float _gotHitTime = 0;
    private bool _gotHit = false;
    private bool _died = false;
    private Color originalColor;
    public Color hurtColor = Color.red;
    public float flashDuration = 0.2f;
    public bool isFacingRight = true;
    
    void Start ()
    {
        _health = maxHealth;
        originalColor = characterRenderer.material.color; // Store the original color
        m_animator = GetComponent<Animator>();
        m_body2d = GetComponent<Rigidbody2D>();
        // m_groundSensor = transform.Find("GroundSensor").GetComponent<Sensor_Bandit>();
    }
	
    // Update is called once per frame
    void Update ()
    {
        _gotHitTime += Time.deltaTime;
        // get hit after a delay
        if (_gotHit && _gotHitTime > hitDelay)
        {
            _gotHit = false;
            StartCoroutine(FlashHurtEffect());
            // m_animator.SetTrigger("Hurt");
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
    

    private IEnumerator FlashHurtEffect()
    {
        characterRenderer.material.color = hurtColor; // Change to hurt color
        yield return new WaitForSeconds(flashDuration); // Wait
        characterRenderer.material.color = originalColor; // Revert to original color
    }

    private void Die()
    {
        m_animator.SetTrigger("Death");
    }
}
