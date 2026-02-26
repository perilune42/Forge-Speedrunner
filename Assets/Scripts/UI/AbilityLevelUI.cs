using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AbilityLevelUI : MonoBehaviour
{
    [SerializeField] List<Image> levelImages;

    public void SetLevel(int level)
    {
        for (int i = 0; i < levelImages.Count; i++)
        {
            if (level >= i)
            {
                levelImages[i].enabled = true;
            }
            else
            {
                levelImages[i].enabled = false;
            }
        }
    }

}