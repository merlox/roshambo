using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using SimpleJSON;

public class RegisterManager : MonoBehaviour
{
    public InputField email;
    public InputField password;
    public InputField passwordRepeat;
    public InputField username;
    public Button registerButton;
    public Text errorMessage;
    public Text successMessage;
    public GameObject spinner;

    // Start is called before the first frame update
    void Start()
    {
        registerButton.onClick.AddListener(RegisterUser);
        ToggleSpinner(false); // Make sure the spinner is hidden when starting the scene
    }

    void RegisterUser()
    {
        // Reset the error message
        ShowError("");
        ShowSuccess("");
        string regexEmail =
            @"^(([\w-]+\.)+[\w-]+|([a-zA-Z]{1}|[\w-]{2,}))@"
            + @"((([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\."
              + @"([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])){1}|"
            + @"([a-zA-Z]+[\w-]+\.)+[a-zA-Z]{2,4})$";
        if (email.text.Length <= 0)
        {
            ShowError("The email can't be empty");
            return;
        }
        if (password.text.Length <= 0 || passwordRepeat.text.Length <= 0)
        {
            ShowError("The password can't be empty");
            return;
        }
        if (username.text.Length <= 0)
        {
            ShowError("The username can't be empty");
            return;
        }
        if (!Regex.IsMatch(email.text, regexEmail))
        {
            ShowError("The email isn't valid");
            return;
        }
        if (password.text != passwordRepeat.text)
        {
            ShowError("The passwords don't match");
            return;
        }

        ToggleSpinner(true);
        StartCoroutine(PostRegisterUser());
    }

    // To register a new user
    IEnumerator PostRegisterUser()
    {
        WWWForm form = new WWWForm();
        form.AddField("email", email.text);
        form.AddField("password", password.text);
        form.AddField("username", username.text);

        UnityWebRequest www = UnityWebRequest.Post(Static.serverUrl + "/user", form);
        yield return www.SendWebRequest();
        ToggleSpinner(false);
        string responseText = www.downloadHandler.text;
        JSONNode response;

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
        Debug.Log("userId " + response["userId"].Value);
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
        registerButton.gameObject.SetActive(!isDisplayed);
        spinner.SetActive(isDisplayed);
    }
}
