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
        else if(GUILayout.Button("DEBUG: awake the registry"))
        {
            Debug.Log("awaken!");
            GameRegistry.Instance.Awake();
        }
    }
}
#endif
