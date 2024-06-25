using UnityEngine;
using OneSignalSDK;

public class OneSignalExtension : MonoBehaviour
{
    public static string UserId => OneSignal.User?.PushSubscription?.Id;
    public static string PushToken => OneSignal.User?.PushSubscription?.Token;

    public static void SetExternalId(string _id)
    {
        OneSignal.Login(_id);
    }

    public static void Unsubscribe()
    {
        OneSignal.Notifications?.ClearAllNotifications();
        OneSignal.Logout();
    }
}
