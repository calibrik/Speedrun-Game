using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Gun : MonoBehaviour
{
    public Rigidbody2D rb => _rb;
    public Transform lArmGripPoint => _lArmGripPoint;
    public Transform rArmGripPoint=>_rArmGripPoint;
    public Character owner
    {
        set { _owner = value; }
    }
        
    public Vector2 posInOwner;
    [SerializeField]
    protected GameObject bulletPrefab;
    [SerializeField]
    protected float punch;
    [SerializeField]
    protected float torqueSpeedAfterShot = 200f;
    [SerializeField]
    protected AudioInfo audioInfo;

    //protected AudioSource _audioSource;
    protected Character _owner;
    protected Transform _gunPoint;
    protected Transform _lArmGripPoint, _rArmGripPoint;
    //protected Quaternion _startRot;
    protected Rigidbody2D _rb;
    protected BoxCollider2D[] _colliders;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        _colliders=GetComponents<BoxCollider2D>();
        foreach (BoxCollider2D collider in _colliders)
        {
            collider.enabled = false;
        }
        //_audioSource = GetComponent<AudioSource>();
    }
    //protected void SpawnSound()
    //{
    //    IEnumerator DestroyAfter(GameObject go,float time)
    //    {
    //        yield return new WaitForSeconds(time);
    //        Destroy(go);
    //    }

    //    GameObject sound=new GameObject($"{transform.name}ShotSound");
    //    if (_gunPoint)
    //        sound.transform.position = _gunPoint.position;
    //    else
    //        sound.transform.position = transform.position;
    //    AudioSource source= Utils.CopyComponent(_audioSource,sound);
    //    source.Play();
    //    StartCoroutine(DestroyAfter(sound, source.clip.length));
    //}
    public virtual void Drop()
    {
        transform.parent = null;
        _owner.RemoveGun();
        _owner = null;
        foreach (BoxCollider2D collider in _colliders)
        {
            collider.enabled = true;
        }
        _rb.bodyType = RigidbodyType2D.Dynamic;
        _rb.AddTorque(torqueSpeedAfterShot);
    }
    protected virtual Vector2 CalculateShotDirection()
    {
        return Vector2.zero;
    }
    protected void SpawnBullet(Vector2 direction)
    {
        if (!_gunPoint)
            return;
        Vector2 gunPointPos = _gunPoint.position;
        IsInsideSmth(ref gunPointPos);
        GameObject bullet = Instantiate(bulletPrefab, gunPointPos, Quaternion.identity);
        bullet.GetComponent<Bullet>().Fire(direction,punch);
    }
    protected virtual bool IsInsideSmth(ref Vector2 outPos)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, (_gunPoint.position - transform.position).normalized, (_gunPoint.position - transform.position).magnitude, bulletPrefab.GetComponent<Bullet>().layersCanHit);
        if (hit)
        {
            Vector2 pos=transform.position;
            outPos = hit.point;//-0.01f* (Vector2)(_gunPoint.position - transform.position).normalized;
            return true;
        }
        return false;
    }
    public virtual void Fire()
    {
        
    }
    protected virtual void OnEnable()
    {
        _rb = GetComponent<Rigidbody2D>();
        _lArmGripPoint = transform.Find("LArmGripPoint");
        _rArmGripPoint = transform.Find("RArmGripPoint");
        _gunPoint = transform.Find("GunPoint");
        //_startRot = transform.rotation;
        Main.inputManager.Game.OnRestartLevel.performed += OnRestartLevel;
    }

    protected virtual void OnRestartLevel(InputAction.CallbackContext value)
    {
        foreach (BoxCollider2D collider in _colliders)
        {
            collider.enabled = true;
        }
        _rb.bodyType = RigidbodyType2D.Kinematic;
        _rb.linearVelocity = Vector2.zero;
        _rb.angularVelocity = 0;
        transform.rotation = Quaternion.identity;
    }
}
