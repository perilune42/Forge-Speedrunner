using System.Collections;
using UnityEngine;

public class Player : Singleton<Player>
{
    public PlayerMovement Movement;
    public PlayerVFXTrail VFXTrail;
    public SpriteRenderer Sprite;
    public PlayerAudio Audio;
    public bool IsDead;
    
    public override void Awake()
    {
        base.Awake();
    }
}