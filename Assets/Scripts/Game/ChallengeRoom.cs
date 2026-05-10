using UnityEngine;

public class ChallengeRoom : MonoBehaviour
{
    public Collectible Collectible;
    public Transform StartPoint;
    private void Awake()
    {
        Collectible.AttachToRoom(this);
    }
}