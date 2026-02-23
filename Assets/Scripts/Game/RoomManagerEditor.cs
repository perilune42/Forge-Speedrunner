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
            rm.AllRooms = FindObjectsByType<Room>(FindObjectsSortMode.InstanceID).ToList();

            foreach (Room room in rm.AllRooms) {
                Undo.RecordObject(room.transform, "Grid-align rooms");

                Vector3 roomPos = room.transform.position;
                roomPos.x = room.gridPosition.x * rm.BaseWidth * 1.2f;
                roomPos.y = room.gridPosition.y * rm.BaseHeight * 1.2f;
                room.transform.position = roomPos;
            }
            foreach (Passage passage in FindObjectsByType<Passage>(FindObjectsSortMode.None))
            {
                var pe = passage.GetComponent<PassageEditor>();
                if(pe != null)
                    pe.FinalizePassage();
            }
            
            EditorUtility.SetDirty(rm);
        }
    }
}
#endif
