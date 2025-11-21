using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenuMagnetManager : MonoBehaviour
{
    public GameObject effect;
    public int currentSelection;
    [SerializeField] private float[] heights;

    private void Awake()
    {
        ResetSelection();
    }

    private void Update()
    {
        effect.transform.localPosition = new Vector3(effect.transform.localPosition.x, heights[currentSelection], 0);
    }

    public void Move(InputAction.CallbackContext context)
    {
        float change = context.ReadValue<Vector2>().x;
        if (change > 0.25 && currentSelection < 3)
        {
            currentSelection++;
        }
        else if (change < 0.25 && currentSelection > 0)
        {
            currentSelection--;
        }
    }

    public void HoverButton(int hover)
    {
        currentSelection = hover;
    }
    
    public void ResetSelection()
    {
        currentSelection = 0;
    }
}
