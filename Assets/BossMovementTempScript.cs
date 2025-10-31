using UnityEngine;

public class BossMovementTempScript : MonoBehaviour
{
    // Update is called once per frame
    void FixedUpdate()
    {
        this.transform.Translate(0.03f, 0f, 0f);
    }
}
