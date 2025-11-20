using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Windows;

public class MainMenuButtonSelectionManager : MonoBehaviour
{
    //manages selecting buttons in the menu and pause screen
    [Header("Game Objects")]
    public Transform buttonParent;
    [Header("Variables")]
    public List<GameObject> buttons;
    [Header("Timers")]
    private float delay = .02f;
    private float readyToChange = 0f;
    public int currentSelection = 0;

    private void Awake()
    {
        foreach (Transform child in buttonParent) {
            GameObject button = child.gameObject;
            buttons.Add(button);
        }
        GameObject savedVariablesObject = GameObject.FindGameObjectWithTag("MultiSceneVariables");
        SetButtonSize(currentSelection);
    }

    public void Move(InputAction.CallbackContext context)
    {
        float change = context.ReadValue<Vector2>().x;
        if(Time.realtimeSinceStartup > readyToChange)
        {
            if(change > .25)
            {
                currentSelection += 1;
                if(currentSelection >= buttons.Count)
                {
                    currentSelection = 0;
                }
            }
            else if (change < -.25)
            {
                currentSelection -= 1;
                if (currentSelection < 0)
                {
                    currentSelection = buttons.Count - 1;
                }
            }
            readyToChange = Time.realtimeSinceStartup + delay;
            SetButtonSize(currentSelection);
        }
    }

    public void SetButtonSize(int currentSelection)
    {
        foreach (GameObject button in buttons) 
        {
            button.GetComponent<RectTransform>().localScale = Vector3.one;
        }
        buttons[currentSelection].GetComponent<RectTransform>().localScale = new Vector3(1.25f, 1.25f, 1.25f);
    }

    public void Select(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            buttons[currentSelection].GetComponent<Button>().onClick.Invoke();
        }
    }
}
