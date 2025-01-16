using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class CharacterController2D : MonoBehaviour
{
    public Vector2 frameVelocity => _frameVelocity;
    public event Action<Vector2> OnMove;
    public event Action OnLeftGround;
    public event Action OnTouchedGround;
    public bool isEnabled
    {
        get { return _isEnabled;}
        set
        {
            _isEnabled = value;
            if (!value)
                ResetValues();
        }
    }
    
    [SerializeField]
    private float groundAcceleration=10;
    [SerializeField]
    private float airAcceleration=10;
    [SerializeField]
    private float groundDeceleration=10;
    // private float airDeceleration=10;
    [SerializeField]
    private float airVerticalDeceleration=20;
    [SerializeField]
    private float airHorizontalDeceleration=10;
    [SerializeField]
    private float maxGroundSpeed=20;
    [SerializeField]
    private float maxAirSpeed=20;
    [SerializeField]
    private float jumpPower=20;
    [SerializeField]
    private float gravity = 9.8f;
    [SerializeField]
    private float maxFallingSpeed;
    [SerializeField]
    private float jumpBufferTime = 0.1f;
    [SerializeField]
    private float offsetForCollisionCheck = 0.5f;
    [SerializeField]
    private LayerMask collisionCheckLayers;
    // [SerializeField] private GameObject circleDebug;    
    
    [Header("DO NOT EDIT")]
    [SerializeField]
    private Vector2 _frameVelocity;
    [SerializeField]
    private bool _isGrounded;
    [SerializeField]
    private bool _isCeiled;
    private CapsuleCollider2D _collider;
    //private BoxCollider2D _collider;
    private Rigidbody2D _rb;
    private float _movementInput;
    private bool _cachedQueriesInCollider;
    // private bool _isJumpInputted;
    private Vector2 _externalForce;
    private Vector2 _externalForceOverride;
    private float _jumpInputTime=-1;
    private bool _isJumping=false;
    private bool _isEnabled;
    // Start is called before the first frame update
    public void Init()
    {
        _rb = GetComponent<Rigidbody2D>();
        _cachedQueriesInCollider = Physics2D.queriesStartInColliders;
        _collider = GetComponent<CapsuleCollider2D>();
        //_collider = GetComponent<BoxCollider2D>();
        ResetValues();
        Main.inputManager.Player.OnMovement.performed += OnMovementPerformed;
        Main.inputManager.Player.OnMovement.canceled += OnMovementCancelled;
        Main.inputManager.Player.OnJump.performed += OnJumpPerformed;
    }
    void CollisionCheck()
    {
        Physics2D.queriesStartInColliders = true;
        //int layerMask = LayerMask.GetMask("Default");
        // GameObject circle= Instantiate(circleDebug, _collider.bounds.center+new Vector3(0,_collider.bounds.extents.y-offsetForCollisionCheck,0),Quaternion.identity);
        // circle.transform.localScale = new Vector2(_collider.bounds.size.x, _collider.bounds.size.x);
        // _isCeiled = Physics2D.CircleCast(_collider.bounds.center+new Vector3(0,_collider.bounds.extents.y-offsetForCollisionCheck,0), _collider.bounds.size.x,
        // Vector2.up, 0.2f, layerMask);

        _isCeiled = Physics2D.CapsuleCast(_collider.bounds.center + new Vector3(0, offsetForCollisionCheck, 0), _collider.bounds.size * 0.8f, _collider.direction, 0,Vector2.up, 0.01f, collisionCheckLayers);
        //_isCeiled = Physics2D.BoxCast(_collider.bounds.center + new Vector3(0, offsetForCollisionCheck, 0), _collider.size * 0.8f, 0, Vector2.up, 0.01f,layerMask);
        bool groundHit = Physics2D.CapsuleCast(_collider.bounds.center - new Vector3(0, offsetForCollisionCheck, 0), _collider.bounds.size * 0.8f, _collider.direction, 0, Vector2.down, 0.01f, collisionCheckLayers);
        //bool groundHit= Physics2D.BoxCast(_collider.bounds.center - new Vector3(0, offsetForCollisionCheck, 0), _collider.size * 0.8f, 0, Vector2.down, 0.01f,layerMask);
        if (PlayerCharacter.S.isInIFrames)
        {
            _isCeiled = groundHit = false;
        }
        if (groundHit && !_isGrounded)
            OnTouchedGround?.Invoke();
        if (!groundHit&&_isGrounded)
            OnLeftGround?.Invoke();
        _isGrounded = groundHit;
        Physics2D.queriesStartInColliders = _cachedQueriesInCollider;
    }
    private void ResetValues()
    {
        if (!_rb)
            Init();
        _isGrounded = _isCeiled = _isJumping = false;
        _externalForce = Vector2.zero;
        _externalForceOverride = Vector2.zero;
        _frameVelocity=Vector2.zero;
        _rb.linearVelocity = Vector2.zero;
    }
    void HandleHorizontalMovement()
    {
        float maxSpeed = _isGrounded ? maxGroundSpeed : maxAirSpeed;
        maxSpeed *= _movementInput;
        float acceleration = _isGrounded ? groundAcceleration : airAcceleration;
        if (_movementInput == 0||Mathf.Abs(_frameVelocity.x) > Mathf.Abs(maxSpeed))
        {
            if (_movementInput < 0 && _frameVelocity.x > 0 || _movementInput > 0 && _frameVelocity.x < 0)
            {
                _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, maxSpeed, acceleration * Time.fixedDeltaTime);
                return;
            }
            float deceleration=(_isGrounded||_isCeiled)&&!_isJumping ? groundDeceleration : airHorizontalDeceleration;
            _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, 0, deceleration * Time.fixedDeltaTime);
            return;
        }
        _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, maxSpeed, acceleration * Time.fixedDeltaTime);
    }

    void HandleVerticalMovement()
    {
        if (_isGrounded&&Time.time - _jumpInputTime <= jumpBufferTime)
        {
            _frameVelocity.y = jumpPower;
            _isJumping = true;
            return;
        }
        if (_frameVelocity.y < -maxFallingSpeed)
            _frameVelocity.y = Mathf.MoveTowards(_frameVelocity.y, -maxFallingSpeed, airVerticalDeceleration * Time.fixedDeltaTime);
        else
            _frameVelocity.y = Mathf.MoveTowards(_frameVelocity.y, -maxFallingSpeed, gravity * Time.fixedDeltaTime);
    }

    //void MousePosCheck()
    //{
    //    if (!PlayerCharacter.S.HasGun)
    //        return;
    //    Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    //    if (mousePos.x<transform.position.x && transform.rotation.y == 0)
    //        transform.rotation = Quaternion.Euler(0,180,0);
    //    if (mousePos.x>=transform.position.x && transform.rotation.y !=0)
    //        transform.rotation = Quaternion.Euler(0,0,0);
    //}
    void FixedUpdate()
    {
        if (!isEnabled) return;
        _frameVelocity = _rb.linearVelocity;
        ApplyExternalForce();
        CollisionCheck();
        HandleVerticalMovement();
        HandleHorizontalMovement();
        //MousePosCheck();
        _isJumping = false;
        OnMove?.Invoke(_frameVelocity);
        _rb.linearVelocity = _frameVelocity;
    }
    private void ApplyExternalForce()
    {
        if (_externalForceOverride != Vector2.zero)
        {
            //Debug.Log(Vector2.Angle(_externalForceOverride, _frameVelocity));
            //if (Vector2.Angle(_externalForceOverride, _frameVelocity) <= angleToTreatForce / 2 && _frameVelocity.magnitude > _externalForceOverride.magnitude)
            //    _frameVelocity += _externalForceOverride;
            _frameVelocity = _externalForceOverride;
        }
        _frameVelocity +=_externalForce;
        _externalForceOverride = Vector2.zero;
        _externalForce = Vector2.zero;
    }
    void OnMovementCancelled(InputAction.CallbackContext value)
    {
        _movementInput = 0;
    }
    void OnMovementPerformed(InputAction.CallbackContext value)
    {
        _movementInput = value.ReadValue<float>();
        MovementRotation();
    }

    public void MovementRotation()
    {
        if (PlayerCharacter.S.HasGun)
            return;
        if (_movementInput < 0 && transform.rotation.y == 0)
            transform.rotation = Quaternion.Euler(0,180,0);
        if (_movementInput > 0 && transform.rotation.y !=0)
            transform.rotation = Quaternion.Euler(0,0,0);
    }
    void OnJumpPerformed(InputAction.CallbackContext value)
    {
        _jumpInputTime = Time.time;
    }

    public void AddForce(Vector2 force, bool isOverride)
    {
        if (!isEnabled)
            return;
        //Debug.Log($"Force {force} {isOverride}");
        if (isOverride)
            _externalForceOverride += force;
        else
            _externalForce += force;
    }
}
