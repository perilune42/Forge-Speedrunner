using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class PlayerVFXTrail : MonoBehaviour
{
    [SerializeField] private GameObject particlePrefab;
    private ParticleSystem particle;
    private ParticleSystemRenderer particleRenderer;

    [SerializeField] private SpriteRenderer playerSpriteRenderer;
    [SerializeField] private List<Material> particleMaterials;
    [SerializeField] private PlayerMovement playerMovement;
    private Texture2D whitePlayerTexture; // player texture but with only white pixels

    private void Awake()
    {
        whitePlayerTexture = new Texture2D(playerSpriteRenderer.sprite.texture.width,
            playerSpriteRenderer.sprite.texture.height);
        Color[] pixels = playerSpriteRenderer.sprite.texture.GetPixels();
        for (int i = 0; i < pixels.Length; i++)
        {
            Color col = pixels[i];
            if (Mathf.Approximately(col.a, 0)) continue;
            pixels[i] = Color.white;
        }
        whitePlayerTexture.SetPixels(pixels);
        whitePlayerTexture.Apply();
    }

    public Action PlayParticle(Color color)
    {
        var particleObj = Instantiate(particlePrefab, transform);
        particle = particleObj.GetComponent<ParticleSystem>();
        particleRenderer = particleObj.GetComponent<ParticleSystemRenderer>();
        particle.Play();
        UpdateSprite();
        UpdateColor(color);
        return () => StopParticle(particleObj);
    }

    private IEnumerator DestroyParticle(float fadeDuration, GameObject particleObj)
    {
        yield return new WaitForSeconds(fadeDuration);
        Destroy(particleObj);
    }

    public void StopParticle(GameObject particleObj)
    {
        if (particleObj == null) return;
        particleObj.GetComponent<ParticleSystem>().Stop();
        StartCoroutine(DestroyParticle(1f, particleObj));
        particleObj = null;
    }

    public void UpdateSprite()
    {
        Rect rect = playerSpriteRenderer.sprite.textureRect;
        Texture2D tex = new Texture2D((int)rect.width, (int)rect.height);
        tex.SetPixels(whitePlayerTexture.GetPixels((int)rect.xMin, (int)rect.yMin, (int)rect.width, (int)rect.height, 0));
        tex.Apply();
        particleMaterials[0] = new Material(source: particleMaterials[0]);
        particleMaterials[0].mainTexture = tex;
        particleRenderer.SetMaterials(particleMaterials);
        particleRenderer.flip = Vector3.right * (playerMovement.FacingDir.x < 0 ? 1 : 0);
    }

    private void UpdateColor(Color color)
    {
        
        var main = particle.main;
        main.startColor = color;

        var colorOverLifetime = particle.colorOverLifetime;
        GradientColorKey[] colorKeys = new GradientColorKey[2];
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];

        colorKeys[0] = new GradientColorKey(color, 1f);
        colorKeys[1] = new GradientColorKey(color, 1f);
        alphaKeys[0] = new GradientAlphaKey(0f, 1f);
        alphaKeys[1] = new GradientAlphaKey(1f, 0f);
        
        Gradient gradient = new Gradient();
        gradient.SetKeys(colorKeys, alphaKeys);

        colorOverLifetime.color = gradient;
    }
}
