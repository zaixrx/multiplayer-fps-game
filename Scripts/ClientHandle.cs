using System.Net;
using UnityEngine;

public class ClientHandle : MonoBehaviour
{
    public static void Welcome(Packet _packet)
    {
        string _msg = _packet.ReadString();
        int _myId = _packet.ReadInt();

        Debug.Log($"Message from server: {_msg}");
        Client.instance.myId = _myId;
        ClientSend.WelcomeReceived();

        Client.instance.udp.Connect(((IPEndPoint)Client.instance.tcp.socket.Client.LocalEndPoint).Port);
    }

    public static void SpawnPlayer(Packet _packet)
    {
        int _id = _packet.ReadInt();
        string _username = _packet.ReadString();
        bool _isPlayerDead = _packet.ReadBool();
        int _currentKills = _packet.ReadInt();
        Vector3 _position = _packet.ReadVector3();
        Quaternion _rotation = _packet.ReadQuaternion();
        int _weaponType = _packet.ReadInt();
        int _magazineSize = _packet.ReadInt();

        GameManager.instance.SpawnPlayer(_id, _username, _isPlayerDead, _currentKills, _position, _rotation, _weaponType, _magazineSize);
    }

    public static void PlayerPosition(Packet _packet)
    {
        int _id = _packet.ReadInt();
        Vector3 _position = _packet.ReadVector3();

        GameManager.players[_id].transform.position = _position;
    }

    public static void PlayerDisconnected(Packet _packet) {
        int _id = _packet.ReadInt();

        Destroy(GameManager.players[_id].gameObject);
        GameManager.players.Remove(_id);
    }

    public static void PlayerHealth(Packet _packet) {
        int _id = _packet.ReadInt();
        float _playerHealth = _packet.ReadFloat();

        GameManager.players[_id].SetHealth(_playerHealth, false);
    }

    public static void PlayerRespawned(Packet _packet) {
        int _id = _packet.ReadInt();

        GameManager.players[_id].Respawn();
    }

    public static void GunShootReceived(Packet _packet) {
        int _id = _packet.ReadInt();
        Vector3 _hitPosition = _packet.ReadVector3();
        Vector3 _hitDirection = _packet.ReadVector3();
        bool _isPlayer = _packet.ReadBool();
        bool _isPlayerDead = _packet.ReadBool();

        if (GameManager.players[_id].isCurrentPlayer) {
            WeaponCrosshair.TargetCrosshairSpacing += GameManager.players[_id].currentWeapon.crosshairShootSpacing;
        }

        GameManager.players[_id].currentWeapon.ShootEffects(_hitPosition, _hitDirection, _isPlayer, _isPlayerDead);
    }

    public static void StatePayload(Packet _packet) {
        int _id = _packet.ReadInt();
        StatePayload _statePayload = _packet.ReadStatePayload();

        if (!GameManager.players.ContainsKey(_id)) return;

        if (GameManager.players[_id].TryGetComponent(out PlayerMovement playerMovement)) {
            playerMovement.OnServerMovementState(_statePayload);
        } else if (GameManager.players[_id].TryGetComponent(out OthersController othersController)) {
            othersController.OnServerMovementState(_statePayload);
        }
    }

    public static void ChangeWeapon(Packet _packet) {
        int _id  = _packet.ReadInt();
        int _weaponType = _packet.ReadInt(); 
        int _ammunition = _packet.ReadInt();
        int _magazineSize = _packet.ReadInt();

        PlayerManager player = GameManager.players[_id];

        player.ChangeWeapon(_weaponType);
        player.currentWeapon.weaponAmmunition = _ammunition;
        player.currentWeapon.weaponMagazineSize = _magazineSize;
    }

    public static void WeaponAmmunition(Packet _packet) {
        int _id = _packet.ReadInt();
        int _weaponAmmunition = _packet.ReadInt();

        GameManager.players[_id].currentWeapon.weaponAmmunition = _weaponAmmunition;
    }

    public static void WeaponReload(Packet _packet) {
        int _id = _packet.ReadInt();

        //reload
    }

    public static void PlayerKill(Packet _packet) {
        int _id = _packet.ReadInt();
        int _kills = _packet.ReadInt();

        GameManager.players[_id].currentKills = _kills;
    }
}
