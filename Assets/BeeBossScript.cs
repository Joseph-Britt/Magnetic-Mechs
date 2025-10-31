using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class BeeBossScript : MonoBehaviour
{
    [Header("Variables")]
    private bool bossActive;
    private float bulletDamage = 1f;
    private float chargeDamage = 1f;
    private float flashTime = .2f;
    [Header("Components")]
    public BeeBossHealthScript beeBossHealthScript;
    public SpriteRenderer sprite;
    public Transform playerTransform;
    public GameObject endingPlatformGrid;
    public GameObject StartNextLevel;


    void Awake()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.GetComponent<Transform>();
        else Debug.Log("player could not be found");
        sprite = GetComponent<SpriteRenderer>();
        endingPlatformGrid.SetActive(false);
        bossActive = false;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 8)
        {
            damageBoss(bulletDamage);
        }
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.layer == 3 && playerTransform.GetComponent<PlayerScript>().isCharging)
        {
            damageBoss(chargeDamage);
        }
    }
    public void damageBoss(float damage)
    {
        if (beeBossHealthScript == null || !bossActive) return;
        beeBossHealthScript?.takeDamage(damage);
        StartCoroutine(FlashRed());
    }
    public IEnumerator FlashRed()
    {
        sprite.color = Color.red;
        yield return new WaitForSeconds(flashTime);
        sprite.color = Color.white;
    }
    public void KillBoss()
    {
        if (!bossActive)
        {
            return;
        }
        bossActive = false;
        //animator.SetBool("hasDied", true);
        //myRigidbody2D.linearVelocity = new Vector3(0, 0, 0);
        StartCoroutine(HandleDeath());
    }
    IEnumerator HandleDeath()
    {
        yield return new WaitForSeconds(1);
        //yield return new WaitUntil(() => gameObject.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Dead"));
        //Destroy(gameObject);
        transform.parent.GetComponent<BeeBossParentScript>().beeBossActive = false;
        endingPlatformGrid.SetActive(true);
        StartNextLevel.SetActive(true);
        gameObject.SetActive(false);

    }
    public void activateBoss()
    {
        bossActive = true;
    }
}
