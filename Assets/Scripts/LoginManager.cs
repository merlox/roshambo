using UnityEngine;
using System.Collections;
using Roshambo.Common.Models;
using UnityEngine.UI;
using System;

public class LoginManager : MonoBehaviour
{
    private UserId currentUserId;
    public Text username;
    public InputField password;
    public Button loginButton;

    // Use this for initialization
    void Start()
    {
        this.loginButton.onClick.AddListener(OnLoginClicked);
    }

    private void OnLoginClicked()
    {
        // Move to the next scene where you can select between single game and others
        this.currentUserId = new UserId();
        this.currentUserId.GUID = Guid.NewGuid();
        this.currentUserId.Name = this.username.text;
        Debug.Log("Username: " + this.username.text);
        Debug.Log("Password: " + this.password.text);
        Debug.Log("Guid :" + this.currentUserId.GUID);
    }
}
