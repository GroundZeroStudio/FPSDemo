using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 武器切换
/// </summary>
public class ChooseGunController : MonoBehaviour
{
    //武器设置为集合，存放多个武器
    public List<GameObject> Weapons=new List<GameObject>();
    private int index;

    // Start is called before the first frame update
    void Start()
    {
        index = 0;
    }

    //切换武器方法（全部武器挂载人物手上，默认隐藏，，谁需要就显示显示出来）
    public void ChangeWeapon(int WeaponIndex)
    {
        for (int i = 0; i < Weapons.Count; i++)
        {
            if (WeaponIndex == i)
            {
                Weapons[i].gameObject.SetActive(true);
                if (WeaponIndex == 0)
                {

                    Weapons[i].gameObject.GetComponentInChildren<WeaponController>().UpdateAmmoUI();      
                   // print(Weapons[i].gameObject.name);
                }
                else if (WeaponIndex == 1)
                {
                    Weapons[i].gameObject.GetComponentInChildren<HandGunController>().UpdateAmmoUI();
                    //print(Weapons[i].gameObject.name);
                }
            }
            else
            {
                Weapons[i].gameObject.SetActive(false);
            }
        }

    }

  
}
