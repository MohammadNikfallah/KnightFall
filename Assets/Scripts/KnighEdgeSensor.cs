using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnighEdgeSensor : MonoBehaviour
{
    public bool isGround;

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Platform"))
        {
            isGround = true;
        }
    }

    public void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Platform"))
        {
            isGround = false;
        }
    }
}
