using UnityEngine;
using UnityEngine.InputSystem;

public class Character : MonoBehaviour
{
    public enum SoundType
    {
        Step
    }
    [System.Serializable]
    protected struct SoundInfo
    {
        public SoundType type;
        public AudioInfo audioInfo;
    }

    public Vector2 pos => transform.position;
    [SerializeField]
    protected SoundInfo[] soundInfos;
    public bool HasGun => _gun != null;
    protected Animator _animator;
    protected CapsuleCollider2D _capsuleCollider;
    protected Transform _lArm, _rArm;
    protected Gore _goreComponent;
    protected Vector2 _startPos;
    protected bool _isAlive = true;
    protected Gun _gun = null;
    // Start is called before the first frame update

    public void PlaySoundOnCharacter(SoundType type)
    {
        AudioManager.S.PlayOnLocation(soundInfos[(int)type].audioInfo, pos);
    }
    protected void SetArmOnGripPoint(Transform armWrapper, Transform gripPoint)
    {
        Transform arm = armWrapper.GetChild(0);
        armWrapper.localPosition = gripPoint.localPosition - arm.localPosition;
        float targetAngle = gripPoint.localEulerAngles.z - arm.localEulerAngles.z;
        Vector2 armToNewPos = Utils.RotateVector(-arm.localPosition, targetAngle);
        armWrapper.localPosition += arm.localPosition + new Vector3(armToNewPos.x, armToNewPos.y, 0);
        armWrapper.localEulerAngles = new Vector3(0, 0, targetAngle);
    }
    public virtual void PickGun(Gun gun)
    {
        _animator?.SetBool("isArmed", true);
        gun.transform.parent = transform;
        gun.transform.localPosition = gun.posInOwner;
        _gun = gun;
        _gun.owner = this;
        if (transform.rotation.y != 0)
            gun.transform.rotation = Quaternion.Euler(0, 180, 0);
        _lArm.parent = gun.transform;
        _rArm.parent = gun.transform;
        SetArmOnGripPoint(_lArm, gun.lArmGripPoint);
        SetArmOnGripPoint(_rArm, gun.rArmGripPoint);
    }
    private Transform WrapAnimatedLimb(Transform limb)
    {
        GameObject wrapper = new GameObject($"{limb.name}Wrapper");
        wrapper.tag = limb.tag;
        wrapper.transform.parent = transform;
        wrapper.transform.localPosition = Vector3.zero;
        limb.parent = wrapper.transform;
        return wrapper.transform;
    }
    protected virtual void Start()
    {
        for (int i = 0; i < soundInfos.Length; i++)
        {
            if ((int)soundInfos[i].type != i)
            {
                Utils.SwapInArray(i, (int)soundInfos[i].type, soundInfos);
            }
        }
        _capsuleCollider = GetComponent<CapsuleCollider2D>();
        _goreComponent = GetComponent<Gore>();
        _isAlive = true;
        _startPos = transform.position;
        _lArm = transform.Find("LArm");
        _rArm = transform.Find("RArm");
        _lArm = WrapAnimatedLimb(_lArm);
        _rArm = WrapAnimatedLimb(_rArm);
        _animator = GetComponent<Animator>();
        Main.inputManager.Game.OnRestartLevel.performed += OnRestartLevel;
    }
    protected virtual void OnRestartLevel(InputAction.CallbackContext value)
    {
        _isAlive = true;
        transform.SetPositionAndRotation(_startPos, Quaternion.identity);
    }
    public virtual void Kill(Vector2 punch)
    {
        if (!_isAlive)
            return;
        _isAlive = false;
        _gun?.Drop();
        _goreComponent.ApplyGore(punch);
    }
    public void RemoveGun()
    {
        if (!HasGun)
            return;
        _animator?.SetBool("isArmed", false);
        _gun = null;
        _lArm.parent = transform;
        _rArm.parent = transform;
        _lArm.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        _rArm.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
    }
}
