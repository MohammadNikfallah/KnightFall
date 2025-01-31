using System.Collections;
using UnityEngine;

public class WizardCombat : MonoBehaviour
{
    public Animator animator;
    public Transform attackPoint; // Center of the attack range
    public bool playerIsClose;
    
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
        detectPlayer();
        if (playerIsClose && _timeSinceAttack > attackFreq)
        {
            Attack();
        }
    }
    
    private void detectPlayer()
    {
        Collider2D[] detectPlayer = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, playerLayer);
        playerIsClose = false;

        if (detectPlayer.Length > 0)
        {
            float directionToEnemy = detectPlayer[0].transform.position.x - transform.position.x;
            float dotProduct = directionToEnemy * _wizard.moveDirection;

            if (dotProduct > 0) // Only include enemies in the forward-facing half
            {
                playerIsClose = true;
            }
        }
    }

    private void Attack()
    {
        _attackNum++;
        _attackNum = (_attackNum - 1) % 2 + 1;
        // Trigger the attack animation
        animator.SetTrigger("Attack" + _attackNum);
        
        StartCoroutine(DelayedDamage());
        _timeSinceAttack = 0f;
    }
    
    private IEnumerator DelayedDamage()
    {
        // Wait for the specified delay
        yield return new WaitForSeconds(damageDelay);

        // Determine if the character is facing right
        bool isFacingRight = _wizard != null && _wizard.moveDirection == 1;

        // Detect all objects in the circle
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, playerLayer);

        foreach (var enemy in hitEnemies)
        {
            float directionToEnemy = enemy.transform.position.x - transform.position.x;
            float dotProduct = directionToEnemy * _wizard.moveDirection;

            if (dotProduct > 0) // Only include enemies in the forward-facing half
            {
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
        Vector3 facingDirection = _wizard.moveDirection == 1 ? Vector3.right : Vector3.left;
        Gizmos.DrawLine(attackPoint.position, attackPoint.position + Quaternion.Euler(0, 0, 45) * facingDirection * attackRange);
        Gizmos.DrawLine(attackPoint.position, attackPoint.position + Quaternion.Euler(0, 0, -45) * facingDirection * attackRange);
    }
}
