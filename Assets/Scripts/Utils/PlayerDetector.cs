using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;

public class PlayerDetector : MonoBehaviour
{
    private void Start()
    {
        
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log(other);
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("player is detected by enemy for attacking");
            other.gameObject.GetComponent<IPlayerDetector>().PlayerDetected();
        }
    }

    public void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("player is undetected by enemy for attacking");
            other.gameObject.GetComponent<IPlayerDetector>().PlayerUnDetected();
        }
    }
}
