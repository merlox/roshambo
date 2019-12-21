using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using SocketIO;

public class Static : MonoBehaviour
{
    public static int timeAfterAction = 1;
    public static string userId;
    public static string userAddress;
    public static double balance;
    // public static string serverUrl = "http://3.137.51.91";
    public static string serverUrl = "http://localhost:80";

    // Game related data
    public static string roomId;
    public static string playerOne;
    public static string playerTwo;
    public static string gameName;
    public static string gameType;
    public static string rounds;
    public static string moveTimer;
}