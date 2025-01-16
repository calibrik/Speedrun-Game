using System;
using UnityEngine;
using UnityEngine.InputSystem;

enum UIState:UInt16
{
    Start,
    Death,
    Finish,
    Running
}

public class UIManger : MonoBehaviour
{
    public static UIManger S => _S;
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject timer;
    [SerializeField] private GameObject finishText;
    [SerializeField] private GameObject killConfirmPanel;
    [SerializeField] private GameObject enemyCount;
    [SerializeField] private GameObject healthBar;

    private Animator _animator;
    private UIState state
    {
        set { SetState(value); }
        get { return _state; }
    }
    private UIState _state;
    private static UIManger _S;
    private Timer _timerComponent;
    // Start is called before the first frame update
    void Start()
    {
        if (S)
            return;
        _S = this;
        _animator = GetComponent<Animator>();
        _timerComponent = timer.GetComponent<Timer>();
        SetState(UIState.Start,false);
        Main.inputManager.Game.OnGameStart.performed += OnGameStart;
        Main.inputManager.Game.OnRestartLevel.performed += OnRestartLevel;
        Main.S.OnLevelFinish += OnLevelFinish;
        Main.S.OnPlayerDeath += OnPlayerDeath;
        Main.S.OnEnemyDied += () =>
        {
            _animator.Play("KillConfirmation",0,0);
        };
    }
    void SetState(UIState state, bool processPast = true)
    {
        if (processPast&&state == _state)
            return;
        if (processPast)
        {
            switch (_state)
            {
                case UIState.Start:
                    {
                        startPanel.SetActive(false);
                        break;
                    }
                case UIState.Finish:
                    {
                        healthBar.SetActive(false);
                        finishText.SetActive(false);
                        timer.SetActive(false);
                        break;
                    }
                case UIState.Death:
                    {
                        healthBar.SetActive(false);
                        finishText.SetActive(false);
                        timer.SetActive(false);
                        break;
                    }
                case UIState.Running:
                    {
                        healthBar.SetActive(false);
                        _animator.SetBool("EndAnim", true);
                        enemyCount.SetActive(false);
                        killConfirmPanel.SetActive(false);
                        _timerComponent.StopTimer();
                        timer.SetActive(false);
                        break;
                    }
            }
        }
        else
        {
            _animator.SetBool("EndAnim",true);
            enemyCount.SetActive(false);
            killConfirmPanel.SetActive(false);
            _timerComponent.StopTimer();
            timer.SetActive(false);
            finishText.SetActive(false);
            startPanel.SetActive(false);
            healthBar.SetActive(false);
        }
        switch (state)
        {
            case UIState.Start:
                {
                    startPanel.SetActive(true);
                    break;
                }
            case UIState.Finish:
                {
                    healthBar.SetActive(true);
                    finishText.SetActive(true);
                    timer.SetActive(true);
                    break;
                }
            case UIState.Death:
                {
                    healthBar.SetActive(true);
                    finishText.SetActive(true);
                    timer.SetActive(true);
                    break;
                }
            case UIState.Running:
                {
                    healthBar.SetActive(true);
                    _animator.SetBool("EndAnim", false);
                    enemyCount.SetActive(true);
                    timer.SetActive(true);
                    _timerComponent.StartTimer();
                    break;
                }
        }
        _state = state;
    }
    void OnPlayerDeath()
    {
        state=UIState.Death;
    }
    void OnGameStart(InputAction.CallbackContext value)
    {
        state=UIState.Running;
    }
    void OnLevelFinish()
    {
        state = UIState.Finish;
    }
    void OnRestartLevel(InputAction.CallbackContext value)
    {
        state = UIState.Start;
    }
}
