using UnityEngine;
using System.Collections;

public class GunController : MonoBehaviour {

    public Transform WeponHolder;
    public Gun startingGun;
    Gun equippedGun;

    void Start(){
        if (startingGun != null){
            EquipGun(startingGun);
        }
    }

    public void EquipGun(Gun gunToEquip) {
        if (equippedGun != null){
            Destroy(equippedGun.gameObject);
        }
        equippedGun = Instantiate(gunToEquip, WeponHolder.position, WeponHolder.rotation) as Gun;
        equippedGun.transform.parent = WeponHolder;
    }

    public void Shoot() {
        if (equippedGun != null) {
            equippedGun.Shoot();
        }
    }
}
