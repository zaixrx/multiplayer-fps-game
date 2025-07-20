using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public GameObject startMenu;
    public InputField usernameField, ipField;

    void Awake()
    {
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


    /// <summary>Attempts to connect to the server.</summary>
    public void ConnectToServer() {
        string ip = ipField.text;
        string username = usernameField.text;

        if (!ValidateIPv4(ip)) return;
        if (username == "") return;

        startMenu.SetActive(false);
        usernameField.interactable = false;
        Client.instance.ConnectToServer(ip);
    }

    public bool ValidateIPv4(string ipString) {
        if (String.IsNullOrWhiteSpace(ipString)) {
            return false;
        }

        string[] splitValues = ipString.Split('.');
        if (splitValues.Length != 4) {
            return false;
        }

        byte tempForParsing;

        return splitValues.All(r => byte.TryParse(r, out tempForParsing));
    }
}
