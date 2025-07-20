using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMenuManager : MonoBehaviour {
    PlayerManager playerManager;
    PlayerController playerController;

    [Header("Menu")]
    [SerializeField] private GameObject menu;
    [SerializeField] private GameObject fpsMenu;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private TextMeshProUGUI killsText;
    
    [Header("Image")]
    [SerializeField] private Image healthBar;


    void Awake() {
        playerManager = GetComponent<PlayerManager>();
        playerController = GetComponent<PlayerController>();
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) ToggleMenu();

        float health = playerManager.health;
        float maxHealth = playerManager.maxHealth;
        int currentKills = playerManager.currentKills;

        healthText.text = health.ToString();
        killsText.text = currentKills.ToString(); 
        healthBar.fillAmount = Mathf.Lerp(healthBar.fillAmount, health / maxHealth, .2f);

        ammoText.text = $"{playerManager.currentWeapon.weaponAmmunition} I {playerManager.currentWeapon.weaponMagazineSize}";
    }

    public void ToggleFPSMenu(bool value) {
        fpsMenu.SetActive(value);
    }

    bool isMenu = false;
    public void ToggleMenu() {
        isMenu = !isMenu;

        menu.SetActive(isMenu);

        Cursor.visible = isMenu;
        Cursor.lockState = (isMenu) ? CursorLockMode.None : CursorLockMode.Locked;

        playerController.playerMovement.canMove = !isMenu;
        playerController.playerCamera.canLook = !isMenu;
        playerController.playerWeapons.canShoot = !isMenu;
        playerController.weaponSway.canSway = !isMenu;
    }
}
