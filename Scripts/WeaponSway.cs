using UnityEngine;

public class WeaponSway : MonoBehaviour {
    public CharacterController controller;

    public bool canSway = true;
    private const float INTERNAL_MULTIPLIER = 6.0f;
	private const float FRONT_BOB_MULTIPLIER = 100;
	private float frontBobMovementAmount = 0.0f;

	[Header("Bobbing")]
    public float bobMultiplier = 1.0f;
	[Range(0f, 0.1f)] public float maxFrontBobMovement = 0.05f;
	[HideInInspector] public float frontBobbingSpeed = 1.0f;
	[Range(0f, 0.3f)] public float sideBobbingSpeed = 0.15f;
	[Range(0f, 0.04f)] public float sideBobbingAmount = 0.02f;

	[Header("Sway")]
    public float step = 0.01f;
    public float maxStepDistance = 0.06f;
    Vector3 swayPos;

    [Header("Sway Rotation")]
    public float rotationStep = 4f;
    public float maxRotationStep = 5f;
    Vector3 swayEulerRot; 

	public float smooth = 10f;
    float smoothRot = 12f;

    private float timer = 0;
    private Vector3 initPos;
    private Vector3 initRot;

    void Start() {
        initPos = transform.localPosition;
        initRot = transform.localEulerAngles;
    }

	Vector2 lookInput;

	void Update() {
		if (!canSway) return;

        Bobbing();

	    lookInput.x = Input.GetAxis("Mouse X");
        lookInput.y = Input.GetAxis("Mouse Y");
        	
		Sway();	
		SwayRotation();

		transform.localPosition = Vector3.Lerp(transform.localPosition, swayPos + initPos, Time.deltaTime * smooth);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(swayEulerRot + initRot), Time.deltaTime * smoothRot);	
    }

	void Sway() {
        Vector3 invertLook = lookInput * -step;
        invertLook.x = Mathf.Clamp(invertLook.x, -maxStepDistance, maxStepDistance);
        invertLook.y = Mathf.Clamp(invertLook.y, -maxStepDistance, maxStepDistance);

        swayPos = invertLook;
    }

    void SwayRotation(){
        Vector2 invertLook = lookInput * -rotationStep;
        invertLook.x = Mathf.Clamp(invertLook.x, -maxRotationStep, maxRotationStep);
        invertLook.y = Mathf.Clamp(invertLook.y, -maxRotationStep, maxRotationStep);

        swayEulerRot = new Vector3(invertLook.y, invertLook.x, invertLook.x);
    }

    void Bobbing() {
        float horizontal = Input.GetAxis("Horizontal");
		float vertical = Input.GetAxis("Vertical");
		float aHorz = Mathf.Abs(horizontal);
		float aVert = Mathf.Abs(vertical);

		bool movingX = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S);
		bool movingY = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D);

		if((movingX && aVert > 0) || (movingY && aHorz > 0)) {
			float moveSpeed = Mathf.Clamp(aVert + aHorz, 0, 1) * frontBobbingSpeed * FRONT_BOB_MULTIPLIER;

			frontBobMovementAmount += moveSpeed;
		}
		else {
			frontBobMovementAmount -= frontBobbingSpeed * FRONT_BOB_MULTIPLIER;

			if(frontBobMovementAmount < 0) {
				frontBobMovementAmount = 0;
			}
		}

		frontBobMovementAmount *= Time.deltaTime;

		float xMovement = 0.0f;
		float yMovement = 0.0f;

		Vector3 calcPosition = initPos;

		if (aHorz == 0 && aVert == 0) {
			timer = 0.0f;
		}
		else {
			xMovement = Mathf.Sin(timer);
			yMovement = -Mathf.Abs(Mathf.Abs(xMovement) - 1);

			timer += sideBobbingSpeed;
			
			if (timer > Mathf.PI * 2) {
				timer = timer - (Mathf.PI * 2);
			}
		}

		float totalMovement = Mathf.Clamp(aVert + aHorz, 0, 1);

		if (xMovement != 0) {
			xMovement = xMovement * totalMovement;
			calcPosition.x = initPos.x + xMovement * sideBobbingAmount;
		}
		else {
			calcPosition.x = initPos.x;
		}

		if (yMovement != 0) {
			yMovement = yMovement * totalMovement;
			calcPosition.y = initPos.y + yMovement * sideBobbingAmount * 2;
		}
		else {
			calcPosition.y = initPos.y;
		}

		float totalFrontX = Mathf.Clamp(frontBobMovementAmount, -maxFrontBobMovement, maxFrontBobMovement);
		float totalFrontY = Mathf.Clamp(frontBobMovementAmount, -maxFrontBobMovement, maxFrontBobMovement);
		float totalFrontZ = Mathf.Clamp(frontBobMovementAmount, -maxFrontBobMovement, maxFrontBobMovement);

		calcPosition.x += totalFrontX;
		calcPosition.y -= totalFrontY;
		calcPosition.z -= totalFrontZ;

		transform.localPosition = Vector3.Lerp(transform.localPosition, calcPosition, Time.deltaTime * INTERNAL_MULTIPLIER * bobMultiplier);
    }

	public void ChangeWeapon() {
		transform.localPosition = new Vector3(0, -4, 0);
	}
}
