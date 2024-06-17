using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomToggle : MonoBehaviour
{
    [SerializeField] private GameObject on;
    [SerializeField] private GameObject off;

    public void SetActive(bool active)
    {
        on.SetActive(active);
        off.SetActive(!active);
    }
}
