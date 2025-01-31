using DefaultNamespace;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Level_1
{
    public class IdleBandit : MonoBehaviour, IDamageable
    {
        [SerializeField] public float      hitDelay = 0.1f;
        [SerializeField] public GameObject floatingPoint;
        [SerializeField] public Vector3 floatingPointOffset;

        private Animator _animator;
        private float _gotHitTime;
        private bool _gotHit;
    

        void Start ()
        {
            _animator = GetComponent<Animator>();
        }
	
        void Update ()
        {
            _gotHitTime += Time.deltaTime;
            
            // get hit after a delay
            if (_gotHit && _gotHitTime > hitDelay)
            {
                _gotHit = false;
                _animator.SetTrigger("Hurt");
            }
        }

        public void TakeDamage(int damage)
        {
            GameObject points = Instantiate(floatingPoint, transform.position + floatingPointOffset, Quaternion.identity) as GameObject;
            points.transform.GetChild(0).GetComponent<TextMesh>().text = damage.ToString();
            _gotHit = true;
            _gotHitTime = 0;
        }
    }
}
