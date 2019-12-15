using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

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

        using (UnityWebRequest www = UnityWebRequest.Post("http://localhost:80/user/login", form))
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
                yield return new WaitForSeconds(2);
                SceneManager.LoadScene("Game");
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
