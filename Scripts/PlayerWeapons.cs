using UnityEngine;

public enum WeaponType {
    Deagle = 1,
    AK47,
    M16
}

[RequireComponent(typeof(PlayerManager))]
public class PlayerWeapons : MonoBehaviour {
    PlayerManager playerManager;
    public bool canShoot = true;
    public Transform gunShootingPoint;

    bool shoot;

    void Awake() {
        playerManager = GetComponent<PlayerManager>();
    }

    void Update() {
        if (playerManager.isDead) return;

        for (int i = 0; i < 3; i++) {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i)) {
                ClientSend.ChangeWeaponRequest(i);
            }
        }

        shoot = (playerManager.currentWeapon.Equals(WeaponType.Deagle)) ? Input.GetMouseButtonDown(0) : Input.GetMouseButton(0); 

        if (shoot && canShoot) {
            ClientSend.PlayerShoot(gunShootingPoint.forward);
        }
    }
}
