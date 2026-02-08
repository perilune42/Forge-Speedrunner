using UnityEngine;

public class Player : Singleton<Player>
{
    public PlayerMovement Movement;
    public PlayerVFXTrail VFXTrail;
    public SpriteRenderer Sprite;
    
    public override void Awake()
    {
        base.Awake();
    }
}