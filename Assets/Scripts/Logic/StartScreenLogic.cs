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

    public void StartStage(string level)
    {
        variableStorage.setCheckpoint(0);
        SceneManager.LoadScene(level);
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
