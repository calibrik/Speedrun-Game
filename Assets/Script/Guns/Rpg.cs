using UnityEngine;
using UnityEngine.InputSystem;

public class Rpg: PlayerGun
{
    private Rocket _rocket;
    private Vector2 _rocketLocalPos;
    private Quaternion _rocketLocalRot;
    protected override void Start()
    {
        base.Start();
        _rocket = GetComponentInChildren<Rocket>();
        _rocketLocalPos = _rocket.transform.localPosition;
        _rocketLocalRot = _rocket.transform.localRotation;
    }
    protected override bool IsInsideSmth(ref Vector2 outPos) 
    {
        Vector2 pos=transform.position;
        Vector2 tipPos = _rocket.transform.TransformPoint(_rocket.tip);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, (tipPos - pos).normalized, (tipPos - pos).magnitude, _rocket.layersCanHit);
        if (hit)
        {
            outPos = hit.point;//-(tipPos - pos).normalized*0.01f;
            return true;
        }
        return false;
    }
    public override void Fire()
    {
        Vector2 direction = CalculateShotDirection();
        //PlayerCharacter.S.controller.AddForce(direction*punch,true);
        Vector2 gunPointPos = Vector2.zero; ;
        if (IsInsideSmth(ref gunPointPos))
        {
            Vector2 pos = _rocket.transform.position;
            Vector2 tipPos = _rocket.transform.TransformPoint(_rocket.tip);
            _rocket.transform.position = pos - tipPos + gunPointPos;
        }
        _rocket.transform.parent = null;
        _rocket.Fire(direction);
        Drop();
    }
    
    protected override void OnRestartLevel(InputAction.CallbackContext value)
    {
        base.OnRestartLevel(value);
        if (!_gunPickup)
        {
            Destroy(_rocket.gameObject);
            return;
        }
        _rocket.OnRestartLevel();
        _rocket.transform.parent = transform;
        _rocket.transform.localPosition = _rocketLocalPos;
        _rocket.transform.localRotation = _rocketLocalRot;
    }
}
