using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class M16 : EnemyGun
{
    [SerializeField]
    private int bulletsInBurstAmount = 3;
    [SerializeField]
    private float betweenBulletDelay = 0.1f;
    // Start is called before the first frame update
    IEnumerator BurstFire()
    {
        for (int i = 0; i < bulletsInBurstAmount; i++)
        {
            if (!_owner)
                yield break;
            SpawnBullet(CalculateShotDirection());
            AudioManager.S.PlayOnLocation(audioInfo, _gunPoint.position);
            yield return new WaitForSeconds(betweenBulletDelay);
        }
    }
    public override void Fire()
    {
        StartCoroutine(BurstFire());
    }
    protected override void OnRestartLevel(InputAction.CallbackContext value)
    {
        base.OnRestartLevel(value);
        StopAllCoroutines();
    }
}
