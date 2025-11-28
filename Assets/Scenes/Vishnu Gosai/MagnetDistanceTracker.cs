using UnityEngine;

public class MagnetDistanceTracker : MonoBehaviour
{
    
    public Vector2 distanceToMagnet;   
    public bool outOfRange;           

    private MagnetSpawnerScript spawner;  
    private Transform magnetTransform;     

    void Awake()
    {
        spawner = GetComponentInParent<MagnetSpawnerScript>();
        if (spawner == null)
        {
            Debug.LogWarning($"{nameof(MagnetDistanceTracker)} on {name}: No MagnetSpawnerScript found on parent.");
        }
    }

    void Update()
    {
        if (spawner == null || !spawner.magnetActive)
        {
            outOfRange = false;
            return;
        }

        if (magnetTransform == null)
        {
            GameObject magnetObj = GameObject.Find("MagnetProjectile(Clone)");
            if (magnetObj != null && magnetObj.layer == LayerMask.NameToLayer("Magnet"))
            {
                magnetTransform = magnetObj.transform;
            }
            else
            {
                outOfRange = false;
                return;
            }
        }

        Vector2 selfPos   = transform.position;
        Vector2 magnetPos = magnetTransform.position;

        distanceToMagnet = magnetPos - selfPos;

        outOfRange = distanceToMagnet.magnitude >= 30f;
    }
}
