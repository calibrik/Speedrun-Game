using System;
using UnityEngine;
using UnityEngine.InputSystem;


public class Main : MonoBehaviour
{
    public static Main S=>_S;
    private static Main _S;
    public static CustomInput inputManager => _inputManager;
    public bool isGameStarted =>_isGameStarted;

    public event Action OnLevelFinish;
    public event Action OnPlayerDeath;
    public event Action OnEnemyDied;
    public event Action OnAllEnemiesDied;

    private bool _isGameStarted = false;
    private static CustomInput _inputManager;
    [SerializeField]
    private float timeScale = 1;
    // Start is called before the first frame update
    void Awake()
    {
        Time.timeScale = timeScale;
        if (_inputManager == null)
        {
            _inputManager = new CustomInput();
            _inputManager.Enable();
        }
        
        if (!_S)
            _S = this;
        DisableInput();
        _isGameStarted = false;
        _inputManager.Game.OnGameStart.performed+=OnGameStart;
        _inputManager.Game.OnRestartLevel.performed+=OnRestartLevel;
    }
    public void PlayerDied()
    {
        _isGameStarted = false;
        DisableInput();
        OnPlayerDeath?.Invoke();
    }
    void OnGameStart(InputAction.CallbackContext value)
    {
        _isGameStarted = true;
        EnableInput();
        _inputManager.Game.OnGameStart.Disable();
    }

    void OnRestartLevel(InputAction.CallbackContext value)
    {
        _isGameStarted = false;
        Debug.ClearDeveloperConsole();
        DisableInput();
        _inputManager.Game.OnGameStart.Enable();
    }
    void DisableInput()
    {
        _inputManager.Player.Disable();
        _inputManager.Cheats.Disable();
    }

    void EnableInput()
    {
        _inputManager.Player.Enable();
        _inputManager.Cheats.Enable();
    }

    public void EnemyDied()
    {
        OnEnemyDied?.Invoke();
    }
    public void AllEnemiesDied()
    {
        OnAllEnemiesDied?.Invoke();
    }
    public void PLayerCrossedFinish()
    {
        _isGameStarted = false;
        DisableInput();
        OnLevelFinish?.Invoke();
    }
}
