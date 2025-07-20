using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Animations.Rigging;

public class PlayerManager : MonoBehaviour {
    [Header("Player Data")]
    public int id;
    public bool isDead;
    public float health;
    public float maxHealth;
    public string username;
    public int currentKills;

    [Header("Lists")]
    public List<Weapon> weapons;
    public List<Renderer> renderers;

    [Header("Others")]
    public TwoBoneIKConstraint rightIK;
    public TwoBoneIKConstraint leftIK;
    public RigBuilder rigBuilder;
    public AudioClip deathSfx;
    public Weapon currentWeapon;
    public bool isCurrentPlayer { get; private set; }

    void Start() {
        isCurrentPlayer = (id == Client.instance.myId);
        if (isCurrentPlayer) currentWeapon = weapons[0];

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void Initialize(int _id, string _username) {
        id = _id;
        username = _username;
        health = maxHealth;
    }

    public void SetHealth(float _health, bool respawn) {
        health = _health;

        if (health <= 0) Die();
    }

    public void Die() {
        isDead = true;

        foreach (Renderer renderer in renderers) {
            renderer.enabled = false;
        }

        if (!isCurrentPlayer) {
            GetComponent<PlayerRagdoll>().EnableRagdoll(true);
        } else {
            GetComponent<PlayerMenuManager>().ToggleFPSMenu(false);
        }

        SoundManager.Instance.PlaySound(deathSfx);
    }

    public void Respawn() {
        isDead = false;

        foreach (Renderer renderer in renderers) {
            renderer.enabled = true;
        }

        if (!isCurrentPlayer) {            
            GetComponent<PlayerRagdoll>().EnableRagdoll(false);
        } else {
            GetComponent<PlayerMenuManager>().ToggleFPSMenu(true);
        }

        SetHealth(maxHealth, true);
    }

    Weapon prevWeapon;
    public void ChangeWeapon(int _weapon) {
        prevWeapon = currentWeapon;

        if (prevWeapon.Equals(weapons[_weapon])) return;

        foreach(Weapon weapon in weapons) {
            if (weapon.Equals(weapons[_weapon])) continue;

            weapon.gameObject.SetActive(false);
        }


        rightIK.data.target = weapons[_weapon].rightHandHold;
        leftIK.data.target = weapons[_weapon].leftHandHold;

        rigBuilder.Build();

        if (isCurrentPlayer) currentWeapon.ChangeWeapon();

        weapons[_weapon].gameObject.SetActive(true);
        currentWeapon = weapons[_weapon];
    }
}
