using UnityEngine;

public class PlayerController : MonoBehaviour {
    public WeaponSway weaponSway;
    public PlayerWeapons playerWeapons;
    public PlayerManager playerManager;
    public PlayerMovement playerMovement;
    public CameraController playerCamera;
    public CharacterController characterController;

    void Awake() {
        playerManager = GetComponent<PlayerManager>();
        playerMovement = GetComponent<PlayerMovement>();
        characterController = GetComponent<CharacterController>();
    }

    void Update() {
        weaponSway = playerManager.currentWeapon.weaponSway;
    }
}
