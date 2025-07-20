using UnityEngine;

public class CameraController : MonoBehaviour {
    public float m_Sensitivity = 2f;
    public float clampValue = 80f;
    public bool canLook = true;

    public Transform playerTransform;

    Quaternion cameraRotation;
    Quaternion playerRotation;

    void Start() {
        cameraRotation = this.transform.localRotation;
        playerRotation = playerTransform.localRotation;

        canLook = true;
    }

    void Update() {
        LookRotation();
    }

    public void LookRotation() {
        if (!canLook) return;

        float yRot = Input.GetAxis("Mouse X") * m_Sensitivity;
        float xRot = Input.GetAxis("Mouse Y") * m_Sensitivity;

        playerRotation *= Quaternion.Euler(0f, yRot, 0f);
        cameraRotation *= Quaternion.Euler(-xRot, 0f, 0f);

        cameraRotation = ClampRotationAroundXAxis(cameraRotation);
        
        playerTransform.localRotation = playerRotation;
        this.transform.localRotation = cameraRotation;
    }

    Quaternion ClampRotationAroundXAxis(Quaternion q) {
        q.x /= q.w;
        q.y /= q.w;
        q.z /= q.w;
        q.w = 1.0f;

        float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);

        angleX = Mathf.Clamp(angleX, -clampValue, clampValue);

        q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

        return q;
    }
}
