using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartManager : MonoBehaviour
{
    public Button register;
    public Button login;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Started");
        Button reg = register.GetComponent<Button>();
        Button log = login.GetComponent<Button>();
        log.onClick.AddListener(() => {
            Debug.Log("Called 1");
            SceneManager.LoadScene("Login");
        });
        reg.onClick.AddListener(() => {
            Debug.Log("Called 2");
            SceneManager.LoadScene("Register");
        });
    }
}


