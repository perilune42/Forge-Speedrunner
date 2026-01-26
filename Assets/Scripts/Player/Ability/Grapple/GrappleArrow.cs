using System;
using UnityEngine;

public class GrappleArrow : MonoBehaviour
{
    [HideInInspector] public GameObject grappleHand;

    private void Start()
    {
        GetComponent<SpriteRenderer>().enabled = true;
    }

    private void FixedUpdate()
    {
        Vector2 pointVec = grappleHand.transform.position - transform.position;
        float angle = Mathf.Atan2(pointVec.y, pointVec.x) * Mathf.Rad2Deg;
        transform.eulerAngles = new Vector3(0, 0, angle);
    }
}
