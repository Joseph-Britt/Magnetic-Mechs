using UnityEngine;

public class ActivateGameObjectsScript : MonoBehaviour
{
    public GameObject ObjectToActivate;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 3 && ObjectToActivate != null) ObjectToActivate.SetActive(true);
    }
}
