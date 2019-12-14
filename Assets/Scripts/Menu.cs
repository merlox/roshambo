using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    public GameObject background;
    public Button menuButton;
    private bool isActive = false;

    // Start is called before the first frame update
    void Start()
    {
        // Make sure the background is off by default
        ToggleActive(false);
        // When you click on the background, close the menu
        background.GetComponent<Button>().onClick.AddListener(() =>
        {
            ToggleActive(false);
        });
        menuButton.onClick.AddListener(() =>
        {
            ToggleActive(!isActive);
        });
    }

    void ToggleActive(bool on)
    {
        isActive = on;
        background.SetActive(on);
    }
}
