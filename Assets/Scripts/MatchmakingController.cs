using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using SimpleJSON;
using System;

public class MatchmakingController : MonoBehaviour
{
    public RectTransform gamesContainer;
    public GameObject spinner;
    public GameObject gameItemPrefab;
    public Text noGamesAvailableText;
    public Text errorText;

    private void Start()
    {
        StartCoroutine(PostCreateGame());
        noGamesAvailableText.gameObject.SetActive(false);
    }

    IEnumerator PostCreateGame()
    {
        ShowError("");
        ToggleSpinner(true);
        WWWForm form = new WWWForm();
        UnityWebRequest www = UnityWebRequest.Get("http://localhost:80/games");
        yield return www.SendWebRequest();
        ToggleSpinner(false);
        string responseText = www.downloadHandler.text;
        SimpleJSON.JSONNode games;

        if (www.isNetworkError || www.isHttpError)
        {
            ShowError("Error getting the games");
            yield break;
        }

        if (responseText.Length > 0)
        {
            games = JSON.Parse(responseText);

            for (int i = 0; i < games.Count; i++)
            {
                GameObject gameItem = Instantiate(gameItemPrefab) as GameObject;
                // Name
                gameItem.transform.Find("Name").GetComponent<Text>().text =
                    games[i]["gameName"].Value;

                // Type
                if (games[i]["gameType"].Value == "Rounds")
                {
                    gameItem.transform.Find("Type").GetComponent<Text>().text =
                        games[i]["rounds"].Value + " rounds";
                }
                else
                {
                    gameItem.transform.Find("Type").GetComponent<Text>().text =
                        games[i]["gameType"].Value;
                }
                // Move timer
                gameItem.transform.Find("Timer").GetComponent<Text>().text =
                    games[i]["moveTimer"].Value + " seconds";
                gameItem.transform.SetParent(gamesContainer.transform, false);
            }
        }
        else
        {
            noGamesAvailableText.gameObject.SetActive(true);
            yield break;
        }
        yield break;
    }

    void ToggleSpinner(bool isDisplayed)
    {
        spinner.SetActive(isDisplayed);
    }

    void ShowError(string msg)
    {
        errorText.text = msg;
    }
}
