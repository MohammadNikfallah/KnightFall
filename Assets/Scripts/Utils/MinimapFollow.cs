using System;
using UnityEngine;

public class MinimapFollow : MonoBehaviour
{
    public Transform player; // Assign the player in the Inspector
    public Vector3 offset = new Vector3(0, 10, 0); // Adjust for best view

    private void Start()
    {
        player = GameObject.Find("HeroKnight").transform;
    }

    void LateUpdate()
    {
        if (player != null)
        {
            Vector3 newPosition = player.position + offset;
            newPosition.z = transform.position.z; // Keep Z fixed
            transform.position = newPosition;
        }
    }
}