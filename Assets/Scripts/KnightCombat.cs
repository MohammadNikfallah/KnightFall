using System;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class KnightCombat : MonoBehaviour
{
    public Animator animator;
    private int _currentAttack;
    private double _timeSinceAttack;
    private Knight knight;
    
    [SerializeField] public Transform attackPoint;
    [SerializeField] public float attackRange;
    [SerializeField] public LayerMask enemyLayers;
    [FormerlySerializedAs("damage")] [SerializeField] public int attackDamage = 30;
    
    [SerializeField] private AudioClip swordSound;
    [SerializeField] private AudioClip swordHitSound;
    [SerializeField] public AudioSource _swordAudioSource;

    private void Awake()
    {
        knight = GetComponent<Knight>();
    }

    void Update() 
    {
        _timeSinceAttack += Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.Mouse0) && _timeSinceAttack > 0.4f && knight.CanAttack())
        {
            Attack();
        }
    }

    private void Attack()
    {
        bool t = false;
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        foreach (var enemy in hitEnemies)
        {
            var enemyScript = enemy.GetComponent<IDamageable>();
            if (enemyScript.IsAlive())
            {
                t = true;
                enemyScript.TakeDamage(attackDamage);
            }
        }

        if (t)
        {
            _swordAudioSource.clip = swordHitSound;
            _swordAudioSource.Play();
        }
        else
        {
            _swordAudioSource.clip = swordSound;
            _swordAudioSource.Play();
        }
        
        _currentAttack++;

        // Loop back to one after third attack
        if (_currentAttack > 3)
            _currentAttack = 1;

        // Reset Attack combo if time since last attack is too large
        if (_timeSinceAttack > 1.0f)
            _currentAttack = 1;

        // Call one of three attack animations "Attack1", "Attack2", "Attack3"
        animator.SetTrigger("Attack" + _currentAttack);

        // Reset timer
        _timeSinceAttack = 0.0f;
    }
    
    public void TakeDamage(int damage)
    {
        throw new NotImplementedException();
    }
    private void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
