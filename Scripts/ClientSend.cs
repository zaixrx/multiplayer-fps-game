using UnityEngine;

public class ClientSend : MonoBehaviour
{
    /// <summary>Sends a packet to the server via TCP.</summary>
    /// <param name="_packet">The packet to send to the sever.</param>
    private static void SendTCPData(Packet _packet)
    {
        _packet.WriteLength();
        Client.instance.tcp.SendData(_packet);
    }

    /// <summary>Sends a packet to the server via UDP.</summary>
    /// <param name="_packet">The packet to send to the sever.</param>
    private static void SendUDPData(Packet _packet)
    {
        _packet.WriteLength();
        Client.instance.udp.SendData(_packet);
    }

    #region Packets
    /// <summary>Lets the server know that the welcome message was received.</summary>
    public static void WelcomeReceived() {
        using (Packet _packet = new Packet((int)ClientPackets.welcomeReceived)) {
            _packet.Write(Client.instance.myId);
            _packet.Write(UIManager.instance.usernameField.text);

            SendTCPData(_packet);
        }
    }

    public static void InputPayload(InputPayload payload) {
        using (Packet _packet = new Packet((int)ClientPackets.inputPayload)) {
            _packet.Write(payload);

            SendUDPData(_packet);
        }
    }

    public static void PlayerShoot(Vector3 _direction) {
        using (Packet _packet = new Packet((int)ClientPackets.playerShoot))
        {
            _packet.Write(_direction);

            SendTCPData(_packet);
        }
    }

    public static void ChangeWeaponRequest(int _weaponType) {
        using (Packet _packet = new Packet((int)ClientPackets.changeWeaponRequest)) {
            _packet.Write(_weaponType);

            SendTCPData(_packet);
        }
    }
    #endregion
}
