using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using SimpleJSON;

public class LoginManager : MonoBehaviour
{
    public InputField email;
    public InputField password;
    public Button loginButton;
    public Text errorMessage;
    public Text successMessage;
    public GameObject spinner;

    // Use this for initialization
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
        if (email.text.Length <= 0)
        {
            ShowError("The email can't be empty");
            return;
        }
        if (password.text.Length <= 0)
        {
            ShowError("The password can't be empty");
            return;
        }

        ToggleSpinner(true);
        StartCoroutine(Login());
    }

    IEnumerator Login()
    {
        WWWForm form = new WWWForm();
        form.AddField("email", email.text);
        form.AddField("password", password.text);

        UnityWebRequest www = UnityWebRequest.Post(Static.serverUrl + "/user/login", form);
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
        loginButton.gameObject.SetActive(!isDisplayed);
        spinner.SetActive(isDisplayed);
    }
}
