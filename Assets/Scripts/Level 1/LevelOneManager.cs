using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelOneManager : MonoBehaviour
{
    private GameObject player;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private GameObject playerPrefab;

    private int _healthUpgrade = 20;
    private int[] _UpgradeCost = {600, 1100, 1600, 2100};
    private int _healthLevel;
    private int _damageUpgrade = 5;
    private int _damageLevel;
    
    void Start()
    {
        player = GameObject.Find("HeroKnight");
    }

    void Update()
    {
        if (Input.GetKeyUp("k"))
        {
            if (player.gameObject.GetComponent<Knight>().UpdateSouls(-1 * _UpgradeCost[_healthLevel]))
            {
                player.gameObject.GetComponent<Knight>().UpdateDamage(_damageUpgrade);
                if (_healthLevel < 3)
                    _healthLevel++;
            }
        }
        else if (Input.GetKeyUp("l"))
        {
            if (player.gameObject.GetComponent<Knight>().UpdateSouls(-1 * _UpgradeCost[_damageLevel]))
            {
                player.gameObject.GetComponent<Knight>().UpdateHealth(_healthUpgrade);
                if (_damageLevel < 3)
                    _damageLevel++;
            }
        }
    }
}
