using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResultRow : MonoBehaviour
{
    [SerializeField] private GameObject[] sprites;
    [SerializeField] private TMP_Text scoreLable;
    [SerializeField] private GameObject smileImg;
    [SerializeField] private GameObject angryImg;

    public void SetValue(int _type, int _score)
    {
        for (var i = 0; i < sprites.Length; i++) sprites[i].SetActive(i + 1 == _type);

        scoreLable.text = _score.ToString();

        smileImg.SetActive(_type == 0);
        angryImg.SetActive(_type != 0);
    }
}
