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
    public float damageDelay = 0.5f; // Delay before applying damage

    // Define two separate cooldowns:
    public float smallCooldown = 0.5f; // cooldown between Attack1 -> Attack2
    public float bigCooldown = 1.0f;   // cooldown between Attack2 -> Attack1

    private float _timeSinceAttack;
    private float currentCooldown = 0f; // initial cooldown is 0 to allow immediate attack
    private int _attackNum = 1;          // start with attack 1
    private Wizard _wizard;

    void Start()
    {
        _wizard = GetComponent<Wizard>();
    }

    void Update()
    {
        if (!_wizard.isDead)
        {
            _timeSinceAttack += Time.deltaTime;
            detectPlayer();

            // Use currentCooldown instead of a fixed attackFreq.
            if (playerIsClose && _timeSinceAttack > currentCooldown)
            {
                Attack();
            }
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
        if (_attackNum == 1)
        {
            // Trigger Attack1 animation.
            animator.SetTrigger("Attack1");
            // After Attack1, use the small cooldown for the next attack (Attack2).
            currentCooldown = smallCooldown;
            _attackNum = 2;
        }
        else // _attackNum == 2
        {
            // Trigger Attack2 animation.
            animator.SetTrigger("Attack2");
            // After Attack2, use the bigger cooldown for the next attack (Attack1).
            currentCooldown = bigCooldown;
            _attackNum = 1;
        }
        
        StartCoroutine(DelayedDamage());
        _timeSinceAttack = 0f;
    }
    
    private IEnumerator DelayedDamage()
    {
        // Wait for the specified delay before applying damage.
        yield return new WaitForSeconds(damageDelay);

        // Detect all players in the attack area.
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, playerLayer);

        foreach (var enemy in hitEnemies)
        {
            float directionToEnemy = enemy.transform.position.x - transform.position.x;
            float dotProduct = directionToEnemy * _wizard.moveDirection;

            // Only damage the player if they are in the forward-facing half.
            if (dotProduct > 0)
            {
                enemy.GetComponent<Knight>().TakeDamage(attackDamage);
            }
        }
    }
    
    // Uncomment the following to visualize the attack range in the editor.
    /*
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        Gizmos.color = Color.red;

        // Draw the full circle.
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);

        // Draw a rough semicircle to indicate the forward-facing direction.
        Vector3 facingDirection = _wizard.moveDirection == 1 ? Vector3.right : Vector3.left;
        Gizmos.DrawLine(attackPoint.position, attackPoint.position + Quaternion.Euler(0, 0, 45) * facingDirection * attackRange);
        Gizmos.DrawLine(attackPoint.position, attackPoint.position + Quaternion.Euler(0, 0, -45) * facingDirection * attackRange);
    }
    */
}
