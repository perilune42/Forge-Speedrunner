using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;

public class Platform : Ability, IStatSource
{
    [SerializeField] private GameObject platformPrefab;
    [HideInInspector] public GameObject platform;
    private SpriteRenderer platformRenderer;
    private BoxCollider2D platformCollider;
    [SerializeField] private Vector3 platformSpawnOffset;
    [SerializeField] private int platformDuration;
    private int curPlatformDuation;
    [SerializeField] private Sprite[] platformSprites;
    private int spriteInterval;
    
    [SerializeField] private float tileWidth;
    private float leftPointer;
    private float rightPointer;

    public float dashVelocityMulti;

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
                platformRenderer.sprite = platformSprites[Mathf.Max(0, platformSprites.Length - (curPlatformDuation / spriteInterval))];
            }
            if (curPlatformDuation <= 0) DestroyPlatform();

            if (CurrentLevel >= 1)
            {
                if (PlayerMovement.transform.position.x < leftPointer)
                {
                    ShiftPlatform(-1);
                }
                else if (PlayerMovement.transform.position.x > rightPointer)
                {
                    ShiftPlatform(1);
                }
            }
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
            leftPointer = platform.transform.position.x - tileWidth / 2;
            rightPointer = leftPointer + tileWidth;
            curPlatformDuation = platformDuration;
            platformRenderer = platform.GetComponent<SpriteRenderer>();
            platformCollider = platform.GetComponent<BoxCollider2D>();
            return base.UseAbility(); 
        }
    }

    public override void OnReset()
    {
        if (platform != null) Destroy(platform);
        base.OnReset();
    }

    public bool IsPlayerTouchingPlatform()
    {
        if (platform == null) return false;
        RaycastHit2D boxCast = Physics2D.BoxCast(PlayerMovement.transform.position, PlayerMovement.SurfaceCollider.bounds.size, 0f, Vector2.down, 0.1f);
        return boxCast.collider == platformCollider;
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

    private void ShiftPlatform(int direction)
    {
        Debug.Log("Shifting platform");
        if (direction == -1) leftPointer -= tileWidth;   
        else if (direction == 1) rightPointer += tileWidth;
        else return;
        platformRenderer.size += Vector2.right * tileWidth;
        platform.transform.position += Vector3.right * (tileWidth * direction / 2);
        platformCollider.size += Vector2.right * tileWidth;
    }
}
