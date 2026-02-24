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

    private void FixedUpdate()
    {
        float angle = arrow.transform.eulerAngles.z;
        float target = Mathf.Abs(Player.Instance.Movement.Velocity.x * angleMulti);
        //target = Mathf.Pow(target, angleExponent);
        target = Mathf.Clamp(target, minAngle, maxAngle);
        target = Mathf.Pow(target, angleExponent);
        angle = Mathf.Lerp(angle, target, lerpValue);
        arrow.transform.eulerAngles = angle * Vector3.forward;
        text.SetText("{0:1}", Mathf.Abs(Player.Instance.Movement.Velocity.x));
    }
}
