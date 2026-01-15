using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneTransitionScript : MonoBehaviour
{
    //hold the logic for starting level 2 when the associated object touches the player. Probably deprecated.
    [Header("Components")]
    public LogicScript logic;
    void Awake()
    {
        logic = GameObject.FindGameObjectWithTag("Logic").GetComponent<LogicScript>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == 3)
        {
            logic.StartLevel("Level 2");
        }
    }
}
