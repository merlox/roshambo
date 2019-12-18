using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameplayController : MonoBehaviour
{
    private GameObject clicked;
    public GameObject cardPlacement;
    private Vector2 clickedInitialPosition = new Vector2(0, 0);

    // Update is called once per frame
    void Update()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);
        RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);

        if (Input.GetMouseButtonDown(0))
        {
            if (hit.collider.gameObject.name == "Me" && clicked != null)
            {
                // Place it on the active area
                clicked.transform.position = hit.collider.gameObject.transform.position;
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
                // If first click, select the active item
                Debug.Log(hit.collider.gameObject.name);
                clicked = hit.collider.gameObject;
                clickedInitialPosition = hit.collider.gameObject.transform.position;
            }
            else
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
}
