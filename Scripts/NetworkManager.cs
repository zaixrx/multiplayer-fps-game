using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public int port = 26950;
    public static NetworkManager instance;

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

    void Start() {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        Server.Start(50, port);
    }

    void OnApplicationQuit() {
        Server.Stop();
    }

    public Player InstantiatePlayer()
    {
        Player player = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity).GetComponent<Player>();
        player.transform.position = player.respawnPoints[Random.Range(0, player.respawnPoints.Length)].position;
        return player;
    }
}
