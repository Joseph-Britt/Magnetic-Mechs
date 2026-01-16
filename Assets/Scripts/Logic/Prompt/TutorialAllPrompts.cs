using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialAllPrompts : MonoBehaviour
{
    //holds all prompts which might appear to the player during the tutorial
    public Dictionary<string, Prompt> prompts;
    private Dictionary<string, float> priorityOrder;
    public MultiSceneVariables savedVariables;
    private bool gamePadNotMouse = false;
    [Header("Prompts")]
    private Prompt movePrompt;
    private Prompt shootPrompt;
    private Prompt jumpPrompt;
    private Prompt afterJumpPrompt;
    private Prompt magnetPrompt;
    private Prompt dropPrompt;
    private Prompt killPilotPrompt;
    [Header("Prompt Input Strings")]
    private string firstMoveString = "Move";
    private string shootString = "Shoot";
    private string jumpString = "Jump";
    private string afterJumpString = "After Jump";
    private string magnetString = "Magnet";
    private string dropString = "Drop";
    private string killPilotString = "Kill";
    [Header("Prompt Output Strings keyboard")]
    private string moveRightK = "Use \"A\" and \"D\" to move left and right";
    private string shootingK = "Hold the Left Mouse Button To Shoot";
    private string jumpingK = "Press Space to Jump. Hold Space Mid-air to Use Jetpack";
    private string afterJumpKG = "Your Jetpack Will Refill Immediately on the Ground and Slowly Mid-air After Not Using it for Long Enough";
    private string magnetingK = "Right Click to Shoot Magnet. Hold \"S\" to Repel and \"W\" to Attract";
    private string dropK = "Hold \"S\" To Drop Through Wooden Floors";
    private string killingPilotK= "Hold \"G\" + \"L\"";
    [Header("Prompt Output Strings gamePad")]
    private string moveRightG = "Use the Left Joystick to move";
    private string shootingG = "Hold Right Trigger to Shoot";
    private string jumpingG = "Press A to Jump. Hold A Mid-air to Use Jetpack";
    private string magnetingG = "Press Left Trigger to Shoot Magnet. Hold Right or Left Bumpers to Repel/Attract";
    private string dropG = "Move Left Joystick down to Drop Through Wooden Floors";
    private string killingPilotG = "Hold Left Trigger, Right Trigger, and A";
    void Awake()
    {
        //set up saved variables
        GameObject savedVariablesObject = GameObject.FindGameObjectWithTag("MultiSceneVariables");
        if (savedVariablesObject != null)
        {
            gamePadNotMouse = savedVariablesObject.GetComponent<MultiSceneVariables>().gamePadNotMouse;
        }

        //set up prompts
        prompts = new Dictionary<string, Prompt>();

        //set up priority order
        priorityOrder = new Dictionary<string, float>
        {
            { firstMoveString, 0 },
            { shootString, 7 },
            { jumpString, 5 },
            { magnetString, 6 },
            { dropString, 4 },
            {afterJumpString, 3 },
            { killPilotString, 10 }
        };

        //move prompt
        movePrompt = gameObject.AddComponent<Prompt>();
        movePrompt.postHocConstructor(
        (gamePadNotMouse ? moveRightG : moveRightK),
        priorityOrder[firstMoveString]
        );
        prompts.Add(firstMoveString, movePrompt);

        //shooting prompt
        shootPrompt = gameObject.AddComponent<Prompt>();
        shootPrompt.postHocConstructor(
        (gamePadNotMouse ? shootingG : shootingK),
        priorityOrder[shootString]
        );
        prompts.Add(shootString, shootPrompt);

        //jump prompt
        jumpPrompt = gameObject.AddComponent<Prompt>();
        jumpPrompt.postHocConstructor(
        (gamePadNotMouse ? jumpingG : jumpingK),
        priorityOrder[jumpString]
        );
        prompts.Add(jumpString, jumpPrompt);
        //jump prompt
        afterJumpPrompt = gameObject.AddComponent<Prompt>();
        afterJumpPrompt.postHocConstructor(
        afterJumpKG,
        priorityOrder[afterJumpString]
        );
        prompts.Add(afterJumpString, afterJumpPrompt);

        //magnet prompt
        magnetPrompt = gameObject.AddComponent<Prompt>();
        magnetPrompt.postHocConstructor(
        (gamePadNotMouse ? magnetingG : magnetingK),
        priorityOrder[magnetString]
        );
        prompts.Add(magnetString, magnetPrompt);

        //drop prompt
        dropPrompt = gameObject.AddComponent<Prompt>();
        dropPrompt.postHocConstructor(
        (gamePadNotMouse ? dropG : dropK),
        priorityOrder[dropString]
        );
        prompts.Add(dropString, dropPrompt);

        //kill pilot prompt
        killPilotPrompt = gameObject.AddComponent<Prompt>();
        killPilotPrompt.postHocConstructor(
        (gamePadNotMouse ? killingPilotG : killingPilotK),
        priorityOrder[killPilotString]
        );
        prompts.Add(killPilotString, killPilotPrompt);
    }
}
