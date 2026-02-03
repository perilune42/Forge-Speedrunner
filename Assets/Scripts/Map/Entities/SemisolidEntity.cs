using UnityEngine;

public class SemisolidEntity : MonoBehaviour
{

    Collider2D col;
    private void Awake()
    {
        col = GetComponent<Collider2D>();
    }

    private void FixedUpdate()
    {
        //var pm = Player.Instance.Movement;
        //if (pm.transform.position.y >= col.bounds.max.y)
        //{
        //    gameObject.layer = LayerMask.NameToLayer("Solid");
        //}
        //else
        //{
        //    gameObject.layer = LayerMask.NameToLayer("Default");
        //}
    }

    public Bounds GetBounds()
    {
        return col.bounds;
    }
}