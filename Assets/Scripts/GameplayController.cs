using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SocketIO;
using TMPro;
using UnityEngine.SceneManagement;

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
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);
        RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);

        if (Input.GetMouseButtonDown(0))
        {
            if (hit.collider != null && hit.collider.gameObject.name == "Me" && clicked != null)
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
            else if (hit.collider != null && hit.collider.gameObject.layer == LayerMask.NameToLayer("Ally-card"))
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
        socket.On("game:round:draw", (SocketIOEvent e) =>
        {
            Debug.Log("Received draw event");
            currentRound++;
            currentRoundVisual.text = currentRound.ToString();
            notification.text = "Draw";
            HideNotification();
        });

        socket.On("game:round:winner-one", (SocketIOEvent e) =>
        {
            Debug.Log("Received winner one");
            currentRound++;
            currentRoundVisual.text = currentRound.ToString();
            notification.text = "Round winner player one";
            HideNotification();
            Destroy(placedCard);
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
        });

        socket.On("game:round:winner-two", (SocketIOEvent e) =>
        {
            Debug.Log("Received winner two");
            currentRound++;
            currentRoundVisual.text = currentRound.ToString();
            notification.text = "Round winner player two";
            HideNotification();
            Destroy(placedCard);
            foreach (GameObject star in allyStars)
            {
                if (star.activeSelf)
                {
                    star.SetActive(true);
                    break;
                }
            }
            foreach (GameObject star in enemyStars)
            {
                if (!star.activeSelf)
                {
                    star.SetActive(false);
                    break;
                }
            }
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

    private IEnumerator HideNotification()
    {
        yield return new WaitForSeconds(3);
        notification.text = "";
    }
}
