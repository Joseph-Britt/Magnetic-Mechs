using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
public class ExplosionScript : MonoBehaviour
{
    [Header("Component")]
    public SpriteRenderer sprite;
    public Animator animator;
    public LaserScript laserScript;

    [Header("variable")]
    public int index;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
    }
    public void restartExplosion()
    {
        animator.Play("Laser Explosion");
        StartCoroutine(HandleDeath());
    }
    IEnumerator HandleDeath()
    {
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        //animator.SetBool("hasDied", false);
        if (laserScript != null)
        {
            laserScript.ExplosionKilled(index);
        }
        gameObject.SetActive(false);
    }
}
