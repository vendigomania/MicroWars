using UnityEngine;
using OneSignalSDK;

public class OneSignalExtension : MonoBehaviour
{
    public static string UserId => OneSignal.Default?.User?.OneSignalId;

    public static void Unsubscribe()
    {
        OneSignal.Notifications?.ClearAllNotifications();
        OneSignal.Logout();
    }
}
