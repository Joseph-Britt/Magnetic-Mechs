using UnityEngine;
using UnityEngine.UI;

public class DifficultyScript : MonoBehaviour
{
    [Header("Components")]
    public Text currentDifficultyText;
    [Header("Scripts")]
    public MultiSceneVariables multiSceneVariables;
    private void Awake()
    {
        multiSceneVariables = GameObject.FindGameObjectWithTag("MultiSceneVariables").GetComponent<MultiSceneVariables>();
    }
    private void FixedUpdate()
    {
        if (multiSceneVariables == null) return;
        if(multiSceneVariables.difficulty == 0)
        {
            currentDifficultyText.text = "Normal";
        }
        else if (multiSceneVariables.difficulty == 1)
        {
            currentDifficultyText.text = "Hard";
        }
        else if (multiSceneVariables.difficulty == 2)
        {
            currentDifficultyText.text = "Impossible";
        }
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
}
