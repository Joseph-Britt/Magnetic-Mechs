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
    public RuntimeAnimatorController lockedLevelAnim;
    public RuntimeAnimatorController currentLevelAnim;
    public RuntimeAnimatorController beatenLevelAnim;
    [Header("Timers")]
    private float delay = .02f;
    private float readyToChange = 0f;
    public int currentSelection = 0;
    public int currentPage = 0;
    private int currentLevel = 0;

    private void Awake()
    {
        PlayerPrefs.SetInt("Level 1", 1);
        PlayerPrefs.SetInt("Level 2", 1);
        PlayerPrefs.SetInt("Level 3", 1);
        PlayerPrefs.SetInt("Level 4", 1);
        PlayerPrefs.SetInt("Level 5", 1);
        PlayerPrefs.SetInt("Level 6", 1);
        PlayerPrefs.SetInt("Level 7", 1);
        PlayerPrefs.SetInt("Level 8", 1);
        PlayerPrefs.SetInt("Level 9", 1);
        PlayerPrefs.SetInt("Level 10", 1);
        int i = 1;
        bool currentPicked = false;
        foreach (Transform child in page1Parent)
        {
            GameObject button = child.gameObject;
            page1Buttons.Add(button);
            if (PlayerPrefs.HasKey($"Level {i}") && PlayerPrefs.GetInt($"Level {i}") == 1)
            {
                button.GetComponent<Button>().interactable = true;
                button.GetComponent<Animator>().runtimeAnimatorController = beatenLevelAnim;
            }
            else
            {
                button.GetComponent<Button>().interactable = false;
                button.GetComponent<Animator>().runtimeAnimatorController = lockedLevelAnim;
            }
            if (!currentPicked && (!PlayerPrefs.HasKey($"Level {i + 1}") || PlayerPrefs.GetInt($"Level {i + 1}") != 1))
            {
                currentPicked = true;
                button.GetComponent<Animator>().runtimeAnimatorController = currentLevelAnim;
                currentLevel = i - 1;
            }
            i++;
        }

        foreach (Transform child in page2Parent)
        {
            GameObject button = child.gameObject;
            page2Buttons.Add(button);
            if (PlayerPrefs.HasKey($"Level {i}") && PlayerPrefs.GetInt($"Level {i}") == 1)
            {
                button.GetComponent<Button>().interactable = true;
                button.GetComponent<Animator>().runtimeAnimatorController = beatenLevelAnim;
            }
            else
            {
                button.GetComponent<Button>().interactable = false;
                button.GetComponent<Animator>().runtimeAnimatorController = lockedLevelAnim;
            }
            if (!currentPicked && (!PlayerPrefs.HasKey($"Level {i + 1}") || PlayerPrefs.GetInt($"Level {i + 1}") != 1))
            {
                currentPicked = true;
                button.GetComponent<Animator>().runtimeAnimatorController = currentLevelAnim;
                currentLevel = i - 1;
            }
            i++;
        }
        buttons = page1Buttons;
        GameObject savedVariablesObject = GameObject.FindGameObjectWithTag("MultiSceneVariables");
        SetButtonSize(currentSelection);
        page1Parent.gameObject.SetActive(true);
        page2Parent.gameObject.SetActive(false);
    }

    public void Move(InputAction.CallbackContext context)
    {
        float change = context.ReadValue<Vector2>().x;
        if(Time.realtimeSinceStartup > readyToChange)
        {
            if(change > .25 && (currentPage * buttons.Count) + currentSelection < currentLevel)
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
                if (currentSelection < 1 && (currentPage == 1 || (buttons.Count - 1) < currentLevel))
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
                else if (currentSelection > 0)
                {
                    currentSelection -= 1;                
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

    public void HoverButton(int hover)
    {
        Debug.Log("hover");
        currentSelection = hover;
        readyToChange = Time.realtimeSinceStartup + delay;
        SetButtonSize(currentSelection);
    }
}
