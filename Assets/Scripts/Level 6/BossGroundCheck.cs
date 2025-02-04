using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossGroundCheck : MonoBehaviour
{
    public bool isGrounded;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Platform"))
        {
            isGrounded = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Platform"))
        {
            isGrounded = false;
        }
    }
}
