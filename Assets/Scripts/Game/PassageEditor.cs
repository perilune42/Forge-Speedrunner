using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(PassageEditor))]
public class PassageEditor_Inspector : Editor
{
    override public void OnInspectorGUI()
    {
        const string PASSAGE_NAME = "Passages";
        PassageEditor pe = (PassageEditor)target;
        DrawDefaultInspector();

        if(GUILayout.Button("Finalize Passage"))
        {
            // since Start hasn't been called for RoomManager yet, this is the only way to get an instance
            RoomManager rm = Object.FindFirstObjectByType<RoomManager>();
            if(rm == null)
            {
                Debug.Log($"ERROR: No RoomManager found. Please create one, and give it a child object named '{PASSAGE_NAME}'");
                return;
            }
            Transform passageFolder = rm.transform.Find(PASSAGE_NAME);
            if(passageFolder != null)
            {
                EditorUtility.SetDirty(pe.attachedPassage); // changes don't persist without this
                Debug.Log($"Found RoomManager {{{rm}}} and passage folder {{{passageFolder}}}\nDeploying!");
                pe.transform.SetParent(passageFolder, true);
            }
            else
            {
                string debugMsg = $"ERROR: Could not find passage folder! This script tries to make all passages children of a GameObject named '{PASSAGE_NAME}'. ";
                Debug.Log(debugMsg);

            }

        }
    }
}
#endif

[ExecuteAlways]
public class PassageEditor : MonoBehaviour
{
    private Vector3 matchMidpoint = new(0.0F, 0.0F, 0.0F);
    private Vector2 fitSize = new(0.0F, 0.0F);
    private LayerMask layerMask;
    public Passage attachedPassage;
    public Color SuccessColor = Color.green;
    public Color FailColor = Color.red;
    public Vector2 RectangleSize = new(0.0F,0.0F);
    public bool foundFit = false;

    void Start()
    {
        attachedPassage = GetComponent<Passage>();
        layerMask = LayerMask.GetMask("Default");
    }

    void OnDrawGizmos()
    {
        if(Application.isPlaying) return;
        Vector3 center = this.transform.position;
        Vector3 size;
        size = new(RectangleSize.x, RectangleSize.y, 0);
        Gizmos.color = FailColor;
        Gizmos.DrawWireCube(center, size);
        if(foundFit)
        {
            Vector3 successSize = new(fitSize.x, fitSize.y, 0);
            Gizmos.color = SuccessColor;
            Gizmos.DrawCube(matchMidpoint, successSize);
        }
    }

    void Update()
    {
        if (transform.hasChanged && !Application.isPlaying)
        {
            // from one end of the coll to the other
            Vector2 centerFactor = RectangleSize / 2.0F;

            Vector2 topright = (Vector2)this.transform.position + centerFactor;
            Vector2 botleft = (Vector2)this.transform.position - centerFactor;

            Collider2D[] allColliders = Physics2D.OverlapAreaAll(topright, botleft, layerMask);

            // Debug.Log($"Number of colliders: {allColliders.Length}");
            // Debug.Log(allColliders);
            

            // take first two 
            if (allColliders.Length >= 2) 
            {
                Doorway door1 = null;
                Doorway door2 = null;
                foreach (Collider2D col in allColliders)
                {
                    var potentialDoor = col.GetComponent<Doorway>();
                    if (potentialDoor != null)
                    {
                        if (door1 == null) door1 = potentialDoor;
                        else if (door2 == null) door2 = potentialDoor;
                        else break;
                    }
                }

                // Debug.Log($"door1: {door1}, door2: {door2}");
                // Debug.Log($"door1 raw: {allColliders[0]}, door2 raw: {allColliders[1]}");
                if(door1 != null && door2 != null)
                {
                    attachedPassage.door1 = door1;
                    attachedPassage.door2 = door2;
                    fitSize.x = Mathf.Abs(door1.transform.position.x - door2.transform.position.x);
                    fitSize.y = Mathf.Abs(door1.transform.position.y - door2.transform.position.y);
                    fitSize += Vector2.one * 1.5F;
                    foundFit = true;
                    matchMidpoint = (door1.transform.position + door2.transform.position) * 0.5F;
                }
                else
                {
                    foundFit = false;
                }
            }
            else
            {
                attachedPassage.door1 = null;
                attachedPassage.door2 = null;
                foundFit = false;
            }

            // coll.IsTouchingLayers(0); // 0 = default layer
            transform.hasChanged = false;
        }
    }
}
