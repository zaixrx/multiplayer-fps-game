using UnityEngine;

[CreateAssetMenu()]
public class WeaponData : ScriptableObject {
    public WeaponType type;
    public float shootCoolDown;
    public float damegeAmount;
    public float reloadTime;
    public int magazineSize;
}
