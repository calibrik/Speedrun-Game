using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGun : Gun
{
    public LayerMask bulletHitMask=>bulletPrefab.GetComponent<Bullet>().layersCanHit;
    protected override Vector2 CalculateShotDirection()
    {
        return (PlayerCharacter.S.pos-((Vector2)_gunPoint.position)).normalized;
    }
}
