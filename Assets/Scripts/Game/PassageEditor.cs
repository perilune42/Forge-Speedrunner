using UnityEngine;
using UnityEditor;

[ExecuteAlways]
public class PassageEditor : MonoBehaviour
{
    public Passage attachedPassage;
    private LayerMask layerMask;
    public Color SuccessColor = Color.green;
    public Color FailColor = Color.red;
    public Vector2 RectangleSize = new(0.0F,0.0F);
    public Vector2 fitSize = new(0.0F, 0.0F);
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
        if (!foundFit)
        {
            size = new(RectangleSize.x, RectangleSize.y, 0);
            Gizmos.color = FailColor;
        }
        else
        {
            size = new(fitSize.x, fitSize.y, 0);
            Gizmos.color = SuccessColor;
        }
        Gizmos.DrawCube(center, size);
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
                Doorway door1 = allColliders[0].GetComponent<Doorway>();
                Doorway door2 = allColliders[1].GetComponent<Doorway>();
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
