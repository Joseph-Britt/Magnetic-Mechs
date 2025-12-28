using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagnetProjectileScript : MonoBehaviour
{
    //code for the magnet projectile
    private float deathTime;
    private float startCollidingTime;
    private Rigidbody2D myRigidBody;
    private bool attached;
    public MagnetSpawnerScript myMagnetSpawnerScript;
    private void Awake()
    {
        myRigidBody = GetComponent<Rigidbody2D>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 6 || collision.gameObject.layer == 17)
        {
            if (collision.gameObject.CompareTag("MovingPlatform"))
            {
                myRigidBody.linearVelocity = Vector3.zero;
                GetComponent<CapsuleCollider2D>().enabled = false;
                myRigidBody.bodyType = RigidbodyType2D.Kinematic;
                gameObject.transform.SetParent(collision.transform);
                attached = true;
            }
            else if (collision.gameObject.CompareTag("NonStickPlatform"))
            {
                DestroyThis();
            }
            else
            {
                myRigidBody.linearVelocity = Vector3.zero;
                GetComponent<CapsuleCollider2D>().enabled = false;
                attached = true;
            }
        }
    }
    private void Update()
    {
        if(!attached && Time.time > deathTime)
        {
            DestroyThis();
        }
    }
    public void Reset()
    {
        GetComponent<CapsuleCollider2D>().enabled = true;
        myRigidBody.bodyType = RigidbodyType2D.Dynamic;
        gameObject.transform.parent = null;
        deathTime = Time.time + 5;
        attached = false;
    }
    public void DestroyThis()
    {
        gameObject.SetActive(false);
        if(myMagnetSpawnerScript != null)
        {
            myMagnetSpawnerScript.magnetActive = false;
        }
    }
}
