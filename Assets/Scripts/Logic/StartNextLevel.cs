using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartNextLevel : MonoBehaviour
{
    public string levelToLoad = "fill in here";
    private float timeToWait = 1.75f;
    [Header("Components")]
    private MultiSceneVariables multiSceneVariables;
    private LogicScript logic;
    private void Awake()
    {
        logic = GameObject.FindGameObjectWithTag("Logic").GetComponent<LogicScript>();
        multiSceneVariables = GameObject.FindGameObjectWithTag("MultiSceneVariables").GetComponent<MultiSceneVariables>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 3) StartCoroutine(StartSpecifiedLevel());
    }
    public IEnumerator StartSpecifiedLevel()
    {
        logic.StartScreenFade();
        if (multiSceneVariables != null) multiSceneVariables.setCheckpoint(0);
        yield return new WaitForSeconds(timeToWait);
        logic.StartLevel(levelToLoad);
    }
}
