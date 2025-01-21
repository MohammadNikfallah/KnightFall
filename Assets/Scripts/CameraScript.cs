using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    [SerializeField] public Transform knight;
    [SerializeField] public Vector2 offset;
    void Start()
    {
        
    }

    void Update()
    {
        var position = knight.position;
        position.z--;
        transform.position = position;
    }
}
