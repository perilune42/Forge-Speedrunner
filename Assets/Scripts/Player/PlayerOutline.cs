using UnityEngine;

public class PlayerOutline : MonoBehaviour
{
    public Material defaultMat;
    public Material outlineMat;

    public int FlashDuration = 10;
    private int currentFlashTimer;

    private bool ready;
    Dash dash;

    private void Start()
    {
        dash = AbilityManager.Instance.GetAbility<Dash>();
        dash.onRecharged += () => ready = true;
    }

    private void FlashOutline()
    {
        currentFlashTimer = FlashDuration;
        Player.Instance.Sprite.material = outlineMat;
    }

    private void FixedUpdate()
    {
        
        if (currentFlashTimer > 0)
        {
            currentFlashTimer--;
            if (currentFlashTimer == 0)
            {
                Player.Instance.Sprite.material = defaultMat;
            }
        }

        if (ready && dash.CanUseAbility()) 
        {
            ready = false;
            FlashOutline();
        }
    }
}