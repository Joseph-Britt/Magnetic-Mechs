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
    public Transform page1Parent;
    public Transform page2Parent;
    [Header("Variables")]
    public List<GameObject> buttons;
    public List<GameObject> page1Buttons;
    public List<GameObject> page2Buttons;
    [Header("Timers")]
    private float delay = .02f;
    private float readyToChange = 0f;
    public int currentSelection = 0;
    public int currentPage = 0;

    private void Awake()
    {
        foreach (Transform child in page1Parent)
        {
            GameObject button = child.gameObject;
            page1Buttons.Add(button);
        }
        foreach (Transform child in page2Parent)
        {
            GameObject button = child.gameObject;
            page2Buttons.Add(button);
        }
        buttons = page1Buttons;
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
                    if (currentPage == 0)
                    {
                        currentPage = 1;
                        buttons = page2Buttons;
                        page1Parent.gameObject.SetActive(false);
                        page2Parent.gameObject.SetActive(true);
                    }
                    currentSelection = 0;
                }
            }
            else if (change < -.25)
            {
                currentSelection -= 1;
                if (currentSelection < 0)
                {
                    if (currentPage == 1)
                    {
                        currentPage = 0;
                        buttons = page1Buttons;
                        page1Parent.gameObject.SetActive(true);
                        page2Parent.gameObject.SetActive(false);
                    }
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
