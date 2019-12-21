using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using SimpleJSON;
using SocketIO;
using System;

public class LoginWithCryptoManager : MonoBehaviour
{
    public InputField mnemonicInput;
    public Button loginButton;
    public Text errorMessage;
    public Text successMessage;
    public GameObject spinner;
    private SocketIOComponent socket = SocketManager.socket;

    // Start is called before the first frame update
    void Start()
    {
        loginButton.onClick.AddListener(OnLoginClicked);
    }

    private void OnLoginClicked()
    {
        // Reset the error message
        ShowError("");
        ShowSuccess("");

        // Check that the email and passwords aren't empty
        if (mnemonicInput.text.Length <= 0)
        {
            ShowError("The mnemonic can't be empty");
            return;
        }

        // Check if it's a 12-word mnemonic
        string[] words = mnemonicInput.text.Split(' ');
        if (words.Length != 12)
        {
            ShowError("The mnemonic must have 12 words separated by a single space");
            return;
        }

        LoginWithCrypto();
    }

    private void LoginWithCrypto()
    {
        ShowError("");
        ShowSuccess("");
        ToggleSpinner(true);
        Dictionary<string, string> data = new Dictionary<string, string>();
        data["mnemonic"] = mnemonicInput.text;
        socket.Emit("setup:login-with-crypto", new JSONObject(data));

        socket.On("setup:login-complete", (SocketIOEvent e) =>
        {
            string a = e.data.GetField("response").ToString();
            JSONNode res = JSON.Parse(a);

            Static.userId = res["userId"].Value;
            Static.userAddress = res["userAddress"].Value;
            Static.balance = res["balance"].AsDouble / 1e6;
            ShowSuccess(res["msg"].Value);
            ToggleSpinner(false);

            StartCoroutine(LoadScene());
        });

        socket.On("issue", (SocketIOEvent e) =>
        {
            ShowError(e.data.GetField("msg").str);
            ToggleSpinner(false);
        });
    }

    void ShowError(string msg)
    {
        this.errorMessage.text = msg;
    }

    void ShowSuccess(string msg)
    {
        this.successMessage.text = msg;
    }

    void ToggleSpinner(bool isDisplayed)
    {
        if (isDisplayed)
        {
            loginButton.gameObject.SetActive(false);
            spinner.SetActive(true);
        }
        else
        {
            loginButton.gameObject.SetActive(true);
            spinner.SetActive(false);
        }
    }

    IEnumerator LoadScene()
    {
        yield return new WaitForSeconds(Static.timeAfterAction);
        SceneManager.LoadScene("Game");
        yield break;
    }
}
