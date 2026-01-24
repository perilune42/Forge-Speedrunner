using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class FadeToBlack : Singleton<FadeToBlack>
{
    Image img;
    public override void Awake()
    {
        base.Awake();
        img = GetComponent<Image>();
    }
    public void FadeIn()
    {
        img.DOColor(Color.black, 0.15f);
    }

    public void FadeOut()
    {
        img.DOColor(Color.clear, 0.15f);
    }
}
