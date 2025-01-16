using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class EnemyCounter : MonoBehaviour
{
    private int _enemiesAmount;
    private int _enemyCount;
    private TextMeshProUGUI _label;
    // Start is called before the first frame update
    void Start()
    {
        _enemiesAmount = GameObject.FindGameObjectsWithTag("Enemy").Length;
        _enemyCount = _enemiesAmount;
        _label=GetComponent<TextMeshProUGUI>();
        _label.color = Color.red;
        _label.text=$"{_enemyCount}/{_enemiesAmount} enemies left";
        Main.S.OnEnemyDied += OnEnemyDied;
        Main.inputManager.Game.OnRestartLevel.performed += OnRestartLevel;
    }
    void OnRestartLevel(InputAction.CallbackContext value)
    {
        _enemyCount = _enemiesAmount;
        _label.color = Color.red;
        _label.text = $"{_enemyCount}/{_enemiesAmount} enemies left";
    }
    void OnEnemyDied()
    {
        if (--_enemyCount>0)
        {
            _label.text = $"{_enemyCount}/{_enemiesAmount} enemies left";
            return;
        }
        _label.color=Color.green;
        _label.text = "All enemies are dead!";
        Main.S.AllEnemiesDied();
    }
}
