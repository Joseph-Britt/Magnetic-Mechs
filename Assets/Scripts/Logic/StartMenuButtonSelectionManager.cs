using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Windows;

public class StartMenuButtonSelectionManager : MonoBehaviour
{
    //manages selecting buttons in the menu and pause screen
    [Header("Game Objects")]
    public Transform StartButtonParent;
    [Header("Variables")]
    public List<GameObject> startButtons;
    [Header("Timers")]
    private float delay = .02f;
    private float readyToChange = 0f;
    public int currentSelection = 0;
    [Header("Components")]
    public StartScreenLogic startScreenLogic;

    //private List<GameObject> buttons;
    private bool isEnabled;

    private void Awake()
    {
        foreach (Transform child in StartButtonParent)
        {
            GameObject button = child.gameObject;
            startButtons.Add(button);
        }
        GameObject savedVariablesObject = GameObject.FindGameObjectWithTag("MultiSceneVariables");
        isEnabled = true;
        SetButtonSize(currentSelection);
    }

    public void Move(InputAction.CallbackContext context)
    {
        Debug.Log("test2");
        if (!isEnabled)
        {
            return;
        }

        float change = context.ReadValue<Vector2>().x - context.ReadValue<Vector2>().y;
        if (Time.realtimeSinceStartup > readyToChange)
        {
            if (change > .25)
            {
                currentSelection += 1;
                if (currentSelection >= startButtons.Count)
                {
                    currentSelection = 0;
                }
            }
            else if (change < -.25)
            {
                currentSelection -= 1;
                if (currentSelection < 0)
                {
                    currentSelection = startButtons.Count - 1;
                }
            }
            readyToChange = Time.realtimeSinceStartup + delay;
            SetButtonSize(currentSelection);
        }
    }

    public void SetButtonSize(int currentSelection)
    {
        if (!isEnabled)
        {
            return;
        }

        foreach (GameObject button in startButtons)
        {
            button.GetComponent<RectTransform>().localScale = Vector3.one;
        }
        startButtons[currentSelection].GetComponent<RectTransform>().localScale = new Vector3(1.25f, 1.25f, 1.25f);
    }

    public void Select(InputAction.CallbackContext context)
    {
        if (!isEnabled)
        {
            return;
        }

        if (context.performed)
        {
            startButtons[currentSelection].GetComponent<Button>().onClick.Invoke();
        }
    }
    public void stopEnabling()
    {
        isEnabled = false;
    }
}
