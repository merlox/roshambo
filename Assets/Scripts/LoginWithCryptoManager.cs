using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using SimpleJSON;

public class LoginWithCryptoManager : MonoBehaviour
{
    public InputField mnemonicInput;
    public Button loginButton;
    public Text errorMessage;
    public Text successMessage;
    public GameObject spinner;

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

        ToggleSpinner(true);
        StartCoroutine(Login());
    }

    IEnumerator Login()
    {
        WWWForm form = new WWWForm();
        form.AddField("mnemonic", mnemonicInput.text);

        UnityWebRequest www = UnityWebRequest.Post(Static.serverUrl + "/user/login-with-crypto", form);
        yield return www.SendWebRequest();
        ToggleSpinner(false);
        string responseText = www.downloadHandler.text;
        SimpleJSON.JSONNode response;

        if (www.isNetworkError || www.isHttpError)
        {
            ShowError(www.error);
            yield break;
        }
        if (responseText == null || responseText.Length <= 0)
        {
            ShowError("Response not received from the server");
            yield break;
        }

        response = JSON.Parse(responseText);
        if (response["ok"].AsBool == false)
        {
            ShowError(response["msg"].Value);
            yield break;
        }
        Debug.Log("User id " + response["userId"].Value);
        Static.userId = response["userId"].Value;
        Static.userAddress = response["userAddress"].Value;
        Static.balance = response["balance"].AsDouble / 1e6;
        ShowSuccess(response["msg"].Value);
        yield return new WaitForSeconds(Static.timeAfterAction);
        SceneManager.LoadScene("Game");
        yield break;
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
}
