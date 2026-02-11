using System;
using UnityEngine;

public class Managers : MonoBehaviour
{
    #region core
    private readonly CoroutineManager _coroutine = new CoroutineManager();
    private readonly UIManager _ui = new UIManager();
    private readonly ResourceManager _resource = new ResourceManager();
    public static CoroutineManager Coroutine => Instance._coroutine;
    public static UIManager UI => Instance._ui;
    public static ResourceManager Resource => Instance._resource;
    #endregion

    #region content

    

    #endregion
    
    private static Managers _instance;
    public static Managers Instance
    {
        get
        {
            if(_instance == null)
            {
                Init();
            }
            return _instance;
        }
    }
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Init()
    {
        if(_instance != null) return;

        GameObject go = GameObject.Find("@Managers");
        if (go == null)
        {
            go = new GameObject { name = "@Managers" };
        }
        _instance = go.GetComponent<Managers>();
        if(_instance == null)
        {
            _instance = go.AddComponent<Managers>();
        }

        DontDestroyOnLoad(go);
        InitForce();
    }
    
    private static void InitForce()
    {
        //Managers Init
        _instance._ui.Init();
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
