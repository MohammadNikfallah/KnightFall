using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleBanditCombat : MonoBehaviour
{
    public Animator animator;
    private int _currentAttack;
    private double _timeSinceAttack;
    
    [SerializeField] public Transform attackPoint;
    [SerializeField] public float attackRange;
    [SerializeField] public LayerMask playerLayer;
    [SerializeField] public int attackDamage = 30;
    [SerializeField] public float attackFreq = 10.0f;
    [SerializeField] public float damageDelay = 0.3f;
    void Update() 
    {
        _timeSinceAttack += Time.deltaTime;
        if (_timeSinceAttack > attackFreq)
        {
            Attack();
        }
    }

    private void Attack()
    {
        // Trigger the attack animation
        animator.SetTrigger("Attack");

        // Start delayed damage
        StartCoroutine(DelayedDamage());
        
        _timeSinceAttack = 0.0f;
    }

    private IEnumerator DelayedDamage()
    {
        // Wait for the specified delay
        yield return new WaitForSeconds(damageDelay);

        // Detect players in range of the attack
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, playerLayer);

        // Apply damage to each detected enemy
        foreach (var enemy in hitEnemies)
        {
            Debug.Log("Enemy got hit: " + enemy.name);
            enemy.GetComponent<Knight>().TakeDamage(attackDamage);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
