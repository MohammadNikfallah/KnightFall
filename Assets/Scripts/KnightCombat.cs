using System;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.Serialization;

public class KnightCombat : MonoBehaviour
{
    public Animator animator;
    private int _currentAttack;
    private double _timeSinceAttack;
    
    [SerializeField] public Transform attackPoint;
    [SerializeField] public float attackRange;
    [SerializeField] public LayerMask enemyLayers;
    [FormerlySerializedAs("damage")] [SerializeField] public int attackDamage = 30;
    void Update() 
    {
        _timeSinceAttack += Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.Mouse0) && _timeSinceAttack > 0.4f)
        {
            Attack();
        }
    }

    private void Attack()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        foreach (var enemy in hitEnemies)
        {
            Debug.Log("enemies got hit" + enemy.name);
            enemy.GetComponent<IDamageable>().TakeDamage(attackDamage);
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

    private void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
