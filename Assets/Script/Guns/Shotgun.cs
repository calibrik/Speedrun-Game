

using UnityEngine;

public class Shotgun : PlayerGun
{
    [SerializeField] private int pelletsAmount=5;
    [SerializeField] private float spreadAngle = 90;

    public override void Fire()
    {
        Vector2 direction = CalculateShotDirection();
        PlayerCharacter.S.controller.AddForce(-direction*punch,true);
        PlayerCharacter.S.StartIFrames();
        Vector2 leftSideVector=Utils.RotateVector(direction,spreadAngle/2);
        float turnAngle = spreadAngle/pelletsAmount;
        float currAngle = spreadAngle;
        //SpawnSound();
        AudioManager.S.PlayOnLocation(audioInfo, _gunPoint.position);
        for (int i = 0; i < pelletsAmount; i++)
        {
            SpawnBullet(Utils.RotateVector(leftSideVector,-currAngle));
            currAngle -= turnAngle;
        }
        Drop();
    }
}
