using UnityEngine;
using UnityEditor;

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

//NOTE: THIS IS JUST SUPPOSED TO HOLD EntityInspector
public class EntityButtons : MonoBehaviour
{

}
