using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class EnemyCharacter : Character
{
    [SerializeField]
    private GameObject gunPreFab;
    [SerializeField]
    private float detectDistance = 20f;
    [SerializeField]
    private float burstDelay = 3;
    [SerializeField]
    private LayerMask checkLayers;

    private float _lastBurst;
    private GameObject _gunObj;
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        if (gunPreFab)
        {
            _gunObj= Instantiate(gunPreFab);
            PickGun(_gunObj.GetComponent<EnemyGun>());
            checkLayers |= (1 << LayerMask.NameToLayer("Player"));
        }
        _lastBurst = -burstDelay;
    }
    private void FixedUpdate()
    {
        if (!_isAlive || !HasGun)
            return;
        if ((PlayerCharacter.S.pos - pos).magnitude > detectDistance)
            return;

        if (PlayerCharacter.S.pos.x < transform.position.x && transform.rotation.y == 0)
            transform.rotation = Quaternion.Euler(0, 180, 0);
        if (PlayerCharacter.S.pos.x >= transform.position.x && transform.rotation.y != 0)
            transform.rotation = Quaternion.Euler(0, 0, 0);

        float angle = Utils.DirectionToAngle((PlayerCharacter.S.pos - pos).normalized);
        angle = transform.localEulerAngles.y == 180 ? 180 - angle : angle;
        _gun.transform.localEulerAngles = new Vector3(0, 0, angle);

        if (!Main.S.isGameStarted || Time.time - _lastBurst <= burstDelay)
            return;
        RaycastHit2D hit = Physics2D.Raycast(pos, (PlayerCharacter.S.pos - pos).normalized, detectDistance, checkLayers);
        if (hit && hit.transform.CompareTag("Player"))
        {
            _lastBurst = Time.time;
            _gun.Fire();
        }
    }
    //IEnumerator CheckPlayerPos()
    //{
    //    if ((PlayerCharacter.S.pos-pos).magnitude > detectDistance)
    //    {
    //        yield return new Wait
    //    }
    //}
    public override void Kill(Vector2 punch)
    {
        base.Kill(punch);
        if (_gunObj)
            _gunObj.GetComponent<Rigidbody2D>().AddForce(punch,ForceMode2D.Impulse);
        Main.S.EnemyDied();
    }
    protected override void OnRestartLevel(InputAction.CallbackContext value)
    {
        base.OnRestartLevel(value);
        if (_gunObj)
        {
            PickGun(_gunObj.GetComponent<Gun>());
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(pos,Vector2.right*detectDistance);
    }
}
