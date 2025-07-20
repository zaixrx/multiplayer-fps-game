using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum WeaponType {
    Deagle,
    AK47,
    M16
}

public class PlayerShooting : MonoBehaviour {
    [Header("Shooting")]
    [SerializeField] private List<WeaponData> weapons;
    [SerializeField] private Transform shootOrigin;

    private int[] ammunition;
    private bool[] canShoot;

    public WeaponData currentWeapon;
    public Player player;

    void Awake() {
        player = GetComponent<Player>();
        ammunition = new int[weapons.Count];
        canShoot = new bool[weapons.Count];
    }

    void Start() {
        currentWeapon = weapons[0];

        for (int i = 0; i < weapons.Count; i++) {
            ammunition[i] = weapons[i].magazineSize;
            canShoot[i] = true;
        }
    }

    public void Shoot(Vector3 _direction) {
        if (player.isDead()) return;
        
        int cw = (int)currentWeapon.type;

        if (!canShoot[cw]) return;
        if (ammunition[cw] <= 0) {
            StartCoroutine(nameof(Reload));
            return;
        }

        RaycastHit hitInfo;

        if (Physics.Raycast(shootOrigin.position, _direction, out hitInfo, float.MaxValue) && ammunition[(int)currentWeapon.type] > 0) {
            Player _player = hitInfo.collider.GetComponent<Player>();
            if (_player != null) {
                _player.TakeDamege(currentWeapon.damegeAmount);

                if (_player.isDead()) {
                    ServerSend.PlayerKill(player, ++player.currentKills);
                }
            }
            ServerSend.GunShootReceived(player, hitInfo.point, hitInfo.normal, hitInfo.collider.GetComponent<Player>() != null);
        }
        
        ServerSend.WeaponAmmunition(player, --ammunition[cw]); 
        if (ammunition[cw] <= 0) {
            StartCoroutine(nameof(Reload));
            return;
        }

        StartCoroutine(nameof(ShootCoolDown));
    }

    private IEnumerator Reload() {
        WeaponData cwData = currentWeapon; 
        int cw = (int)currentWeapon.type;

        canShoot[cw] = false;
        ServerSend.WeaponReload(player);

        yield return new WaitForSeconds(currentWeapon.reloadTime);

        ammunition[(int)cw] = cwData.magazineSize;
        ServerSend.WeaponAmmunition(player, ammunition[(int)cw]);
        canShoot[(int)cw] = true;
    }

    private IEnumerator ShootCoolDown() {
        int cw = (int)currentWeapon.type;

        canShoot[cw] = false;
        yield return new WaitForSeconds(currentWeapon.shootCoolDown);
        canShoot[cw] = true;
    }

    public void ChangeWeapon(WeaponType _weaponType) {
        currentWeapon = weapons.Where(x => x.type == _weaponType).First();

        ServerSend.ChangeWeapon(player, (int)_weaponType, ammunition[(int)_weaponType], currentWeapon.magazineSize);
    }
}