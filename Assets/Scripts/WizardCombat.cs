using System.Collections;
using UnityEngine;

public class WizardCombat : MonoBehaviour
{
    public Animator animator;
    public Transform attackPoint; // Center of the attack range
    
    public float attackRange = 2f; // Radius of the circle
    public LayerMask playerLayer;
    public int attackDamage = 30;
    public float attackFreq = 1.0f;
    public float damageDelay = 0.5f; // Delay before applying damage

    private float _timeSinceAttack;
    private int _attackNum = 1;
    private Wizard _wizard;

    void Start()
    {
        _wizard = GetComponent<Wizard>();
    }

    void Update()
    {
        _timeSinceAttack += Time.deltaTime;

        if (_timeSinceAttack >= attackFreq)
        {
            Attack();
            _timeSinceAttack = 0f;
        }
    }

    private void Attack()
    {
        _attackNum++;
        _attackNum = (_attackNum - 1) % 2 + 1;
        // Trigger the attack animation
        animator.SetTrigger("Attack" + _attackNum);
        
        StartCoroutine(DelayedDamage());
    }
    
    private IEnumerator DelayedDamage()
    {
        // Wait for the specified delay
        yield return new WaitForSeconds(damageDelay);

        // Determine if the character is facing right
        bool isFacingRight = _wizard != null && _wizard.isFacingRight;

        // Detect all objects in the circle
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, playerLayer);

        foreach (var enemy in hitEnemies)
        {
            // Filter enemies based on their position relative to the character's facing direction
            Vector2 directionToEnemy = enemy.transform.position - attackPoint.position;
            float dotProduct = Vector2.Dot(directionToEnemy.normalized, isFacingRight ? Vector2.right : Vector2.left);

            if (dotProduct > 0) // Only include enemies in the forward-facing half
            {
                Debug.Log("Enemy hit: " + enemy.name);
                enemy.GetComponent<Knight>().TakeDamage(attackDamage);
            }
        }
    }

    // Visualize the semicircular range
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        Gizmos.color = Color.red;

        // Draw the full circle
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);

        // Draw semicircle (forward-facing direction)
        Vector3 facingDirection = _wizard.isFacingRight ? Vector3.right : Vector3.left;
        Gizmos.DrawLine(attackPoint.position, attackPoint.position + Quaternion.Euler(0, 0, 45) * facingDirection * attackRange);
        Gizmos.DrawLine(attackPoint.position, attackPoint.position + Quaternion.Euler(0, 0, -45) * facingDirection * attackRange);
    }
}
