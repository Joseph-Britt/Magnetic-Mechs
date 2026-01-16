using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JetpackScript : MonoBehaviour
{
    //script for managing the UI associated with the jetpack
    [Header("Components")]
    public Animator animator;

    void Awake()
    {
        animator = gameObject.GetComponent<Animator>();
    }

    public void setJetpack(bool jetpackOn = false)
    {
        animator.SetBool("JetpackOn", jetpackOn);
    }
}
