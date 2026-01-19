using UnityEngine;
[CreateAssetMenu(fileName = "RoomSO", menuName = "Scriptable Objects/RoomSO")]
public class RoomSO : ScriptableObject
{
    [SerializeField] public GameObject RoomPrefab;
    [SerializeField] public RoomSO left;
    [SerializeField] public RoomSO right;
    [SerializeField] public RoomSO up;
    [SerializeField] public RoomSO down;
}
