using UnityEngine;

[System.Serializable]
public struct WeaponRecoilData {
    [Header("Camera Recoil")]
    public float recoilX;
    public float recoilY;
    public float recoilZ;

    public float snappiness;
    public float returnSpeed;

    [Header("Gun Recoil")]
    public float gunRecoil;

    public float gunSnappiness;
    public float gunReturnSpeed;

    [Header("Gun Reload")]
    public float reloadSpeed;
}

public class WeaponRecoil : MonoBehaviour {
    WeaponRecoilData data;
    public WeaponRecoilData RecoilData {
        get => data;
        set {
            data = value;
        }
    }

    public PlayerManager manager;

    private Vector3 currentRotation, targetRotation, currentGunRotation, targetGunRotation;

    void Update() {
        targetGunRotation = Vector3.Lerp(targetGunRotation, Vector3.zero, data.gunReturnSpeed * Time.deltaTime);
        currentGunRotation = Vector3.Slerp(currentGunRotation, targetGunRotation, data.gunSnappiness * Time.deltaTime);
        manager.currentWeapon.transform.localRotation = Quaternion.Euler(currentGunRotation);

        targetRotation = Vector3.Lerp(targetRotation, Vector3.zero, data.returnSpeed * Time.deltaTime);
        currentRotation = Vector3.Slerp(currentRotation, targetRotation, data.snappiness * Time.deltaTime);
        transform.localRotation = Quaternion.Euler(currentRotation);
    }

    public void Recoil() {
        targetGunRotation += new Vector3(data.gunRecoil, 0, 0);
        targetRotation += new Vector3(data.recoilX, Random.Range(-data.recoilY, data.recoilY), Random.Range(-data.recoilZ, data.recoilZ));
    }
}
