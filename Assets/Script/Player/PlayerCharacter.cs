using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCharacter : Character
{
    public static PlayerCharacter S => _S;
    private static PlayerCharacter _S;
    public CharacterController2D controller=>_controller;
    public bool isShiledActive=>_shield.isActive;
    public bool isInIFrames=>_iFramesCoroutine!=null;
    [SerializeField]
    private float iFramesTime=0.1f;
    [SerializeField]
    private float maxHealth = 100;
    [SerializeField]
    private GameObject healthBarObj;

    private HealthBar _healthBar;
    private ShieldComponent _shield;
    private float _currHealth;
    private Coroutine _iFramesCoroutine=null;
    private CharacterController2D _controller;
    private GameObject _dynamicCapsuleCenter;
    private GameObject _iFramesBody;
    private GameObject _audioListener;

    public void Damage(float damage)
    {
        SetHealth(_currHealth-damage);
        if (_currHealth==0)
            Kill(Vector2.up * 5);
    }
    void SetHealth(float health)
    {
        _currHealth = health;
        if (_currHealth < 0)
        {
            _currHealth = 0;
        }
        _healthBar.SetBar(health/maxHealth);
    }
    IEnumerator ProccessIFrames()
    {
        _iFramesBody.SetActive(true);
        _capsuleCollider.enabled = false;
        int layerMask = LayerMask.GetMask("Glass");
        float enteredIFrames = Time.time;
        while (Time.time-enteredIFrames<iFramesTime)
        {
            float offsetY = 0;
            float offsetX = _controller.frameVelocity.x > 0 ? _capsuleCollider.size.x / 2 : -_capsuleCollider.size.x / 2;
            Vector2 dir =_controller.frameVelocity.x > 0 ? Vector2.right : Vector2.left;
            for (int i = 0; i < 3; i++) 
            {
                Vector2 startPos = _dynamicCapsuleCenter.transform.position + new Vector3(0, (_capsuleCollider.size.y / 2) - offsetY);
                float rayLength = Time.fixedDeltaTime * Mathf.Abs(_controller.frameVelocity.x) * 1.1f + offsetX;
                RaycastHit2D hit=Physics2D.Raycast(startPos,dir, rayLength,layerMask);
                //Debug.DrawRay(_dynamicCapsuleCenter.transform.position + new Vector3(0, (_capsuleCollider.size.y / 2) - offsetY),
                //    dir * (Time.fixedDeltaTime * Mathf.Abs(_controller.frameVelocity.x) * 1.1f+offsetX), Color.red,10);
                if (hit)
                    hit.transform.GetComponent<Glass>().BreakByRay(startPos,rayLength,dir);
                offsetY += _capsuleCollider.size.y/2;
            }
            offsetX = 0;
            offsetY = _controller.frameVelocity.y >= 0 ? _capsuleCollider.size.y / 2 : -_capsuleCollider.size.y / 2;
            dir = _controller.frameVelocity.y >= 0 ? Vector2.up : Vector2.down;
            for (int i = 0; i < 3; i++)
            {
                Vector2 startPos = _dynamicCapsuleCenter.transform.position + new Vector3((_capsuleCollider.size.x / 2) - offsetX, 0);
                float rayLength = Time.fixedDeltaTime * Mathf.Abs(_controller.frameVelocity.y) * 1.1f + offsetY;
                RaycastHit2D hit = Physics2D.Raycast(startPos, dir, rayLength, layerMask);
                //Debug.DrawRay(_dynamicCapsuleCenter.transform.position + new Vector3((_capsuleCollider.size.x / 2) - offsetX, offsetY),
                //    dir * (Time.fixedDeltaTime * Mathf.Abs(_controller.frameVelocity.y) * 1.1f+offsetY), Color.blue, 10);
                if (hit)
                    hit.transform.GetComponent<Glass>().BreakByRay(startPos,rayLength,dir);
                offsetX += _capsuleCollider.size.x / 2;
            }
            yield return null;
        }
        _iFramesBody.SetActive(false);
        _capsuleCollider.enabled = true;
        _iFramesCoroutine = null;
    }
    public void StartIFrames()
    {
        //isInIFrames = true;
        if (_iFramesCoroutine != null)
            StopCoroutine(_iFramesCoroutine);
        _iFramesCoroutine=StartCoroutine(ProccessIFrames());
    }
    protected override void Start()
    {
        if (_S!=null)
        {
            Destroy(gameObject);
            return;
        }
        _S = this;
        base.Start();
        _shield=GetComponentInChildren<ShieldComponent>();
        _healthBar=healthBarObj.GetComponent<HealthBar>();
        SetHealth(maxHealth);
        _dynamicCapsuleCenter = new GameObject("DynamicCenter");
        _dynamicCapsuleCenter.transform.parent = transform;
        _dynamicCapsuleCenter.transform.position = _capsuleCollider.bounds.center;
        _iFramesBody = transform.Find("IFramesBody").gameObject;
        _audioListener = new GameObject("AudioListener");
        _audioListener.AddComponent<AudioListener>();
        _controller = GetComponent<CharacterController2D>();
        _controller.Init();
        if (_animator)
        {
            _controller.OnLeftGround += () =>
            {
                _animator.SetBool("isInAir", true);
            };
            _controller.OnTouchedGround += () =>
            {
                _animator.SetBool("isInAir", false);
            };
            Main.inputManager.Player.OnMovement.performed += (InputAction.CallbackContext value) =>
            {
                _animator.SetBool("isMovementPerformed", true);
            };
            Main.inputManager.Player.OnMovement.canceled += (InputAction.CallbackContext value) =>
            {
                _animator.SetBool("isMovementPerformed", false);
            };
        }
        Main.inputManager.Player.OnFire.performed += OnFirePerformed;
        Main.inputManager.Player.OnFire.Disable();
        _controller.isEnabled = false;
        Main.inputManager.Game.OnGameStart.performed += (value) => 
        {
            _controller.isEnabled = true;
        };
    }
    public override void PickGun(Gun gun)
    {
        base.PickGun(gun);
        Main.inputManager.Player.OnFire.Enable();
    }
    void OnFirePerformed(InputAction.CallbackContext value)
    {
        if (_gun)
        {
            _gun.Fire();
            Main.inputManager.Player.OnFire.Disable();
            _controller.MovementRotation();
        }
    }
    private void Update()
    {
        _audioListener.transform.position = transform.position;
        if (!_isAlive||!HasGun)
            return;
        Vector2 mousePos = Utils.GetMousePosInWorld();
        Vector2 mousePosDirection = (mousePos - pos).normalized;
        if (mousePos.x < transform.position.x && transform.rotation.y == 0)
            transform.rotation = Quaternion.Euler(0, 180, 0);
        if (mousePos.x >= transform.position.x && transform.rotation.y != 0)
            transform.rotation = Quaternion.Euler(0,0,0);
        float angle = Utils.DirectionToAngle(mousePosDirection);
        angle = transform.localEulerAngles.y == 180 ? 180-angle : angle;
        _gun.transform.localEulerAngles = new Vector3(0,0,angle); 
    }

    public override void Kill(Vector2 punch)
    {
        base.Kill(punch);
        SetHealth(0);
        _controller.isEnabled = false;
        Main.S.PlayerDied();
    }
    protected override void OnRestartLevel(InputAction.CallbackContext value)
    {
        base.OnRestartLevel(value);
        SetHealth(maxHealth);
        RemoveGun();
        StopAllCoroutines();
        _controller.isEnabled = false;
        //_controller.ResetValues();
        Main.inputManager.Player.OnFire.Disable();
    }
}
