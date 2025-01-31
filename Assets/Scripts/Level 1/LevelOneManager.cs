using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelOneManager : MonoBehaviour
{
    [SerializeField] public GameObject player;

    private int _healthUpgrade = 20;
    private int[] _UpgradeCost = {600, 1100, 1600, 2100};
    private int _healthLevel = 0;
    private int _damageUpgrade = 5;
    private int _damageLevel = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp("k"))
        {
            player.gameObject.GetComponent<Knight>().UpdateDamage(_damageUpgrade);
            player.gameObject.GetComponent<Knight>().UpdateSouls(-1 * _UpgradeCost[_healthLevel]);
            _healthLevel++;
        }
        else if (Input.GetKeyUp("l"))
        {
            player.gameObject.GetComponent<Knight>().UpdateDamage(_healthUpgrade);
            player.gameObject.GetComponent<Knight>().UpdateSouls(-1 * _UpgradeCost[_damageLevel]);
            _damageLevel++;
        }
    }
}
