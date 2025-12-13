using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class MagnetSpawnerScript : MonoBehaviour
{
    //script for launching the magnet from the player
    private float LaunchForce = 35f;
    private float reloadTime = .6f;
    private float timer;
    private bool magnetDisabled = false;
    public GameObject myMagnet;
    public bool magnetActive;
    [Header("Components")]
    public GameObject magnetPrefab;
    public GameObject magnetSpawnpoint;
    //private AudioSource audioBox;

    public GameObject player;
    void Start()
    {
        timer = reloadTime;
        //audioBox = gameObject.GetComponent<AudioSource>();
        player = GameObject.FindGameObjectWithTag("Player");
        initializeMagnet();
        player.GetComponent<PlayerScript>().setMagnet(myMagnet);
        magnetActive = false;
    }
    private void initializeMagnet()
    {
        myMagnet = Instantiate(magnetPrefab);
        myMagnet.GetComponent<MagnetProjectileScript>().myMagnetSpawnerScript = this;
        myMagnet.SetActive(false);
    }
    private void FixedUpdate()
    {
        timer = timer + Time.fixedDeltaTime;
    }
    public void Launch()
    {
        if (player == null)
        {
            Debug.Log("The player could not be found");
            return;
        }
        if (timer < reloadTime || magnetDisabled) return;
        //GameObject magnet = Instantiate(magnetPrefab, magnetSpawnpoint.transform.position + new Vector3(0,0,-1), transform.rotation);
        magnetActive = true;
        myMagnet.GetComponent<MagnetProjectileScript>().Reset();
        myMagnet.transform.position = magnetSpawnpoint.transform.position + new Vector3(0, 0, -1);
        myMagnet.transform.rotation = transform.rotation;
        //myMagnet.transform.Rotate(new Vector3(0, 0, 90));
        myMagnet.SetActive(true);
        Rigidbody2D magnetRB = myMagnet.GetComponent<Rigidbody2D>();
        magnetRB.linearVelocity = Vector3.zero;
        magnetRB.AddForce(transform.right * LaunchForce, ForceMode2D.Impulse);
        //audioBox.Play();
        timer = 0;

    }
    //Events
    public void DisableShooting()
    {
        magnetDisabled = true;
    }
    public void EnableShooting()
    {
        magnetDisabled = false;
    }
}
