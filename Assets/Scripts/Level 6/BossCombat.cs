using System.Collections;
using UnityEngine;

namespace Level_6
{
    public class BossCombat : MonoBehaviour
    {
        public Animator animator;
        private int _currentAttack;
        private double _timeSinceAttack;
        public bool playerIsClose;
        public bool isAttacking = false;
        private Boss boss;
        
        [SerializeField] public Transform attackPoint;
        [SerializeField] public float attackRange;
        [SerializeField] public LayerMask playerLayer;
        [SerializeField] public int attackDamage = 30;
        // [SerializeField] public float attackFreq = 10.0f;
        [SerializeField] public float damageDelay = 0.5f;
        
        
        public float smallCooldown = 1.5f; 
        public float bigCooldown = 2f;   
        private int _attackNum = 1;
        private float currentCooldown = 0f;

        private void Start()
        {
            boss = GetComponent<Boss>();
        }

        void Update() 
        {
            if (!boss.isDead)
            {
                _timeSinceAttack += Time.deltaTime;
                detectPlayer();
                if (playerIsClose && _timeSinceAttack > currentCooldown)
                {
                    Attack();
                }
            }
        }

        private void detectPlayer()
        {
            Collider2D[] detectPlayer = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, playerLayer);

            playerIsClose = detectPlayer.Length > 0;
        }

        public void Attack()
        {
            if (_attackNum == 1)
            {
                isAttacking = true;
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
                isAttacking = false;
            }
        
            StartCoroutine(DelayedDamage());
            _timeSinceAttack = 0f;
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
                enemy.GetComponent<Knight>().TakeDamage(attackDamage);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (attackPoint != null)
                Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }

        public void PlayerDetected()
        {
            playerIsClose = true;
        }

        public void PlayerUnDetected()
        {
            playerIsClose = false;
        }
    }
}