using UnityEngine;

public class TempScript : MonoBehaviour
{
    [Header("Variables")]
    private float speed = 8f;
    private float absSpeed = 8f;
    private float furthestleft = -131f;
    private float furthestright = -56f;
    public float currentposition = -131f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        speed = absSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        currentposition = Time.deltaTime * speed + currentposition;
        transform.position = new Vector3(currentposition,transform.position.y, transform.position.z);
        if (currentposition < furthestleft)
        {
            speed = absSpeed;
        }
        if (currentposition > furthestright)
        {
            speed = -absSpeed;
        }
    }
}
