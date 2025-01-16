using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerGun : Gun
{
    protected Vector2 _startPos;
    public GunPickup GunPickup
    {
        set => _gunPickup = value;
    }

    protected GunPickup _gunPickup;
    public override void Drop()
    {
        base.Drop();
    }
    protected override Vector2 CalculateShotDirection()
    {
        Vector2 mousePos = Utils.GetMousePosInWorld();
        return (mousePos - PlayerCharacter.S.pos).normalized;
    }
    public override void Fire()
    {
        Vector2 direction = CalculateShotDirection();
        PlayerCharacter.S.controller.AddForce(-direction * punch, true);
        SpawnBullet(direction);
        //_audioSource.Play();
        Drop();
    }
    protected override void OnEnable()
    {
        base.OnEnable();
        _startPos = transform.position;
    }
    protected override void OnRestartLevel(InputAction.CallbackContext value)
    {
        Main.inputManager.Game.OnRestartLevel.performed -= OnRestartLevel;
        if (!_gunPickup)
        {
            Destroy(gameObject);
            return;
        }

        base.OnRestartLevel(value);
        transform.position = _startPos;
        transform.parent = _gunPickup.transform;
        _gunPickup.gameObject.SetActive(true);
        gameObject.SetActive(false);
    }
}
