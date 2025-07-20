using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

[RequireComponent(typeof(CharacterController), typeof(PlayerShooting))]
public class Player : MonoBehaviour {
    [Header("Player Data")]
    public int id;
    public string username;
    public int currentKills;

    [Header("Health")]
    private bool canGetDameged = true;
    public float maxHealth = 100;
    public float health;
    public Transform[] respawnPoints;

    [Header("Others")]
    public PlayerShooting playerShooting;

    private float timer;
    private int currentTick;
    private float minTimeBetweenTicks;
    private const int BUFFER_SIZE = 4096;
    private const float SERVER_TICK_RATE = Constants.TICKS_PER_SEC;

    private StatePayload[] stateBuffer;
    private Queue<InputPayload> inputQueue;

    void Awake() {
        m_Character = GetComponent<CharacterController>();
        playerShooting = GetComponent<PlayerShooting>();
    }

    void Start() {
        minTimeBetweenTicks = 1f / SERVER_TICK_RATE;

        m_Tran = transform;

        stateBuffer = new StatePayload[BUFFER_SIZE];
        inputQueue = new Queue<InputPayload>();
    }

    void Update() {
        if (isDead()) return;
    }

    void FixedUpdate() {
        if (isDead()) return;

        timer += Time.deltaTime;

        while (timer >= minTimeBetweenTicks) {
            timer -= minTimeBetweenTicks;
            HandleTick();
            currentTick++;
        }
    }

    public void Initialize(int _id, string _username) {
        id = _id;
        health = maxHealth;
        username = _username;
    }

    void HandleTick() {
        int bufferIndex = -1;
        while(inputQueue.Count > 0) {
            InputPayload inputPayload = inputQueue.Dequeue();

            bufferIndex = inputPayload.tick % BUFFER_SIZE;

            StatePayload statePayload = Movement(inputPayload);
            stateBuffer[bufferIndex] = statePayload;
        }

        if (bufferIndex != -1) {
            ServerSend.StatePayload(this, stateBuffer[bufferIndex]);
        }
    }

    public void OnClientInput(InputPayload inputPayload) {
        inputQueue.Enqueue(inputPayload);

        transform.localScale = inputPayload.scale;
    }

    public void TakeDamege(float _damege) {
        if (isDead()) return;
        if (!canGetDameged) return;

        health -= _damege;

        if (isDead()) {
            health = 0;

            m_Character.enabled = false;

            StartCoroutine(nameof(Respawn));
        }

        ServerSend.PlayerHealth(this);
    }

    private IEnumerator Respawn() {
        yield return new WaitForSeconds(5f);
        
        health = maxHealth;
        
        transform.position = respawnPoints[Random.Range(0, respawnPoints.Length)].position;
        m_Character.enabled = true;

        ServerSend.PlayerPosition(this);
        ServerSend.PlayerRespawned(this);
        StartCoroutine(nameof(RespawnCooldown));
    }

    private IEnumerator RespawnCooldown() {
        canGetDameged = false;
        yield return new WaitForSeconds(2f);
        canGetDameged = true;
    }

    public bool isDead() => health <= 0;

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

    [Header("Movement")]
    [SerializeField] private float m_MaxVelocity;
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
        float friction = (m_JumpQueued) ? 0 : 1;
        ApplyFriction(friction);

        var wishdir = new Vector3(m_MoveInput.x, 0, m_MoveInput.z);
        wishdir = m_Tran.TransformDirection(wishdir);
        wishdir.Normalize();
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
        m_Tran.localEulerAngles = payload.rotation;

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

        Vector3 cameraRotation = new Vector3(payload.cameraRotation.x, m_Tran.localEulerAngles.y, 0);

        StatePayload statePayload = new StatePayload() {
            tick = payload.tick,
            input = m_MoveInput,
            velocity = Velocity,
            position = transform.position,
            cameraRotation = cameraRotation,
            isGrounded = m_Character.isGrounded
        };

        return statePayload;
    }
    #endregion
}
