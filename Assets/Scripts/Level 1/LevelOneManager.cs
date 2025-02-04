using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelOneManager : MonoBehaviour
{
    private GameObject player;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] public GameObject healthCost;
    [SerializeField] public GameObject damageCost;


    private static int _healthUpgrade = 20;
    private static int[] _UpgradeCost = {600, 900, 1200, 1500};
    private static int _healthLevel;
    private static int _damageUpgrade = 5;
    private static int _damageLevel;
    
    void Start()
    {
        player = GameObject.Find("HeroKnight");
    }

    void Update()
    {
        if (Input.GetKeyUp("k"))
        {
            if (player.gameObject.GetComponent<Knight>().UpdateSouls(-1 * _UpgradeCost[_damageLevel]))
            {
                player.gameObject.GetComponent<Knight>().UpdateDamage(_damageUpgrade);
                if (_damageLevel < 3)
                {
                    _damageLevel++;
                    damageCost.GetComponent<TextMesh>().text = _UpgradeCost[_damageLevel].ToString();
                }
            }
        }
        else if (Input.GetKeyUp("l"))
        {
            if (player.gameObject.GetComponent<Knight>().UpdateSouls(-1 * _UpgradeCost[_healthLevel]))
            {
                player.gameObject.GetComponent<Knight>().UpdateHealth(_healthUpgrade);
                if (_healthLevel < 3)
                {
                    _healthLevel++;
                    healthCost.GetComponent<TextMesh>().text = _UpgradeCost[_healthLevel].ToString();
                }
            }
        }
    }
}
