using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    // private bool _isRunning = false;
    private float _timeStarted = 0;
    private bool _isRunning = false;
    private TextMeshProUGUI _timerText;
    // Start is called before the first frame update
    void Start()
    {
        _timerText = GetComponent<TextMeshProUGUI>();
    }
    public void StartTimer()
    {
        _timeStarted = Time.time;
        _isRunning = true;
    }

    public void StopTimer()
    {
        _isRunning = false;
    }
    // Update is called once per frame
    void Update()
    {
        if (!_isRunning)
            return;
        float time=Time.time - _timeStarted;
        int milliseconds = (int)(time * 1000 % 1000);
        int seconds = (int)(time % 60);
        int minutes = (int)(time / 60);
        _timerText.text = $"{minutes}:{seconds:00}:{milliseconds:000}";
    }
}
