using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreateGameController : MonoBehaviour
{
	public Button createGameButton;
	public GameObject spinner;
	public Text errorMessage;
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
        // ToggleSpinner(false);
    }

	void ShowError(string msg)
	{
		this.errorMessage.text = msg;
        ToggleSpinner(false);
    }

	void ToggleSpinner(bool isDisplayed)
	{
        createGameButton.gameObject.SetActive(!isDisplayed);
		spinner.SetActive(isDisplayed);
	}
}
