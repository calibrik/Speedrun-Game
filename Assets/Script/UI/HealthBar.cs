using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    private Slider _bar;
    // Start is called before the first frame update
    void Awake()
    {
        _bar = GetComponent<Slider>();
    }
    public void SetBar(float value)
    {
        _bar.value = value;
    }
}
