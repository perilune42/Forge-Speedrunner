using UnityEngine;

public class Rocket : Ability
{
    [SerializeField] private int duration, windupDuration;
    private int curDuration;
    private float curSteering;
    private float speed;
    [SerializeField] private float minSpeed, maxSpeed, acceleration, steering, launchSteering, launchSteeringUpgraded, steeringLoss;
    private Vector2 inertia;
    private bool launched;
    private float angle;
    [SerializeField] private GameObject rocketVisualPrefab;
    private GameObject rocketVisual;
    public override void Start()
    {
        base.Start();
        PlayerMovement.OnHitWallAny += (Entity, Vector2) =>
        {
            if (launched) CancelAbility();
        };
    }

    public override void OnReset()
    {
        base.OnReset();

    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (PlayerMovement.SpecialState == SpecialState.Rocket)
        {
            curDuration++;
            if (curDuration < duration)
            {
                if (curDuration == windupDuration)
                {
                    Launch();
                }
                else if (curDuration < windupDuration) 
                {
                    speed += acceleration;
                    if (speed > maxSpeed) speed = maxSpeed;
                    curSteering *= steeringLoss;
                    if (CurrentLevel >= 2 && curSteering < launchSteeringUpgraded) curSteering = launchSteeringUpgraded;
                    inertia *= 0.9f;
                }
                else
                {
                    curSteering = CurrentLevel >= 2 ? launchSteeringUpgraded : launchSteering;
                }
                HandleSteering();
                PlayerMovement.Velocity = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)) * speed + inertia;
            }
            else
            {
                CancelAbility();
            }
        }
        else if (inputButton.HasPressed) UseAbility();
    }

    private void Launch()
    {
        speed = maxSpeed;
        stopParticleAction += PlayerVFXTrail.PlayParticle(Color.yellow);
        launched = true;
        inertia = Vector2.zero;
    }

    private void HandleSteering()
    {
        float steerSpeed = curSteering;
        if (steerSpeed < 0) return;
        angle -= PInput.Instance.MoveVector.x * steerSpeed;
        Player.Instance.Sprite.transform.eulerAngles = Vector3.forward * (angle - 90f);
    }

    public override bool CanUseAbility()
    {
        if (PlayerMovement.SpecialState != SpecialState.Normal) return false;
        return base.CanUseAbility();
    }

    public override bool UseAbility()
    {
        if (!CanUseAbility()) return false;
        if (rocketVisual != null) Destroy(rocketVisual);
        curDuration = 0;
        PlayerMovement.SpecialState = SpecialState.Rocket;
        curSteering = steering;
        speed = minSpeed;
        angle = 90f;
        inertia = PlayerMovement.Velocity;
        launched = false;
        rocketVisual = Instantiate(rocketVisualPrefab, Player.Instance.Sprite.transform);
        return base.UseAbility();
    }

    private void CancelAbility()
    {
        PlayerMovement.SpecialState = SpecialState.Normal;
        rocketVisual.transform.SetParent(null, true);
        DynamicEntity de = rocketVisual.GetComponent<DynamicEntity>();
        de.enabled = true;
        de.Velocity = PlayerMovement.Velocity;
        de.OnHitWallAny += (Entity, Vector2) => Destroy(rocketVisual);
        rocketVisual.GetComponent<ParticleSystem>().Stop();
        Player.Instance.Sprite.transform.eulerAngles = Vector3.zero;
        launched = false;
        stopParticleAction?.Invoke();
    }
} 
