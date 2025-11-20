using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartNextLevel : MonoBehaviour
{
    public string levelToLoad = "fill in here";
    public float timeToWait = 1.5f;
    [Header("Components")]
    private MultiSceneVariables multiSceneVariables;
    private void Awake()
    {
        multiSceneVariables = GameObject.FindGameObjectWithTag("MultiSceneVariables").GetComponent<MultiSceneVariables>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 3) StartCoroutine(StartSpecifiedLevel());
    }
    public IEnumerator StartSpecifiedLevel()
    {
        if (multiSceneVariables != null) multiSceneVariables.setCheckpoint(0);
        yield return new WaitForSeconds(timeToWait);
        SceneManager.LoadScene(levelToLoad);
    }
}
