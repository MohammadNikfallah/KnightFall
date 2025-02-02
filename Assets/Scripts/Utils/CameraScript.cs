using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    private Transform player;
    [SerializeField] public Vector2 offset;
    void Start()
    {
        if (player == null)
        {
            GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");
            if (foundPlayer != null)
            {
                player = foundPlayer.transform;
            }
        }
    }

    void Update()
    {
        var position = player.position;
        position.z--;
        transform.position = position;
    }
}
