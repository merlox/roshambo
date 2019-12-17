using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class UserJsonResponse
{
    public bool ok;
    public string msg;
}

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

    IEnumerator PostRegisterUser()
    {
        WWWForm form = new WWWForm();
        form.AddField("email", email.text);
        form.AddField("password", password.text);
        form.AddField("username", username.text);

        using (UnityWebRequest www = UnityWebRequest.Post("http://localhost:80/user", form))
        {
            yield return www.SendWebRequest();

            ToggleSpinner(false);

            string responseText = www.downloadHandler.text;
            bool responseOk = false;
            string responseMessage = "";
            if (responseText.Length > 0)
            {
                UserJsonResponse res = JsonUtility.FromJson<UserJsonResponse>(responseText);
                responseOk = res.ok;
                responseMessage = res.msg;
                Static.email = email.text;
                Static.username = username.text;
            }
            if (www.isNetworkError || www.isHttpError)
            {
                if (responseText.Length > 0)
                {
                    ShowError(responseMessage);
                    yield break;
                }
                else
                {
                    ShowError(www.error);
                    yield break;
                }
            }
            else
            {
                ShowSuccess(responseMessage);
                yield return new WaitForSeconds(Static.timeAfterAction);
                SceneManager.LoadScene("Login");
                yield break;
            }
        }
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
            registerButton.gameObject.SetActive(false);
            spinner.SetActive(true);
        }
        else
        {
            registerButton.gameObject.SetActive(true);
            spinner.SetActive(false);
        }
    }
}
