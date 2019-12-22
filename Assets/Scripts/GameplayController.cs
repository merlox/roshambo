using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SocketIO;
using TMPro;
using UnityEngine.SceneManagement;
using SimpleJSON;

/*
  1. Implement socket.io to join a game by clicking on the Join button for each game
  2. Be able to place cards once in the game by sending events and receiving events from
  your opponent. The game creator always starts first.

  Receive events with:
    socket.On("boop", TestBoop); // Call a method
    // Handle everything here
    socket.On("boop", (SocketIOEvent e) => {
		Debug.Log(string.Format("[name: {0}, data: {1}]", e.name, e.data));
	});

  Send events with:
    socket.Emit("user:login");

  OR
    Dictionary<string, string> data = new Dictionary<string, string>();
    data["email"] = "some@email.com";
    data["pass"] = Encrypt("1234");
    socket.Emit("user:login", new JSONObject(data));
 */
public class GameplayController : MonoBehaviour
{
    private GameObject clicked;
    public GameObject cardPlacement;
    private Vector2 clickedInitialPosition = new Vector2(0, 0);
    private SocketIOComponent socket = SocketManager.socket;
    public TextMeshProUGUI gameName;
    public TextMeshProUGUI playerOne;
    public TextMeshProUGUI playerTwo;
    public TextMeshProUGUI gameType;
    public TextMeshProUGUI currentRoundVisual;
    public TextMeshProUGUI notification;
    private int currentRound = 1;
    public GameObject[] allyStars;
    public GameObject[] enemyStars;
    private GameObject placedCard; // The card in the active section
    public GameObject playerOneCardsContainer;
    public GameObject playerTwoCardsContainer;
    private bool isPlayerOne = false;

    private void Start()
    {
        SocketEvents();
        gameName.text = Static.gameName;
        if (Static.gameType == "Rounds")
        {
            gameType.text = Static.rounds + " " + Static.gameType;
        }
        else
        {
            gameType.text = Static.gameType;
        }
        playerOne.text = Static.playerOne;
        playerTwo.text = Static.playerTwo;
        if (Static.userId == Static.playerOne) isPlayerOne = true;

        // Show cards for the right player
        foreach(Transform child in playerOneCardsContainer.transform)
        {
            child.Find("Open").gameObject.SetActive(isPlayerOne);
        }
        foreach (Transform child in playerTwoCardsContainer.transform)
        {
            child.Find("Open").gameObject.SetActive(!isPlayerOne);
        }
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);
        RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
        string cardPlacementName = isPlayerOne ? "Me" : "Enemy";
        int playerPlacementName = isPlayerOne ?
            LayerMask.NameToLayer("Ally-card") :
            LayerMask.NameToLayer("Enemy-card");

