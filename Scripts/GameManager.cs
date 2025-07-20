using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public static Dictionary<int, PlayerManager> players = new Dictionary<int, PlayerManager>();

    public GameObject localPlayerPrefab;
    public GameObject playerPrefab;

    void Awake() {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    public void SpawnPlayer(
        int _id, string _username, bool _isPlayerDead, int _currentKills,
        Vector3 _position, Quaternion _rotation,
        int _weaponType, int _magazineSize
    ) {
        GameObject _player;
        bool isPlayer = (_id == Client.instance.myId);
        if (isPlayer) {
            _player = Instantiate(localPlayerPrefab, _position, _rotation);
        }
        else {
            _player = Instantiate(playerPrefab, _position, _rotation);
        }

        PlayerManager playerManager = _player.GetComponent<PlayerManager>();
        playerManager.Initialize(_id, _username);

        players.Add(_id, _player.GetComponent<PlayerManager>());

        playerManager.ChangeWeapon(_weaponType);
        playerManager.isDead = _isPlayerDead;
        playerManager.currentKills = _currentKills;
        playerManager.currentWeapon.weaponAmmunition = _magazineSize;
        playerManager.currentWeapon.weaponMagazineSize = _magazineSize;

        if (_isPlayerDead) {
            playerManager.Die();
        }
    }
}
