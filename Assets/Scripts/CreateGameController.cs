using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CreateGameController : MonoBehaviour
{
	public Button createGameButton;
	public GameObject spinner;
	public Text errorMessage;
    public Text successMessage;
    public InputField gameName;
    public Dropdown gameType;
    public InputField roundsNumber;
    public InputField moveTimer;
    public int defaultRoundNumber = 5;
    public int defaultMoveTimer = 10;

    // Start is called before the first frame update
    void Start()
    {
        ToggleSpinner(false);
        createGameButton.onClick.AddListener(CreateGame);
	}

    void CreateGame()
	{
        ShowError("");
        ShowSuccess("");

        ToggleSpinner(true);
        int selectedDropdownValue = gameType.value;
        string selectedDropdownText = gameType.options[gameType.value].text;
        string selectedRoundNumber = roundsNumber.text;
        string selectedMoveTimer = moveTimer.text;

        if (gameName.text.Length <= 0)
        {
            ShowError("The game name can't be empty");
            return;
        }
        if (selectedDropdownValue == 0)
        {
            ShowError("You must select a game type");
            return;
        }
        if (selectedRoundNumber.Length == 0)
        {
            selectedRoundNumber = defaultRoundNumber.ToString();
        }
        if (selectedMoveTimer.Length == 0)
        {
            selectedMoveTimer = defaultMoveTimer.ToString();
        }

        // Send gameName, gameType, rounds, moveTimer
        // Must be logged in to access that endpoint
        StartCoroutine(
            PostCreateGame(gameName.text, selectedDropdownText, selectedRoundNumber, selectedMoveTimer)
        );
    }

	void ShowError(string msg)
	{
		this.errorMessage.text = msg;
        Debug.Log("Error " + msg);
        ToggleSpinner(false);
    }

    void ShowSuccess(string msg)
    {
        this.successMessage.text = msg;
        ToggleSpinner(false);
    }

    void ToggleSpinner(bool isDisplayed)
	{
        createGameButton.gameObject.SetActive(!isDisplayed);
		spinner.SetActive(isDisplayed);
	}

    IEnumerator PostCreateGame(string _gameName, string _gameType, string _rounds, string _moveTimer)
    {
        Debug.Log("Called create game " + _gameName + " " + _gameType + " " + _rounds + " " + _moveTimer);
        WWWForm form = new WWWForm();
        form.AddField("gameName", _gameName);
        form.AddField("gameType", _gameType);
        form.AddField("rounds", _rounds);
        form.AddField("moveTimer", _moveTimer);

        using (UnityWebRequest www = UnityWebRequest.Post("http://localhost:80/game", form))
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
                // Give the user 2 seconds to see the success message and then redirect
                ShowSuccess(responseMessage);
                yield return new WaitForSeconds(Static.timeAfterAction);
                SceneManager.LoadScene("Matchmaking");
                yield break;
            }
        }
    }
}
