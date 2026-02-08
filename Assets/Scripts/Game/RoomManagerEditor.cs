using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
[CustomEditor(typeof(RoomManager))]
public class RoomManager_Inspector : Editor
{
    override public void OnInspectorGUI()
    {
        DrawDefaultInspector();
        RoomManager rm = (RoomManager)target;
        if(GUILayout.Button("Finalize rooms"))
        {
            rm.AllRooms = rm.GetComponentsInChildren<Room>().ToList();
            EditorUtility.SetDirty(rm);
        }
    }
}
#endif
