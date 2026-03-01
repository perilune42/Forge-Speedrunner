// using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeToBlack : Singleton<FadeToBlack>
{
    [SerializeField] Animator anim;

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    public IEnumerator FadeOut()
    {
        anim.Play("FadeOutTransition");

        // Takes at least a frame for state to actually change
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        while (anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
        {
            yield return null;
        }
    }

    public IEnumerator FadeIn()
    {
        anim.Play("FadeInTransition");

        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        while (anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
        {
            yield return null;
        }
    }
}
