using System;
using Unity.Mathematics;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

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

    private ParticleSystem particle;

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
            
            if (curPlatformDuation == 0) StartCoroutine(DestroyPlatform());
            else
            {
                curPlatformDuation--;
                if (curPlatformDuation % spriteInterval == 0) 
                {
                    int index = platformSprites.Length - (curPlatformDuation / spriteInterval);
                    index = Mathf.Clamp(index, 0, platformSprites.Length - 1);
                    platformRenderer.sprite = platformSprites[index];
                }
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
            particle = platform.GetComponentInChildren<ParticleSystem>();
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
        List<RaycastHit2D> results = new();
        Physics2D.BoxCast(PlayerMovement.transform.position, PlayerMovement.SurfaceCollider.bounds.size, 0f, Vector2.down, ContactFilter2D.noFilter, results, 0.1f);
        foreach (RaycastHit2D raycastHit2D in results)
        {
            if (raycastHit2D.collider == platformCollider) return true;
        }
        return false;
    }

    private IEnumerator DestroyPlatform()
    {
        if (platform == null)
        {
            Debug.LogError("Tried to destroy platform while platform is null");
            yield break;
        }
        platformCollider.enabled = false;
        platformRenderer.enabled = false;
        yield return new WaitForSeconds(1f);
        Destroy(platform);
    }

    private void ShiftPlatform(int direction)
    {
        if (direction == -1) 
        {
            particle.transform.position = new Vector3(leftPointer, particle.transform.position.y, 0);
            leftPointer -= tileWidth;  
        }
        else if (direction == 1) 
        {
            particle.transform.position = new Vector3(rightPointer, particle.transform.position.y, 0);
            rightPointer += tileWidth;
        }
        else return;
        platformRenderer.size += Vector2.right * tileWidth;
        platform.transform.position += Vector3.right * (tileWidth * direction / 2);
        platformCollider.size += Vector2.right * tileWidth;
        particle.Play();
    }
}
