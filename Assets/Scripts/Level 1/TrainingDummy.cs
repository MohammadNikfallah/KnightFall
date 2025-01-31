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

    // Start is called before the first frame   update
    void Start()
    {
        originalColor = characterRenderer.material.color;
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
            Debug.Log("in if");
            receivedHit = false;
            StartCoroutine(FlashHurtEffect());
        }
    }
    
    private IEnumerator FlashHurtEffect()
    {
        Debug.Log("in flash");
        characterRenderer.material.color = hurtColor;
        yield return new WaitForSeconds(flashDuration);
        characterRenderer.material.color = originalColor;
    }
    
    public void TakeDamage(int damage)
    {
        Debug.Log("take damage");
        ShowFloatingText(damage);
        hitTimer = 0f;
        receivedHit = true;
    }
    
    private void ShowFloatingText(int damage)
    {
        if (floatingTextPrefab != null)
        {
            GameObject textObject = Instantiate(floatingTextPrefab, transform.position + floatingTextOffset, Quaternion.identity);
            textObject.transform.GetChild(0).GetComponent<TextMesh>().text = damage.ToString();
        }
    }
}
