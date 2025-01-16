using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Bullet : MonoBehaviour
{
    protected enum HitType
    {
        Spark,
        Blood,
        Ground
    }
    [Serializable]
    protected struct HitInfo
    {
        public HitType type;
        public AudioInfo audio;
        public GameObject vfx;
    }

    [SerializeField] 
    private float speed;
    [SerializeField] 
    private float maxLifetime=2;
    [SerializeField]
    protected float physicsPunchReduction = 0.2f;
    //[SerializeField]
    //public Vector2 tipOffset;
    [SerializeField]
    public LayerMask layersCanHit;
    [SerializeField]
    private float damage=10;//Only for enemy bullets
    [SerializeField]
    private float hitCheckDistance=5;
    [SerializeField]
    private HitInfo[] hitInfos;
    //[SerializeField]
    //private GameObject sparkHitVFX;
    //[SerializeField]
    //private GameObject bloodHitVFX;
    //[SerializeField]
    //private GameObject groundHitVFX;

    private bool _isFired;
    private Vector2 _shootFrom;
    private GameObject _vfxSpawned;
    private Vector2 direction;
    private Vector2 _velocity;
    private float punch;

    private void Start()
    {
        for (int i = 0; i < hitInfos.Length; i++)
        {
            if ((int)hitInfos[i].type!=i)
            {
                Utils.SwapInArray(i, (int)hitInfos[i].type, hitInfos);
            }
        }
        Main.inputManager.Game.OnRestartLevel.performed += OnRestartLevel;
        StartCoroutine(DestroyAfter(maxLifetime));
    }
    IEnumerator DestroyAfter(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }
    public void Fire(Vector2 direction,float punch)
    {
        _isFired = true;
        this.punch=punch;
        this.direction=direction.normalized;
        transform.rotation = Quaternion.Euler(0, 0, Utils.DirectionToAngle(direction));
        _velocity = direction * speed;
        _shootFrom = transform.position;
    }
    void BulletHit(RaycastHit2D hit)
    {
        Vector2 startPoint=transform.position;
        Vector2 pos = transform.position;
        Vector2 checkFrom = hit.point - _velocity.normalized * hitCheckDistance;
        RaycastHit2D checkHit = Physics2D.Raycast(checkFrom, (hit.point - checkFrom).normalized, hitCheckDistance + 0.1f, layersCanHit);
        //Debug.DrawLine(checkFrom, checkHit.point, Color.yellow,15);
        transform.position = pos - startPoint + checkHit.point;
        float angle=Utils.DirectionToAngle(hit.normal);
        if (hit.transform.CompareTag("Player")&&PlayerCharacter.S.isShiledActive|| hit.transform.CompareTag("Explosive"))
        {
            _vfxSpawned = Instantiate(hitInfos[(int)HitType.Spark].vfx, checkHit.point, Quaternion.Euler(0, 0, angle));
            AudioManager.S.PlayOnLocation(hitInfos[(int)HitType.Spark].audio, hit.point);
        }
        else if (hit.transform.CompareTag("Limb")|| hit.transform.CompareTag("Enemy") || hit.transform.CompareTag("Player"))
        {
            _vfxSpawned = Instantiate(hitInfos[(int)HitType.Blood].vfx, checkHit.point, Quaternion.Euler(0, 0, angle));
            AudioManager.S.PlayOnLocation(hitInfos[(int)HitType.Blood].audio, hit.point);
        }
        else
        { 
            _vfxSpawned = Instantiate(hitInfos[(int)HitType.Ground].vfx, checkHit.point, Quaternion.Euler(0, 0, angle));
            AudioManager.S.PlayOnLocation(hitInfos[(int)HitType.Ground].audio, hit.point);
        }
        transform.Find("Sprite").gameObject.SetActive(false);
        _isFired = false;
        Transform trail = transform.Find("Trail");
        if (!trail)
        {
            Destroy(gameObject);
            return;
        }
        StartCoroutine(DestroyAfter(trail.GetComponent<TrailRenderer>().time));
    }
    void OnDestroy()
    {
        //Debug.DrawLine(transform.position, _shootFrom,Color.magenta,15);
        Main.inputManager.Game.OnRestartLevel.performed -= OnRestartLevel;
    }
    void OnRestartLevel(InputAction.CallbackContext value)
    {
        Destroy(gameObject);
    }
    private void Update()
    {
        if (!_isFired)
            return;
        Vector2 startPoint = transform.position;
        float rayLength=speed *Time.deltaTime * 2;
        Debug.DrawRay(startPoint, _velocity.normalized * rayLength, Color.blue);
        RaycastHit2D hit = Physics2D.Raycast(startPoint, _velocity.normalized, rayLength, layersCanHit);
        if (!hit)
        { 
            transform.Translate(_velocity * Time.deltaTime, Space.World);
            return;
        }
        //Debug.Log($"Bullet hit {hit.transform.name}");
        switch (hit.transform.gameObject.tag)
        {
            case "Explosive":
                {
                    BulletHit(hit);
                    hit.transform.GetComponent<ExplosiveObject>().Explode(hit.transform.position);
                    break;
                }
            case "Enemy":
                {
                    BulletHit(hit);
                    EnemyCharacter enemyCharacter = hit.transform.GetComponent<EnemyCharacter>();
                    enemyCharacter.Kill(punch * physicsPunchReduction * direction);
                    break;
                }
            case "Glass":
                {
                    hit.transform.GetComponent<Glass>().BreakByRay(startPoint,rayLength,_velocity.normalized);
                    transform.Translate(_velocity * Time.deltaTime, Space.World);
                    break;
                }
            case "Player":
                {
                    BulletHit(hit);
                    if (!PlayerCharacter.S.isShiledActive)
                        PlayerCharacter.S.Damage(damage);
                    break;
                }
            default:
                {
                    BulletHit(hit);
                    Rigidbody2D rb = hit.transform.GetComponent<Rigidbody2D>();
                    if (rb && rb.bodyType == RigidbodyType2D.Dynamic)
                        rb.AddForce(direction * punch * physicsPunchReduction * 0.1f, ForceMode2D.Impulse);
                    break;
                }
        }
    }
}