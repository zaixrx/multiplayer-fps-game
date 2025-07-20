using UnityEngine;

public struct PlayerCharacterInputs {
    public bool jump;
    public bool jumpUp;
    public bool jumpDown;
    public float vertical;
    public float horizontal;
}

public struct InputPayload {
    public int tick;
    public Vector3 scale;
    public Vector3 rotation;
    public Vector3 cameraRotation;
    public PlayerCharacterInputs characterInputs;
}

public struct StatePayload {
    public int tick;
    public Vector3 input;
    public bool isGrounded;
    public Vector3 position;
    public Vector3 velocity;
    public Vector3 cameraRotation;
}

public class PlayerMovement : MonoBehaviour {
    [HideInInspector] public PlayerManager playerManager;

    private float timer;
    private int currentTick;
    private float minTimeBetweenTicks;
    private const int SERVER_TICK_RATE = Constants.ServerTickRate;
    private const int BUFFER_SIZE = 4096;

    private StatePayload[] stateBuffer;
    private InputPayload[] inputBuffer;
    private StatePayload latestServerState;
    private StatePayload lastProcessedState;

    public bool canMove = true;
    PlayerCharacterInputs characterInputs;

    void Awake() {
        m_Character = GetComponent<CharacterController>();
        playerManager = GetComponent<PlayerManager>();
    }

    void Start() {
        minTimeBetweenTicks = 1f / SERVER_TICK_RATE;

        canMove = true;

        m_Tran = transform;
        m_CamTran = m_Camera.transform;

        stateBuffer = new StatePayload[BUFFER_SIZE];
        inputBuffer = new InputPayload[BUFFER_SIZE];
    }

    void FixedUpdate() {
        timer += Time.deltaTime;

        if (canMove) {
            characterInputs = new PlayerCharacterInputs() {
                horizontal = Input.GetAxisRaw("Horizontal"),
                vertical = Input.GetAxisRaw("Vertical"),
                jump = Input.GetKey(KeyCode.Space),
                jumpUp = Input.GetKeyUp(KeyCode.Space),
                jumpDown = Input.GetKeyDown(KeyCode.Space),
            };
        } else {
            characterInputs = new PlayerCharacterInputs() {
                horizontal = 0,
                vertical = 0,
                jump = false,
                jumpUp = false,
                jumpDown = false,
            };
        }

        while (timer >= minTimeBetweenTicks) {
            timer -= minTimeBetweenTicks;
            HandleTick();
            currentTick++;
        }
    }

    void HandleTick() {
        if (!latestServerState.Equals(default(StatePayload)) &&
            (lastProcessedState.Equals(default(StatePayload)) ||
            !latestServerState.Equals(lastProcessedState)))
        {
            HandleServerReconciliation();
        }

        int bufferIndex = currentTick % BUFFER_SIZE;

        InputPayload inputPayload = new InputPayload() {
            tick = currentTick,
            scale = transform.localScale,
            rotation = transform.localEulerAngles,
            characterInputs = characterInputs,
            cameraRotation = m_CamTran.localEulerAngles,
        };

        inputBuffer[bufferIndex] = inputPayload;

        stateBuffer[bufferIndex] = Movement(inputPayload);

        ClientSend.InputPayload(inputPayload);
    }

    public bool canReconcile = true;

    void HandleServerReconciliation() {
        if (!canReconcile) return;

        lastProcessedState = latestServerState;

        int serverStateBufferIndex = latestServerState.tick % BUFFER_SIZE;
        float positionDelta = Vector3.Distance(latestServerState.position, stateBuffer[serverStateBufferIndex].position);

        if (positionDelta > 0.001f) {
            Debug.Log("Recxw");

            transform.position = latestServerState.position;
            Velocity = latestServerState.velocity;

            stateBuffer[serverStateBufferIndex] = latestServerState;

            int tickToProcess = latestServerState.tick + 1;

            while (tickToProcess < currentTick) {
                int bufferIndex = tickToProcess % BUFFER_SIZE;

                StatePayload statePayload = Movement(inputBuffer[bufferIndex]);

                stateBuffer[bufferIndex] = statePayload;

                tickToProcess++;
            }
        }
    }

    public void OnServerMovementState(StatePayload serverState) {
        latestServerState = serverState;
    }

    #region Movement
    [System.Serializable]
    public struct MovementSettings {
        public float MaxSpeed;
        public float Acceleration;
        public float Deceleration;

        public MovementSettings(float maxSpeed, float accel, float decel) {
            MaxSpeed = maxSpeed;
            Acceleration = accel;
            Deceleration = decel;
        }
    }

    [Header("Aiming")]
    [SerializeField] private Camera m_Camera;
    

    [Header("Movement")]
    [SerializeField] private float m_MaxVelocity = 10f;
    [SerializeField] private float m_Friction = 6;
    [SerializeField] private float m_Gravity = 20;
    [SerializeField] private float m_JumpForce = 8;
    [SerializeField] private float m_AirControl = 0.3f;
    [SerializeField] private bool m_AutoBunnyHop = false;
    [SerializeField] private MovementSettings m_GroundSettings = new MovementSettings(7, 14, 10);
    [SerializeField] private MovementSettings m_AirSettings = new MovementSettings(7, 2, 2);
    [SerializeField] private MovementSettings m_StrafeSettings = new MovementSettings(1, 50, 50);

    public Vector3 Velocity {
        get => m_PlayerVelocity;
            
        set {
            m_PlayerVelocity = Vector3.zero;
            m_PlayerVelocity = value;
        }
    }

    private CharacterController m_Character;
    private Vector3 m_MoveDirectionNorm = Vector3.zero;
    private Vector3 m_PlayerVelocity = Vector3.zero;

