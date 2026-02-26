// using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeToBlack : Singleton<FadeToBlack>
{
    Image img;
    [SerializeField] Animator FadeAnimation;
    private readonly int endState = Animator.StringToHash("Over");
    private bool isOver(Animator anim)
        => anim.GetCurrentAnimatorStateInfo(0).IsName("Over");
    private IEnumerator waitFor(Animator anim)
    {
        while(!isOver(anim))
            yield return null;
    }
    public override void Awake()
    {
        base.Awake();
        img = GetComponent<Image>();
        img.color = new Vector4(0f,0f,0f,0f);;
    }

    public IEnumerator Fade()
    {
        FadeAnimation.Play("FadeOut", 0, 0f);
        FadeAnimation.Update(0f);
        yield return Listener;
    }
    public IEnumerator Listener => waitFor(FadeAnimation);
}
