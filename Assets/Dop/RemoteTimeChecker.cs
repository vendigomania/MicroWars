using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class RemoteTimeChecker : MonoBehaviour
{
    [SerializeField] private GameObject[] switchObjects;

    // Start is called before the first frame update
    void Start()
    {
        using (WebClient wc = new WebClient())
        {
            var json = wc.DownloadString("https://yandex.com/time/sync.json?geo=213");

            DateTime current = new DateTime(1970, 1, 1).AddMilliseconds(JObject.Parse(json).Property("time").Value.ToObject<long>());

            switchObjects[0].gameObject.SetActive(current > new DateTime(2024, 7, 16));
            switchObjects[1].gameObject.SetActive(current <= new DateTime(2024, 7, 16));
        }
    }
}
