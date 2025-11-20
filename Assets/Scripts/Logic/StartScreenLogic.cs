using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class StartScreenLogic : MonoBehaviour
{
    //holds the logic for functions which are called into during the starting screen
    public GameObject startScreenStart;
    public GameObject startScreenLevelSelect;
    public MultiSceneVariables variableStorage;
    public PlayerInput myInput;
    public PlayerInput selectionInput;

    private void Awake()
    {
        myInput = GetComponent<PlayerInput>();
        variableStorage = GameObject.FindGameObjectWithTag("MultiSceneVariables").GetComponent<MultiSceneVariables>();
    }
    public void StartGame()
    {
        startScreenStart.SetActive(false);
        startScreenLevelSelect.SetActive(true);
    }
    public void StartStage1()
    {
        variableStorage.setCheckpoint(0);
        SceneManager.LoadScene("Level 1");
    }
    public void StartStage2()
    {
        variableStorage.setCheckpoint(0);
        SceneManager.LoadScene("Level 2");
    }
    public void StartStage3()
    {
        variableStorage.setCheckpoint(0);
        SceneManager.LoadScene("Level 3");
    }
    public void StartStage4()
    {
        variableStorage.setCheckpoint(0);
        SceneManager.LoadScene("Level 4");
    }
    public void StartStage5()
    {
        variableStorage.setCheckpoint(0);
        SceneManager.LoadScene("Level 5");
    }
    public void StartStage6()
    {
        variableStorage.setCheckpoint(0);
        SceneManager.LoadScene("Level 6");
    }
    public void StartStage7()
    {
        variableStorage.setCheckpoint(0);
        SceneManager.LoadScene("Level 7");
    }
    public void StartStage8()
    {
        variableStorage.setCheckpoint(0);
        SceneManager.LoadScene("Level 8");
    }
    public void StartStage9()
    {
        variableStorage.setCheckpoint(0);
        SceneManager.LoadScene("Level 9");
    }
    public void GamePadPressed(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            variableStorage.gamePadNotMouse = true;
            myInput.SwitchCurrentActionMap("UI");
            StartGame();
        }
    }
    public void SpacePressed(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            variableStorage.gamePadNotMouse = false;
            myInput.SwitchCurrentActionMap("UI");
            StartGame();
        }
    }
}
