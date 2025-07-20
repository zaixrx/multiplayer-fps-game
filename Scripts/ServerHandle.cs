using UnityEngine;

public class ServerHandle
{
    public static void WelcomeReceived(int _fromClient, Packet _packet)
    {
        int _clientIdCheck = _packet.ReadInt();
        string _username = _packet.ReadString();

        Debug.Log($"{Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} connected successfully and is now player {_fromClient}.");
        if (_fromClient != _clientIdCheck)
        {
            Debug.Log($"Player \"{_username}\" (ID: {_fromClient}) has assumed the wrong client ID ({_clientIdCheck})!");
        }
        Server.clients[_fromClient].SendIntoGame(_username);
    }


    public static void PlayerShoot(int _fromClient, Packet _packet) {
        Vector3 direction = _packet.ReadVector3();

        Server.clients[_fromClient].player.GetComponent<PlayerShooting>().Shoot(direction);
    }

    public static void InputPayload(int _fromClient, Packet _packet) {        
        InputPayload payload = _packet.ReadInputPayload();

        Server.clients[_fromClient].player.OnClientInput(payload);
    }

    public static void ChangeWeaponRequest(int _fromClient, Packet _packet) {
        int weaponId = _packet.ReadInt();

        Server.clients[_fromClient].player.GetComponent<PlayerShooting>().ChangeWeapon((WeaponType)weaponId);
    }
}
