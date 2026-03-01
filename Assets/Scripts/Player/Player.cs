using System.Collections;
using UnityEngine;

public class Player : Singleton<Player>
{
    public PlayerMovement Movement;
    public PlayerVFXTrail VFXTrail;
    public SpriteRenderer Sprite;
    public PlayerAudio Audio;
    public bool IsDead; // True from start of death animation to end of fade out animation
    
    public override void Awake()
    {
        base.Awake();
    }
}