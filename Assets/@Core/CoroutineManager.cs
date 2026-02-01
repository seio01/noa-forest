using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineManager
{
    private MonoBehaviour _mono;

    private void Init()
    {
        // TODO Managers.Instance 주입
        // _mono = 
    }

    public Coroutine StartCoroutine(IEnumerator coroutine)
    {
        if (_mono == null)
        {
            Init();
        }
        return _mono.StartCoroutine(coroutine);
    }

    public Coroutine StartDelayed(float seconds, IEnumerator routine)
    {
        return StartCoroutine(DelayThen(seconds, routine));
    }

    private IEnumerator DelayThen(float seconds, IEnumerator routine)
    {
        if (seconds > 0f)
        {
            yield return new WaitForSeconds(seconds);
        }

        yield return routine;
    }

    public void StopCoroutine(IEnumerator coroutine)
    {
        _mono?.StopCoroutine(coroutine);
    }
}