    private bool m_JumpQueued = false;

    private float m_PlayerFriction = 0;

    private Vector3 m_MoveInput;
    private Transform m_Tran;
    private Transform m_CamTran;

    void QueueJump(PlayerCharacterInputs characterInputs) {
        if (m_AutoBunnyHop) {
            m_JumpQueued = characterInputs.jump;
            return;
        }

        if (characterInputs.jumpDown && !m_JumpQueued) {
            m_JumpQueued = true;
        }

        if (characterInputs.jumpUp) {
            m_JumpQueued = false;
        }
    }

    void AirMove() {
        float accel;

        var wishdir = m_MoveInput;
        wishdir = m_Tran.TransformDirection(wishdir);

        float wishspeed = wishdir.magnitude;
        wishspeed *= m_AirSettings.MaxSpeed;

        wishdir.Normalize();
        m_MoveDirectionNorm = wishdir;

        float wishspeed2 = wishspeed;
        if (Vector3.Dot(m_PlayerVelocity, wishdir) < 0) {
            accel = m_AirSettings.Deceleration;
        }
        else {
            accel = m_AirSettings.Acceleration;
        }

        if (m_MoveInput.z == 0 && m_MoveInput.x != 0) {
            if (wishspeed > m_StrafeSettings.MaxSpeed) {
                wishspeed = m_StrafeSettings.MaxSpeed;
            }

            accel = m_StrafeSettings.Acceleration;
        }

        Accelerate(wishdir, wishspeed, accel);
        if (m_AirControl > 0) {
            AirControl(wishdir, wishspeed2);
        }

        m_PlayerVelocity.y -= m_Gravity * minTimeBetweenTicks;
    }

    private void AirControl(Vector3 targetDir, float targetSpeed) {
        if (Mathf.Abs(m_MoveInput.z) < 0.001f || Mathf.Abs(targetSpeed) < 0.001f) {
            return;
        }

        float zSpeed = m_PlayerVelocity.y;
        m_PlayerVelocity.y = 0;
        float speed = m_PlayerVelocity.magnitude;
        m_PlayerVelocity.Normalize();

        float dot = Vector3.Dot(m_PlayerVelocity, targetDir);
        float k = 32;
        k *= m_AirControl * dot * dot * minTimeBetweenTicks;

        if (dot > 0) {
            m_PlayerVelocity.x *= speed + targetDir.x * k;
            m_PlayerVelocity.y *= speed + targetDir.y * k;
            m_PlayerVelocity.z *= speed + targetDir.z * k;

            m_PlayerVelocity.Normalize();
            m_MoveDirectionNorm = m_PlayerVelocity;
        }

        m_PlayerVelocity.x *= speed;
        m_PlayerVelocity.y = zSpeed;
        m_PlayerVelocity.z *= speed;
    }

    private void GroundMove() {        
        float friction = (m_JumpQueued) ? 0f : 1f;
        ApplyFriction(friction);

        var wishdir = new Vector3(m_MoveInput.x, 0, m_MoveInput.z);
        wishdir = m_Tran.TransformDirection(wishdir).normalized;
        m_MoveDirectionNorm = wishdir;

        var wishspeed = wishdir.magnitude * m_GroundSettings.MaxSpeed;

        Accelerate(wishdir, wishspeed, m_GroundSettings.Acceleration);

        m_PlayerVelocity.y = -m_Gravity * minTimeBetweenTicks;

        if (m_JumpQueued) {
            m_PlayerVelocity.y = m_JumpForce;
            m_JumpQueued = false;
        }
    }

    private void ApplyFriction(float t) {
        Vector3 vec = m_PlayerVelocity; 
        vec.y = 0;
        float speed = vec.magnitude;
        float drop = 0;

        if (m_Character.isGrounded) {
            float control = speed < m_GroundSettings.Deceleration ? m_GroundSettings.Deceleration : speed;
            drop = control * m_Friction * minTimeBetweenTicks * t;
        }

        float newSpeed = speed - drop;
        m_PlayerFriction = newSpeed;

        if (newSpeed < 0) {
            newSpeed = 0;
        }
        if (speed > 0) {
            newSpeed /= speed;
        }

        m_PlayerVelocity.x *= newSpeed;
        m_PlayerVelocity.z *= newSpeed;
    }

    void Accelerate(Vector3 targetDir, float targetSpeed, float accel) {
        float currentspeed = Vector3.Dot(m_PlayerVelocity, targetDir);
        float addspeed = targetSpeed - currentspeed;
        if (addspeed <= 0) {
            return;
        }

        float accelspeed = accel * minTimeBetweenTicks * targetSpeed;
        if (accelspeed > addspeed) {
            accelspeed = addspeed;
        }

        m_PlayerVelocity.x += accelspeed * targetDir.x;
        m_PlayerVelocity.z += accelspeed * targetDir.z;
    }

    StatePayload Movement(InputPayload payload) {
        m_MoveInput = new Vector3(payload.characterInputs.horizontal, 0, payload.characterInputs.vertical);
        
        QueueJump(payload.characterInputs);

        if (m_Character.isGrounded) {
            GroundMove();
        } else {
            AirMove();
        }

        if (Velocity.magnitude > m_MaxVelocity) {
            float yVelocity = Velocity.y; 

            Velocity = Velocity.normalized * m_MaxVelocity;
            Velocity = new Vector3(Velocity.x, yVelocity, Velocity.z);
        }
        
        m_Character.Move(m_PlayerVelocity * minTimeBetweenTicks);

        StatePayload statePayload = new StatePayload() {
            tick = payload.tick,
            velocity = Velocity,
            position = transform.position
        };

        return statePayload;
    }

    #endregion
}