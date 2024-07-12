using UnityEngine;
using OneSignalSDK;

public class OneSignalExtension : MonoBehaviour
{
    public static string UserId => OneSignal.Default?.User?.OneSignalId;

    public static void Initialize()
    {
        OneSignal.Initialize("e7dc4f7d-3c35-4634-b8ba-1d15865683ca");
    }

    public static void Unsubscribe()
    {
        OneSignal.Notifications?.ClearAllNotifications();
        OneSignal.Logout();
    }
}
