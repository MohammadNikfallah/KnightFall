using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;

public class TrainingDummy : MonoBehaviour, IDamageable
{
    private bool receivedHit;
    private float hitTimer;
    public float hitReactionDelay = 0.1f;
    public Renderer characterRenderer;
    private Color hurtColor = Color.red;
    private float flashDuration = 0.2f;
    private Color originalColor;
    public GameObject floatingTextPrefab;
    public Vector3 floatingTextOffset;
    public float jiggleAmount = 10f;
    public float jiggleDuration = 0.2f;
    private GameObject player;

    // Start is called before the first frame   update
    void Start()
    {
        originalColor = characterRenderer.material.color;
        player = GameObject.Find("HeroKnight");
    }

    // Update is called once per frame
    void Update()
    {
        ProcessHitReaction();
    }
    
    private void ProcessHitReaction()
    {
        if (receivedHit)
        {
            receivedHit = false;
            StartCoroutine(FlashHurtEffect());
        }
    }
    
    private IEnumerator FlashHurtEffect()
    {
        characterRenderer.material.color = hurtColor;
        yield return new WaitForSeconds(flashDuration);
        characterRenderer.material.color = originalColor;
    }
    
    public void TakeDamage(int damage)
    {
        ShowFloatingText(damage);
        hitTimer = 0f;
        receivedHit = true;
        StartCoroutine(JiggleEffect(player.transform.position - gameObject.transform.position));
    }

    public bool IsAlive()
    {
        return true;
    }

    private void ShowFloatingText(int damage)
    {
        if (floatingTextPrefab != null)
        {
            GameObject textObject = Instantiate(floatingTextPrefab, transform.position + floatingTextOffset, Quaternion.identity);
            textObject.transform.GetChild(0).GetComponent<TextMesh>().text = damage.ToString();
        }
    }
    
    private IEnumerator JiggleEffect(Vector2 hitDirection)
    {
        Debug.Log(hitDirection);
        float elapsedTime = 0f;
        float jiggleDirection = hitDirection.x > 0 ? 1f : -1f; // Determine direction based on hit source

        while (elapsedTime < jiggleDuration)
        {
            float angle = Mathf.Sin(elapsedTime / jiggleDuration * Mathf.PI) * jiggleAmount * jiggleDirection;
            transform.rotation = Quaternion.Euler(0, 0, angle);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        transform.rotation = Quaternion.Euler(0, 0, 0);
    }
}
