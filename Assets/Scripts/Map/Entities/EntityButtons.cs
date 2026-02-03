using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(EntityButtons))]
public class EntityInspector : Editor
{
    override public void OnInspectorGUI()
    {

        Entity[] entities = ((EntityButtons)target).GetComponentsInChildren<Entity>();
        if (GUILayout.Button("Run all OnValidate methods"))
        {
            foreach(Entity entity in entities)
                entity.OnValidate();
        }
    }
}
#endif

//NOTE: THIS IS JUST SUPPOSED TO HOLD EntityInspector
public class EntityButtons : MonoBehaviour
{

}
