using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

public class Managers : MonoBehaviour
{
    #region core

    

    #endregion

    #region content

    

    #endregion
    
    private static Managers _instance;
    public static bool IsInit = false;
    public static Managers Instance
    {
        get
        {
            if (IsInit == false)
            {
                IsInit = true;
                if (_instance == null)
                {
                    GameObject go = GameObject.Find("@Managers");
                    if (go == null)
                    {
                        go = new GameObject { name = "@Managers" };
                        go.AddComponent<Managers>();
                    }
                    DontDestroyOnLoad(go);
                    _instance = go.GetComponent<Managers>();
                }
                InitForce();
            }
            return _instance;
        }
    }
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Init()
    {
        if (IsInit == false)
        {
            IsInit = true;
            if (Instance == null)
            {
                GameObject go = GameObject.Find("@Managers");
                if (go == null)
                {
                    go = new GameObject { name = "@Managers" };
                    go.AddComponent<Managers>();
                }

                DontDestroyOnLoad(go);
                _instance = go.GetComponent<Managers>();
            }

            InitForce();
        }
    }
    
    private static void InitForce()
    {
        //Managers Init
    }

    private DateTime _lastPauseTime = DateTime.MinValue;
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            Debug.Log("앱이 일시 중지됨 (백그라운드)");
            _lastPauseTime = DateTime.Now;
        }
        else
        {
            Debug.Log("앱이 재개됨 (포그라운드)");
            if (_lastPauseTime != DateTime.MinValue)
            {
                TimeSpan elapsedTime = DateTime.Now - _lastPauseTime;
                Debug.Log($"앱이 일시 중지된 시간: {elapsedTime.TotalSeconds}초");
            }
        }
    }
    
    
}
