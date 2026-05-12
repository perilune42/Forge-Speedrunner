using FMODUnity;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DataGate : Entity
{
    public override bool IsSolid => true;

    [SerializeField] TMP_Text dataRequiredText;

    protected override void Awake()
    {
        Game.Instance.OnUpdateDataCount += UpdateGate;
    }

    void UpdateGate()
    {
        if (Game.Instance.GetDataCollected() >= Game.Instance.GetDataRequired())
        {
            transform.GetChild(0).gameObject.SetActive(false);
        }
        else
        {
            transform.GetChild(0).gameObject.SetActive(true);
            dataRequiredText.text = $"{Game.Instance.GetDataCollected()}/{Game.Instance.GetDataRequired()}";
        }
    }
}