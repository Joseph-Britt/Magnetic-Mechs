using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;


public class DifficultyScript : MonoBehaviour
{
    [Header("Components")]
    //public Text currentDifficultyText;
    public List<GameObject> difficultyButtons;
    [Header("Scripts")]
    public MultiSceneVariables multiSceneVariables;
    //[Header("Variables")]
    //private int currentSelection;
    private void Awake()
    {
        multiSceneVariables = GameObject.FindGameObjectWithTag("MultiSceneVariables").GetComponent<MultiSceneVariables>();
        foreach (Transform child in gameObject.transform)
        {
            GameObject button = child.gameObject;
            difficultyButtons.Add(button);
        }
    }
    private void FixedUpdate()
    {
        if (multiSceneVariables == null) return;
        SetButtonSize(multiSceneVariables.difficulty);
    }
    public void SetNormal()
    {
        multiSceneVariables.difficulty = 0;
    }
    public void SetHard()
    {
        multiSceneVariables.difficulty = 1;
    }
    public void SetImpossible()
    {
        multiSceneVariables.difficulty = 2;
    }
    public void SetButtonSize(int currentSelection)
    {
        foreach (GameObject button in difficultyButtons)
        {
            button.GetComponent<RectTransform>().localScale = Vector3.one;
        }
        difficultyButtons[currentSelection].GetComponent<RectTransform>().localScale = new Vector3(1.25f, 1.25f, 1.25f);
    }
}
