#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;



[CustomEditor(typeof(Entity), true)]
public class Entity_Inspector : Editor
{
    override public void OnInspectorGUI()
    {
        Entity e = (Entity)target;
        DrawDefaultInspector();

        if (GUILayout.Button("Snap To Grid"))
        {
            e.transform.localPosition = new Vector3(Mathf.Round(e.transform.localPosition.x * 2) / 2f,
                                                Mathf.Round(e.transform.localPosition.y * 2) / 2f,
                                                0);
            EditorUtility.SetDirty(e.transform);
        }
    }
}
#endif