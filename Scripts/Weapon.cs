using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Weapon : MonoBehaviour {
    [Header("Gun Information")]
    public Transform gunTip;
    public List<Renderer> meshes;
    public Transform leftHandHold;
    public Transform rightHandHold;
    public WeaponRecoilData reocilData;
    public float crosshairSpacing;
    public float moveCrosshairSpacing;
    public float crosshairShootSpacing;

    public int weaponAmmunition { get; set; }
    public int weaponMagazineSize { get; set; }

    [Header("Visual Effects")]
    public GameObject bloodImpact;
    public GameObject bulletImpact;
    public TrailRenderer weaponTrail;
    public ParticleSystem muzzleParticle;

    [Header("Sound Effects")]
    public AudioClip shootSfx;

    [Header("Third Party")]
    public WeaponSway weaponSway;
    public WeaponRecoil weaponRecoil;

    void Start() {
        WeaponCrosshair.TargetCrosshairSpacing = crosshairShootSpacing;
    }

    public void ShootEffects(Vector3 hitPosition, Vector3 hitDirection, bool isPlayer, bool isPlayerDead) {        
        // Sound Effects
        SoundManager.Instance.PlaySound(shootSfx, transform.position);

        // Virtual Effects
        TrailRenderer trail = Instantiate(weaponTrail, gunTip.position, Quaternion.identity);
        StartCoroutine(SpawnTrail(trail, hitPosition, hitDirection, isPlayer, isPlayerDead));

        ParticleSystem _muzzleParticle = Instantiate(muzzleParticle, gunTip.position, Quaternion.identity, gunTip);
        Destroy(_muzzleParticle.gameObject, .5f);

        // Animations
        weaponRecoil.RecoilData = reocilData;
        weaponRecoil.Recoil();
    }

    public void ChangeWeapon() {
        weaponSway.ChangeWeapon();
    }

    IEnumerator SpawnTrail(TrailRenderer trail, Vector3 hitPosition, Vector3 hitDirection, bool isPlayer, bool isPlayerDead) {
        float time = 0;
        Vector3 startPosition = gunTip.position;
        
        while (time < 1) {
            trail.transform.position = Vector3.Lerp(startPosition, hitPosition, time);
            time += Time.deltaTime / trail.time;

            yield return null;
        }

        trail.transform.position = hitPosition;

        if (isPlayer) {
            Instantiate(bloodImpact, hitPosition, Quaternion.LookRotation(hitDirection));
        }

        if (!isPlayer) {
            Instantiate(bulletImpact, hitPosition, Quaternion.LookRotation(hitDirection));
        }

        Destroy(trail.gameObject, trail.time);
    }
}
