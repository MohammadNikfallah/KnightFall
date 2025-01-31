using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level1_Fall : MonoBehaviour
{
    public GameObject Player;

    public GameObject respawnPoint;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log(other.gameObject.tag);
        if (other.gameObject.CompareTag("Player"))
        {
            Player.transform.position = respawnPoint.transform.position;
        }
    }
}
