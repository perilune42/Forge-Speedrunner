// using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeToBlack : Singleton<FadeToBlack>
{
    Image img;
    [SerializeField] Animator FadeInAnimation;
    [SerializeField] Animator FadeOutAnimation;
    private readonly int endState = Animator.StringToHash("Over");
    private bool isOver(Animator anim)
        => anim.GetCurrentAnimatorStateInfo(0).shortNameHash == endState;
    private IEnumerator waitFor(Animator anim)
    {
        yield return new WaitUntil(() => isOver(anim));
    }
    public override void Awake()
    {
        base.Awake();
        img = GetComponent<Image>();
        FadeInAnimation.Play("Over");
        FadeOutAnimation.Play("Over");
        // FadeInAnimation.enabled = false;
        // FadeOutAnimation.enabled = false;
    }
    public IEnumerator FadeIn()
    {
        // img.DOColor(Color.black, 0.15f);
        FadeInAnimation.Play("FadeInTransition");
        yield return waitFor(FadeInAnimation);
    }

    public IEnumerator FadeOut()
    {
        // img.DOColor(Color.clear, 0.15f);
        FadeOutAnimation.Play("FadeOutTransition");
        yield return waitFor(FadeOutAnimation);
    }
}
