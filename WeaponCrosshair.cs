using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct CrosshairData {
    public Image top;
    public Image right;
    public Image down;
    public Image left;
}

public class WeaponCrosshair : MonoBehaviour {
    [SerializeField] private float smoothValue;
    [SerializeField] private CrosshairData data;
    [SerializeField] private PlayerManager manager;
    [SerializeField] private CharacterController characterController;

    private float currentCrosshairSpacing;
    public static float TargetCrosshairSpacing { get; set; }

    void Update() {
        SetCrosshair();
    }

    void SetCrosshair() {
        currentCrosshairSpacing = Mathf.Lerp(
            currentCrosshairSpacing, TargetCrosshairSpacing, smoothValue * Time.deltaTime
        );

        float defaultValue = (characterController.velocity.magnitude > 1f) ? manager.currentWeapon.moveCrosshairSpacing : manager.currentWeapon.crosshairSpacing;

        TargetCrosshairSpacing = Mathf.Lerp(
            TargetCrosshairSpacing, defaultValue, smoothValue * Time.deltaTime
        );

        data.top.rectTransform.anchoredPosition = new Vector3(0, currentCrosshairSpacing, 0);
        data.down.rectTransform.anchoredPosition = new Vector3(0, -currentCrosshairSpacing, 0);
        data.right.rectTransform.anchoredPosition = new Vector3(currentCrosshairSpacing, 0, 0);
        data.left.rectTransform.anchoredPosition = new Vector3(-currentCrosshairSpacing, 0, 0);
    }
}
