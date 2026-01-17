using UnityEngine;

public class Player : Singleton<Player>
{
    public PlayerMovement Movement;

    public override void Awake()
    {
        base.Awake();
    }
}