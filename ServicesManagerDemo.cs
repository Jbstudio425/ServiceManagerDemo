using System;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
#if UNITY_ANDROID
using Unity.Notifications.Android;
#elif UNITY_IOS
using Unity.Notifications.iOS;
#endif
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEngine.Analytics;
// using Firebase;
// using mixpanel;


public class ServicesManagerDemo : MonoBehaviour {
    public static ServicesManagerDemo instance;
    private void Awake() {
        if(instance == null) instance = this;
        else Destroy(this.gameObject);
        DontDestroyOnLoad(this.gameObject);
    }

    private void Start() {
        TestRequest();
        //if user open app via sended notification
        #region user click notification ot not
        #if UNITY_ANDROID
        var notificationIntentData = AndroidNotificationCenter.GetLastNotificationIntent();
        
        if (notificationIntentData != null)
        {
            // Mixpanel.Track("Local_Notification","status",1);
            // AnalyticsEvent.Custom("Local_Notification",new Dictionary<string,object>{{"status",1}});
            // Firebase.Analytics.FirebaseAnalytics.LogEvent("Local_Notification","status",1);
            Debug.Log("status_true");
        }else{
            // Mixpanel.Track("Local_Notification","status",0);
            // AnalyticsEvent.Custom("Local_Notification",new Dictionary<string,object>{{"status",0}});
            // Firebase.Analytics.FirebaseAnalytics.LogEvent("Local_Notification","status",0);
            
        }
        #elif UNITY_IOS
        // var n = iOSNotificationCenter.GetLastRespondedNotification();
        // if (n != null)
        // {
        //     Mixpanel.Track("Local_Notification","status",1);
        //     AnalyticsEvent.Custom("Local_Notification",new Dictionary<string,object>{{"status",1}});
        //     Firebase.Analytics.FirebaseAnalytics.LogEvent("Local_Notification","status",1);
        // }
        // else
        // {
        //     Mixpanel.Track("Local_Notification","status",0);
        //     AnalyticsEvent.Custom("Local_Notification",new Dictionary<string,object>{{"status",0}});
        //     Firebase.Analytics.FirebaseAnalytics.LogEvent("Local_Notification","status",0);
        // }
        #endif
        #endregion
        
    }
    #if UNITY_ANDROID || UNITY_IOS
    private void OnApplicationPause(bool gameStatus)
    {
        if (gameStatus)//set notification when app goes to background
        {
#if UNITY_ANDROID
            string channel = "missyouchannel";
            var c = new AndroidNotificationChannel()
            {
                Id = channel,
                Name = "Default Channel",
                Importance = Importance.High,
                Description = "Generic notifications",
            };
            AndroidNotificationCenter.RegisterNotificationChannel(c);
            var notification = new AndroidNotification();
            notification.Title = "";
            notification.Text = "";
            notification.FireTime = DateTime.Now.AddDays(3);
            

            AndroidNotificationCenter.SendNotification(notification, channel);

            var dailyNotification = new AndroidNotification();
            dailyNotification.Title = "";
            dailyNotification.Text = "";
            dailyNotification.FireTime = DateTime.Now.AddDays(1);
            
            

            AndroidNotificationCenter.SendNotification(dailyNotification, channel);
#elif UNITY_IOS
            var timeTrigger = new iOSNotificationTimeIntervalTrigger()
            {
            TimeInterval = new TimeSpan(72, 0, 0),
            Repeats = false
            };

            var dailyTimeTrigger = new iOSNotificationTimeIntervalTrigger()
            {
            TimeInterval = new TimeSpan(24, 0, 0),
            Repeats = false
            };

            var notificationIOS = new iOSNotification()
            {
              // You can optionally specify a custom identifier which can later be 
              // used to cancel the notification, if you don't set one, a unique 
              // string will be generated automatically.
              Identifier = "three_day",
              Title = "",
              Body = "",
              Subtitle = "",
              ShowInForeground = true,
              ForegroundPresentationOption = (PresentationOption.Alert | PresentationOption.Sound),
              CategoryIdentifier = "category_a",
              ThreadIdentifier = "thread1",
              Trigger = timeTrigger,
            };

            iOSNotificationCenter.ScheduleNotification(notificationIOS);

            var dailyNotificationIOS = new iOSNotification()
            {
              // You can optionally specify a custom identifier which can later be 
              // used to cancel the notification, if you don't set one, a unique 
              // string will be generated automatically.
              Identifier = "one_day",
              Title = "",
              Body = "",
              Subtitle = "",
              ShowInForeground = true,
              ForegroundPresentationOption = (PresentationOption.Alert | PresentationOption.Sound),
              CategoryIdentifier = "category_a",
              ThreadIdentifier = "thread1",
              Trigger = dailyTimeTrigger,
            };

            iOSNotificationCenter.ScheduleNotification(dailyNotificationIOS);
#endif
        }
        else //cancel notifications when app is foreground
        {

#if UNITY_ANDROID
        AndroidNotificationCenter.CancelAllNotifications();
#elif UNITY_IOS
        var allNotifications = iOSNotificationCenter.GetScheduledNotifications();
        foreach (var n in allNotifications)
        {
            iOSNotificationCenter.RemoveScheduledNotification(n.Identifier);
        }
#endif
        }
        
    }
#endif

    void TestRequest(){
        //GetServiceResponse method has third parameter but it is default value is null
        //if you have request with parameters use third parameter
        //use strings for your keys and values
        Hashtable args = new Hashtable();
        args.Add("your key","your value");
        GetServiceResponse("Your URL", ProcessRespondText,args);
    }

    void ProcessRespondText(string text){
        //try catch is for a deserializing exception
        try
        {
            //use returned text whatever you want
            Debug.Log(text);
        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    public void GetServiceResponse(string url, Action<string> method, Hashtable args = null){
        
        WWWForm form = new WWWForm();
        if(args != null)
            foreach (DictionaryEntry item in args)
            {
                if(int.TryParse(item.Value.ToString(), out int result)){
                    form.AddField(item.Key.ToString(),result);
                }else{
                    form.AddField(item.Key.ToString(),item.Value.ToString());
                }
            }
        UnityWebRequest www = UnityWebRequest.Post(url,form);
        DownloadHandlerBuffer dh = new DownloadHandlerBuffer();
        www.downloadHandler = dh;
        www.timeout = 10;
        StartCoroutine(getServiceResponseCo(www,
        response => {
                if(!string.IsNullOrEmpty(response)){
                    method(response);
                }else{
                    Debug.Log("Network Error");
                }
            }));
    }

    IEnumerator getServiceResponseCo(UnityWebRequest www, Action<string> callback){
        yield return www.SendWebRequest();
        if (!www.isHttpError && !www.isNetworkError)
        {
            callback(www.downloadHandler.text);   
        }else{
            callback(null);
        }
    }

}
