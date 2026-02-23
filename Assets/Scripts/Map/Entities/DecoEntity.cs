using UnityEditor;
using UnityEngine;


public class DecoEntity : MonoBehaviour
{
    [HideInInspector] public Collider2D Hitbox;
    [SerializeField] Animator animator;
    public float SnapDistance = 0.5f;

    public int wireID;

    protected virtual void Awake()
    {
        Hitbox = GetComponent<Collider2D>();
        gameObject.layer = LayerMask.NameToLayer("Entity");
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            if (animator != null)
            {
                animator.Play("WireBumped" + wireID);
            }
        }
    }

    protected virtual void OnTriggerExit2D(Collider2D collision)
    {
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(DecoEntity), true)]
public class DecoEntity_Inspector : Editor
{
    
    override public void OnInspectorGUI()
    {
        DecoEntity e = (DecoEntity)target;
        DrawDefaultInspector();

        if (GUILayout.Button("Snap To Grid"))
        {
            e.transform.localPosition = new Vector3(Mathf.Round(e.transform.localPosition.x / e.SnapDistance) * e.SnapDistance,
                                                Mathf.Round(e.transform.localPosition.y / e.SnapDistance) * e.SnapDistance,
                                                0);
            for (int i = 0; i < e.transform.childCount; i++)
            {
                e.transform.GetChild(i).localPosition = Vector3.zero;
            }
            EditorUtility.SetDirty(e.transform);
        }
    }
}
#endif