        if (Input.GetMouseButtonDown(0))
        {
            if (hit.collider != null &&
                hit.collider.gameObject.name == cardPlacementName &&
                clicked != null)
            {
                // Place it on the active area
                clicked.transform.position = hit.collider.gameObject.transform.position;
                EmitPlaced(clicked.tag);
                placedCard = clicked;
                clicked = null;
            }
            else if (hit.collider != null && clicked != null)
            {
                // If clicked the active element, return it
                clicked.transform.position = clickedInitialPosition;
                clicked = null;
            }
            else if (hit.collider != null && hit.collider.gameObject.layer == playerPlacementName)
            {
                // If first click, select the card
                clicked = hit.collider.gameObject;
                clickedInitialPosition = hit.collider.gameObject.transform.position;
            }
            else if (clicked != null)
            {
                // If clicked nothing, return it
                clicked.transform.position = clickedInitialPosition;
                clicked = null;
            }
        }
        if (clicked != null)
        {
            clicked.transform.position = mousePos2D;
        }
    }

    // When a card is placed
    private void EmitPlaced(string cardType)
    {
        Dictionary<string, string> data = new Dictionary<string, string>();
        data["roomId"] = Static.roomId;
        data["cardType"] = cardType;
        socket.Emit("game:card-placed", new JSONObject(data));
    }

    private void SocketEvents()
    {
        socket.On("game:round:draw", (SocketIOEvent res) =>
        {
            Debug.Log("Received draw event");
            currentRound++;
            currentRoundVisual.text = currentRound.ToString();
            notification.text = "Draw";
            string resText = res.data.ToString();
            JSONNode parsed = JSON.Parse(resText);
            DestroyEnemyUsedCard(placedCard, parsed);
        });

        socket.On("game:round:winner-one", (SocketIOEvent res) =>
        {
            Debug.Log("Received winner one");
            currentRound++;
            currentRoundVisual.text = currentRound.ToString();
            notification.text = "Round winner player one";
            MoveStarsRound(true);
            string resText = res.data.ToString();
            JSONNode parsed = JSON.Parse(resText);
            DestroyEnemyUsedCard(placedCard, parsed);
        });


        socket.On("game:round:winner-two", (SocketIOEvent res) =>
        {
            Debug.Log("Received winner two");
            currentRound++;
            currentRoundVisual.text = currentRound.ToString();
            notification.text = "Round winner player two";
            MoveStarsRound(false);
            string resText = res.data.ToString();
            JSONNode parsed = JSON.Parse(resText);
            DestroyEnemyUsedCard(placedCard, parsed);
        });

        socket.On("game:finish:winner-player-two", (SocketIOEvent e) =>
        {
            if (Static.userId == Static.playerOne) {
                SceneManager.LoadScene("Loss");
            }
            else
            {
                SceneManager.LoadScene("Win");
            }
        });

        socket.On("game:finish:winner-player-one", (SocketIOEvent e) =>
        {
            if (Static.userId == Static.playerOne)
            {
                SceneManager.LoadScene("Win");
            }
            else
            {
                SceneManager.LoadScene("Loss");
            }
        });

        socket.On("game:finish:draw", (SocketIOEvent e) =>
        {
            SceneManager.LoadScene("Draw");
        });
    }

    private void DestroyEnemyUsedCard(GameObject myCard, JSONNode parsed)
    {
        if (isPlayerOne)
        {
            string selectedEnemyCard = parsed["playerTwoActive"].Value;
            foreach (Transform child in playerTwoCardsContainer.transform)
            {
                if (child.tag == selectedEnemyCard)
                {
                    StartCoroutine(MoveObject(myCard, child.transform, GameObject.Find("Enemy").transform.position, .3f));
                    break;
                }
            }
        }
        else
        {
            string selectedEnemyCard = parsed["playerOneActive"].Value;
            foreach (Transform child in playerOneCardsContainer.transform)
            {
                if (child.tag == selectedEnemyCard)
                {
                    StartCoroutine(MoveObject(myCard, child.transform, GameObject.Find("Me").transform.position, .3f));
                    break;
                }
            }
        }
    }

    private void MoveStarsRound(bool isPlayerOneWinner)
    {
        if (isPlayerOneWinner)
        {
            foreach (GameObject star in allyStars)
            {
                if (!star.activeSelf)
                {
                    star.SetActive(true);
                    break;
                }
            }
            foreach (GameObject star in enemyStars)
            {
                if (star.activeSelf)
                {
                    star.SetActive(false);
                    break;
                }
            }
        }
        else
        {
            foreach (GameObject star in allyStars)
            {
                if (star.activeSelf)
                {
                    star.SetActive(false);
                    break;
                }
            }
            foreach (GameObject star in enemyStars)
            {
                if (!star.activeSelf)
                {
                    star.SetActive(true);
                    break;
                }
            }
        }
    }

    IEnumerator MoveObject(GameObject myCard, Transform item, Vector3 target, float overTime)
    {
        float startTime = Time.time;
        // Attention, the start position must be separated to avoid the lerp from
        // going too fast. Don't do item.position = Vector3.Lerp(item.position...)
        // since the start will accelerate too fast
        Vector3 startPosition = item.position;
        while (Time.time < startTime + overTime)
        {
            float pos = (Time.time - startTime) / overTime;
            item.position = Vector3.Lerp(startPosition, target, pos);
            yield return null;
        }
        item.position = target;
        item.Find("Open").gameObject.SetActive(true);
        item.Find("Background closed").gameObject.SetActive(false);
        yield return new WaitForSeconds(1);
        Destroy(item.gameObject);
        Destroy(myCard);
        notification.text = "";
    }
}
