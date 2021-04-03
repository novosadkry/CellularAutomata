using System;
using UnityEngine;

public class ClockScript : MonoBehaviour
{
    public event Action OnTick;

    [Header("Properties")]
    public float tickDelta;
    public bool paused;
    
    private float _previousTick;

    private void Update()
    {
        if (paused)
            return;
        
        if (Time.time > _previousTick + tickDelta)
        {
            OnTick?.Invoke();
            _previousTick = Time.time;
        }
    }
}
