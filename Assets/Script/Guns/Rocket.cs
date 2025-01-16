using System.Collections;
using UnityEngine;

public class Rocket : ExplosiveObject
{
    public Vector2 tip => tipOffset;

    [SerializeField]
    private float speed;
    [SerializeField] 
    private float maxLifetime=2;
    [SerializeField]
    private Vector2 tipOffset;
    [SerializeField]
    public LayerMask layersCanHit;
    [SerializeField]
    private float hitCheckDistance=5;

    private bool _isFired = false;
    private Vector2 _velocity;
    public void Fire(Vector2 direction)
    {
        transform.rotation = Quaternion.Euler(0, 0, Utils.DirectionToAngle(direction));
        _velocity = direction * speed;
        _isFired = true;
        StartCoroutine(OnTimeout());
    }
    IEnumerator OnTimeout()
    {
        yield return new WaitForSeconds(maxLifetime);
        Explode(transform.position);
    }
    public override void Explode(Vector2 explosionPoint, float additionalPunch=0, bool isOverride = false)
    {
        if (_isExploded)
            return;
        _isFired = false;
        _isExploded = true;
        //Vector2 explosionPoint=transform.position;
        gameObject.SetActive(false);
        //_velocity = Vector2.zero;
        SpawnVFX();
        AudioManager.S.PlayOnLocation(audioInfo, explosionPoint);
        bool isExplosiveHit = false;
        RaycastHit2D[] hits=Physics2D.CircleCastAll(explosionPoint,explosionRadius,Vector2.up,0.001f,layersCanExplode);
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.transform.CompareTag("Explosive"))
            {
                hit.transform.gameObject.GetComponent<ExplosiveObject>().Explode(hit.transform.position, punch, true);
                isExplosiveHit = true;
            }
        }
        foreach (RaycastHit2D hit in hits)
        {
            RaycastHit2D rayHit = GetAccurateHit(hit,explosionPoint);
            if (!rayHit||isBlocked(explosionPoint, rayHit))
                continue;
            switch (hit.transform.gameObject.tag)
            {
                case "Player":
                    {
                        //Time.timeScale = 0;
                        //Debug.DrawLine(explosionPoint, rayHit.point, Color.yellow, 10);
                        if (!isExplosiveHit)
                        {
                            AddForceToPlayer(explosionPoint, rayHit.point, punch, true);
                        }
                        break;
                    }
                case "Enemy":
                    {
                        hit.transform.GetComponent<EnemyCharacter>().Kill((rayHit.point-explosionPoint).normalized*punch*physicsPunchReduction);
                        break;
                    }
                case "Glass":
                    {
                        hit.transform.GetComponent<Glass>().BreakByExplosion(explosionPoint,explosionRadius);
                        break;
                    }
                default:
                    {
                        AddForceToObject(explosionPoint, hit.transform.gameObject, punch);
                        break;
                    }
            }
        }
    }
    private void Update()
    {
        Vector2 startPoint = transform.position;
        if (_isFired)
        {
            startPoint = transform.TransformPoint(tipOffset);
            Debug.DrawRay(startPoint, _velocity.normalized * (speed * Time.deltaTime*2), Color.blue);
            RaycastHit2D hit = Physics2D.Raycast(startPoint, _velocity.normalized, speed * Time.deltaTime*3, layersCanHit);
            if (hit)
            {
                Vector2 pos = transform.position;
                Vector2 checkFrom= hit.point-_velocity.normalized*hitCheckDistance;
                RaycastHit2D hitFromShootPoint=Physics2D.Raycast(checkFrom,(hit.point-checkFrom).normalized,hitCheckDistance+0.1f,layersCanHit);
                //Debug.DrawLine(checkFrom, hitFromShootPoint.point, Color.red, 15);
                //Debug.Log($"Rocket hit {hit.transform.name}");
                transform.position = pos - startPoint + hitFromShootPoint.point;
                Explode((Vector2)transform.TransformPoint(tipOffset)-_velocity.normalized * 0.01f);
                return;
            }
            transform.Translate(_velocity * Time.deltaTime, Space.World);
        }
    }
    private void OnDestroy()
    {
        foreach (GameObject vfx in _vfxs)
        {
            Destroy(vfx);
        }
    }
    public void OnRestartLevel()
    {
        gameObject.SetActive(true);
        _isExploded = false;
        _isFired = false;
        RestartVFX();
    }
    protected override void OnDrawGizmos()
    {
        Vector3 explosionPoint = transform.TransformPoint(tipOffset);
        Vector3 dir1 = Utils.RotateVector(Vector2.up, verticalAngle / 2);
        Vector3 dir2 = Utils.RotateVector(Vector2.up, -verticalAngle / 2);
        Gizmos.color = Color.green;
        Gizmos.DrawRay(explosionPoint + dir1 * deathDistance, dir1 * (explosionRadius - deathDistance));
        Gizmos.DrawRay(explosionPoint + dir2 * deathDistance, dir2 * (explosionRadius - deathDistance));
    }
}
