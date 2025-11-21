using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CivilianScript : MonoBehaviour
{
    //handles the script for turning the targetting reticule on for players
    [Header("Components")]
    public GameObject targetingReticle;

    public void turnOnTargetingReticle()
    {
        targetingReticle.GetComponent<SpriteRenderer>().enabled = true;
    }
    public void turnOffTargetingReticle()
    {
        targetingReticle.GetComponent<SpriteRenderer>().enabled = false;
    }
}
