using UnityEngine;
[CreateAssetMenu(fileName = "Room", menuName = "Scriptable Objects/Room")]
public class Room : ScriptableObject
{
    [SerializeField] public GameObject RoomPrefab;
    [SerializeField] public Room left;
    [SerializeField] public Room right;
    [SerializeField] public Room up;
    [SerializeField] public Room down;
}
