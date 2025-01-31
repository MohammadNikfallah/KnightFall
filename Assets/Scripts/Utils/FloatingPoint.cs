using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingPoint : MonoBehaviour
{
    void Start()
    {
        Destroy(gameObject, 1f);
        transform.localPosition += new Vector3(Random.Range(-0.3f, 0.3f), 0, 100);
    }
}
