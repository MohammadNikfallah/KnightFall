using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelOneManager : MonoBehaviour
{
    [SerializeField] public GameObject player;

    private int _healthUpgrade = 20;
    private int[] _UpgradeCost = {600, 1100, 1600, 2100};
    private int _healthLevel;
    private int _damageUpgrade = 5;
    private int _damageLevel;
    
    void Start()
    {
        
    }

    void Update()
    {
        if (Input.GetKeyUp("k"))
        {
            if (player.gameObject.GetComponent<Knight>().UpdateSouls(-1 * _UpgradeCost[_healthLevel]))
            {
                player.gameObject.GetComponent<Knight>().UpdateDamage(_damageUpgrade);
                _healthLevel++;
            }
        }
        else if (Input.GetKeyUp("l"))
        {
            if (player.gameObject.GetComponent<Knight>().UpdateSouls(-1 * _UpgradeCost[_damageLevel]))
            {
                player.gameObject.GetComponent<Knight>().UpdateDamage(_healthUpgrade);
                _damageLevel++;
            }
        }
    }
}
