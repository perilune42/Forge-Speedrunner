using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(MapGen))]
public class MapGenEditor_Inspector : Editor
{
    override public void OnInspectorGUI()
    {
        DrawDefaultInspector();
        MapGen mg = (MapGen)target;
        if(GUILayout.Button("Generate a map"))
        {
            mg.CreateMap();
        }
        else if(GUILayout.Button("Destroy the map"))
        {
            mg.DeleteMap();
        }
        else if(GUILayout.Button("Test the algorithm"))
        {
            mg.Test();
        }
        else if(GUILayout.Button("See a failed test"))
            mg.CreateFailedTest(mg.CheckThis);
    }
}
#endif
