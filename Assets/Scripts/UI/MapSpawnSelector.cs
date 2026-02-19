using UnityEngine;

public class MapSpawnSelector : MonoBehaviour
{
    Doorway linkedDoorway;

    public void LinkToDoorway(Doorway linkedDoorway)
    {
        this.linkedDoorway = linkedDoorway;
        transform.eulerAngles = new Vector3(0, 0, Vector2.SignedAngle(Vector2.up, -linkedDoorway.GetTransitionDirection()));
    }

    public void SpawnNow()
    {
        Game.Instance.ReturnToPlay(true, linkedDoorway);
    }
}