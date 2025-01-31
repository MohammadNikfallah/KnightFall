using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;

public class EdgeDetection : MonoBehaviour
{
    private GameObject _character;


    public void Start()
    {
        _character = transform.parent.gameObject;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Platform"))
        {
            _character.GetComponent<IEdgeDetecter>().EdgeDetected();
        }
    }
}
