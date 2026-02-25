using TMPro;
using UnityEngine;

public class Speedometer : MonoBehaviour
{
    
    [SerializeField] private GameObject arrow;
    [SerializeField] private float maxAngle;
    [SerializeField] private float minAngle;
    [SerializeField] private float angleMulti, angleExponent;   
    [SerializeField] private float lerpValue;
    [SerializeField] private TMP_Text text;
    private float angle;

    private void FixedUpdate()
    {
        float mg = Player.Instance.Movement.Velocity.magnitude;
        float target = mg * angleMulti;
        //target = Mathf.Pow(target, angleExponent);
        target = Mathf.Pow(target, angleExponent);
        target = Mathf.Clamp(target, minAngle, maxAngle);
        angle = Mathf.Lerp(angle, target, lerpValue);
        arrow.transform.eulerAngles = (90f - angle) * Vector3.forward;
        text.SetText("{0:1}", mg);
    }
}
