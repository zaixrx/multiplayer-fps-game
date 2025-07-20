using UnityEngine;

public class ServerSend {
    private static void SendTCPData(int _toClient, Packet _packet)
    {
        _packet.WriteLength();
        Server.clients[_toClient].tcp.SendData(_packet);
    }

    private static void SendUDPData(int _toClient, Packet _packet)
    {
        _packet.WriteLength();
        Server.clients[_toClient].udp.SendData(_packet);
    }

    private static void SendTCPDataToAll(Packet _packet)
    {
        _packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            Server.clients[i].tcp.SendData(_packet);
        }
    }

    private static void SendTCPDataToAll(int _exceptClient, Packet _packet)
    {
        _packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            if (i != _exceptClient)
            {
                Server.clients[i].tcp.SendData(_packet);
            }
        }
    }

    private static void SendUDPDataToAll(Packet _packet)
    {
        _packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            Server.clients[i].udp.SendData(_packet);
        }
    }

    private static void SendUDPDataToAll(int _exceptClient, Packet _packet)
    {
        _packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            if (i != _exceptClient)
            {
                Server.clients[i].udp.SendData(_packet);
            }
        }
    }

    #region Packets
    public static void Welcome(int _toClient, string _msg)
    {
        using (Packet _packet = new Packet((int)ServerPackets.welcome))
        {
            _packet.Write(_msg);
            _packet.Write(_toClient);

            SendTCPData(_toClient, _packet);
        }
    }

    public static void SpawnPlayer(int _toClient, Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.spawnPlayer))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.username);
            _packet.Write(_player.isDead());
            _packet.Write(_player.currentKills);
            _packet.Write(_player.transform.position);
            _packet.Write(_player.transform.rotation);
            _packet.Write((int)_player.playerShooting.currentWeapon.type);
            _packet.Write(_player.playerShooting.currentWeapon.magazineSize);

            SendTCPData(_toClient, _packet);
        }
    }

    public static void PlayerPosition(Player _player) {
        using (Packet _packet = new Packet((int)ServerPackets.playerPosition)) {
            _packet.Write(_player.id);
            _packet.Write(_player.transform.position);

            SendUDPDataToAll(_packet);
        }
    }

    public static void PlayerDisconnected(int _playerID) {
        using (Packet _packet = new Packet((int)ServerPackets.playerDisconnected))
        {
            _packet.Write(_playerID);

            SendTCPDataToAll(_packet);
        }
    }

    public static void PlayerHealth(Player _player) {
        using (Packet _packet = new Packet((int)ServerPackets.playerHealth)) {
            _packet.Write(_player.id);
            _packet.Write(_player.health);

            SendTCPDataToAll(_packet);
        }
    }

    public static void PlayerRespawned(Player _player) {
        using (Packet _packet = new Packet((int)ServerPackets.playerRespawned)) {
            _packet.Write(_player.id);

            SendTCPDataToAll(_packet);
        }
    }

    public static void GunShootReceived(Player _player, Vector3 _position, Vector3 _direction, bool _isPlayer) {
        using (Packet _packet = new Packet((int)ServerPackets.gunShootReceived)) {
            _packet.Write(_player.id);
            _packet.Write(_position);
            _packet.Write(_direction);
            _packet.Write(_isPlayer);
            _packet.Write(_player.isDead());

            SendTCPDataToAll(_packet);
        }
    }

    public static void StatePayload(Player _player, StatePayload _statePayload) {
        using (Packet _packet = new Packet((int)ServerPackets.statePayload)) {
            _packet.Write(_player.id);
            _packet.Write(_statePayload);

            SendUDPDataToAll(_packet);
        }
    }

    public static void ChangeWeapon(Player _player, int _weaponType, int _ammunition, int _magazineSize) {
        using (Packet _packet = new Packet((int)ServerPackets.changeWeapon)) {
            _packet.Write(_player.id);
            _packet.Write(_weaponType);
            _packet.Write(_ammunition);
            _packet.Write(_magazineSize);

            SendTCPDataToAll(_packet);
        }
    }

    public static void WeaponAmmunition(Player _player, int _ammunition) {
        using (Packet _packet = new Packet((int)ServerPackets.weaponAmmunition)) {
            _packet.Write(_player.id);
            _packet.Write(_ammunition);

            SendTCPDataToAll(_packet);
        }
    }

    public static void WeaponReload(Player _player) {
        using (Packet _packet = new Packet((int)ServerPackets.weaponReload)) {
            _packet.Write(_player.id);

            SendTCPData(_player.id, _packet);
        }
    }

    public static void PlayerKill(Player _player, int kills) {
        using (Packet _packet = new Packet((int)ServerPackets.playerKill)) {
            _packet.Write(_player.id);
            _packet.Write(kills);

            SendTCPDataToAll(_packet);
        }
    } 
    #endregion
}
