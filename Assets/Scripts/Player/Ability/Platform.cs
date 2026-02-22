using Unity.Mathematics;
using UnityEngine;

public class Platform : Ability
{
    [SerializeField] private GameObject platformPrefab;
    private GameObject platform;
    private SpriteRenderer platformRenderer;
    [SerializeField] private Vector3 platformSpawnOffset;
    [SerializeField] private int platformDuration;
    private int curPlatformDuation;
    [SerializeField] private Sprite[] platformSprites;
    private int spriteInterval;

    public override void Start()
    {
        base.Start();
        spriteInterval = platformDuration / platformSprites.Length;
        Debug.Log(spriteInterval);
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (platform != null)
        {
            curPlatformDuation--;
            if (curPlatformDuation % spriteInterval == 0) 
            {
                platformRenderer.sprite = platformSprites[platformSprites.Length - (curPlatformDuation / spriteInterval)];
            }
            if (curPlatformDuation <= 0) DestroyPlatform();
        }
        if (inputButton.HasPressed && CanUseAbility() && GetCooldown() >= 1f) UseAbility();
    }

    public override bool CanUseAbility()
    {
        return base.CanUseAbility();
    }

    public override bool UseAbility()
    {
        if (platform != null) 
        {
            Destroy(platform);
            return false;
        }
        else 
        {
            platform = Instantiate(platformPrefab, PlayerMovement.transform.position, Quaternion.identity);
            curPlatformDuation = platformDuration;
            platformRenderer = platform.GetComponent<SpriteRenderer>();
            return base.UseAbility(); 
        }
    }

    public override void OnReset()
    {
        if (platform != null) Destroy(platform);
        base.OnReset();
    }


    private void DestroyPlatform()
    {
        if (platform == null)
        {
            Debug.LogError("Tried to destroy platform while platform is null");
            return;
        }
        Debug.Log("destroyed platform");
        Destroy(platform);
    }
}
