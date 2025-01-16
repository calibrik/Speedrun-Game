using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ExplosiveObject : MonoBehaviour
{
    public float punch;
    [SerializeField]
    private float secondaryDirectionPunchReducement=0.5f;
    [SerializeField] 
    protected float deathDistance;
    [SerializeField]
    protected float explosionRadius;
    [SerializeField] 
    protected float verticalAngle = 60;
    [SerializeField]
    protected float physicsPunchReduction = 0.2f;
    //[SerializeField]
    //protected GameObject explosionPrefab;
    [SerializeField]
    protected LayerMask layersCanExplode;
    [SerializeField]
    protected AudioInfo audioInfo;
    protected bool _isExploded = false;
    protected Vector2 _pos => transform.position;
    protected List<GameObject> _vfxs;

    protected void Start()
    {
        _vfxs = new List<GameObject>();
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child= transform.GetChild(i);
            if (child.CompareTag("VFX"))
                _vfxs.Add(child.gameObject);
        }
    }

    protected void SpawnVFX()
    {
        foreach (GameObject vfx in _vfxs)
        {
            vfx.transform.parent = null;
            vfx.SetActive(true);
        }
    }
    protected void RestartVFX()
    {
        foreach (GameObject vfx in _vfxs)
        {
            vfx.transform.parent = transform;
            vfx.SetActive(false);
        }
    }

    void OnRestartLevel(InputAction.CallbackContext value)
    {
        Main.inputManager.Game.OnRestartLevel.performed -= OnRestartLevel;
        gameObject.SetActive(true);
        _isExploded = false;
        RestartVFX();
    }
    protected void AddForceToObject(Vector2 explosionPoint, GameObject obj,float punch)
    {
        Vector2 objPos = obj.transform.position;
        Vector2 direction = objPos - explosionPoint;
        Rigidbody2D hitRb = obj.GetComponent<Rigidbody2D>();
        direction.Normalize();
        if (hitRb && hitRb.bodyType == RigidbodyType2D.Dynamic)
        {
            hitRb.AddForce(punch*physicsPunchReduction * direction, ForceMode2D.Impulse);
        }
    }
    protected bool CheckPlayerDistance(Vector2 explosionPoint, Vector2 hitPoint)
    {
        Vector2 direction = (hitPoint - explosionPoint).normalized;
        float distance = (hitPoint - explosionPoint).magnitude;
        if (distance <= deathDistance)
        {
            if (distance==0)
                direction= (PlayerCharacter.S.pos - explosionPoint).normalized;
            PlayerCharacter.S.Kill(direction * punch * physicsPunchReduction);
            return false;
        }
        return true;
    }
    protected bool isBlocked(Vector2 explosionPoint, RaycastHit2D rayHit)
    {
        int layerMask = (1 << rayHit.collider.gameObject.layer) | LayerMask.GetMask("Default");
        RaycastHit2D[] hits = Physics2D.RaycastAll(explosionPoint, (rayHit.point - explosionPoint).normalized, explosionRadius, layerMask);
        if (hits.Length==0)
        {
            Debug.Log($"No hit in block for {rayHit.transform.gameObject.name} by {gameObject.name} dist {(rayHit.point - explosionPoint).magnitude} radius {explosionRadius}");
            return true;
        }
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.transform.CompareTag("Block"))
            {
                return true;
            }
            if (hit.transform.gameObject == rayHit.transform.gameObject)
            {
                return false;
            }
        }
        Debug.LogWarning($"Fell in default for {rayHit.transform.name} by {gameObject.name} dist {(rayHit.point - explosionPoint).magnitude} radius {explosionRadius}");
        return true;
    }
    protected void AddForceToPlayer(Vector2 explosionPoint, Vector2 hitPos,float punch, bool isOverride = false)
    {
        if (!CheckPlayerDistance(explosionPoint, hitPos))
            return;
        PlayerCharacter.S.StartIFrames();
        Vector2 direction=(PlayerCharacter.S.pos-explosionPoint).normalized;
        float angle = Vector2.Angle(Vector2.up, direction);
        if (angle <= verticalAngle / 2)
        {
            PlayerCharacter.S.controller.AddForce(new Vector2(direction.x * punch*secondaryDirectionPunchReducement, punch), isOverride);
        }
        else if (angle >= 180 - verticalAngle / 2)
        {
            PlayerCharacter.S.controller.AddForce(new Vector2(direction.x * punch*secondaryDirectionPunchReducement, -punch), isOverride);
        }
        else
        {
            PlayerCharacter.S.controller.AddForce(new Vector2(punch*(direction.x>0?1:-1),direction.y * punch), isOverride);
        }
    }
    protected RaycastHit2D GetAccurateHit(RaycastHit2D badHit,Vector2 explosionPoint)
    {
        RaycastHit2D[] rayHits = Physics2D.RaycastAll(explosionPoint, (badHit.point - explosionPoint).normalized, explosionRadius, 1 << badHit.collider.gameObject.layer);
        RaycastHit2D rayHit = default;
        foreach (RaycastHit2D r in rayHits)
        {
            if (r.transform == badHit.transform)
            {
                rayHit = r;
                break;
            }
        }
        return rayHit;
    }
    public virtual void Explode(Vector2 explosionPoint, float additionalPunch=0,bool isOverride=false)
    {
        if (_isExploded)
            return;
        _isExploded = true;
        Main.inputManager.Game.OnRestartLevel.performed += OnRestartLevel;
        float totalPunch= punch + additionalPunch;
        //Vector2 explosionPoint = transform.position;
        SpawnVFX();
        AudioManager.S.PlayOnLocation(audioInfo, explosionPoint);
        gameObject.SetActive(false);
        RaycastHit2D[] hits = Physics2D.CircleCastAll(explosionPoint, explosionRadius, Vector2.up, 0, layersCanExplode);
        foreach (RaycastHit2D hit in hits)
        {
            RaycastHit2D rayHit = GetAccurateHit(hit,explosionPoint);
            //Debug.DrawLine(explosionPoint, hit.point, Color.blue, 10);
            //Debug.DrawLine(explosionPoint, rayHit.point, Color.yellow, 10);
            //bool isGoBlocked = isBlocked(hit.transform.gameObject, rayHit.point);
            //Debug.Log($"Collider {rayHit.collider.name}");
            if (!rayHit||isBlocked(explosionPoint,rayHit))
                continue;
            switch (hit.transform.gameObject.tag)
            {
                case "Player":
                    {
                        AddForceToPlayer(explosionPoint, rayHit.point,totalPunch,isOverride);
                        break;
                    }
                case "Enemy":
                    {
                        hit.transform.GetComponent<EnemyCharacter>().Kill(physicsPunchReduction * punch * (rayHit.point - explosionPoint).normalized);
                        break;
                    }
                case "Glass":
                    {
                        hit.transform.GetComponent<Glass>().BreakByExplosion(explosionPoint, explosionRadius);
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
    //private void Update()
    //{
    //    Debug.Log(isGoBlocked(PlayerCharacter.S.gameObject));
    //}
    //void OnDisable()
    //{
    //    Main.inputManager.Game.OnRestartLevel.performed += OnRestartLevel;
    //}
    protected virtual void OnDrawGizmos()
    {
        Vector3 dir1 = Utils.RotateVector(Vector2.up, verticalAngle / 2);
        Vector3 dir2 = Utils.RotateVector(Vector2.up, -verticalAngle / 2);
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position+dir1*deathDistance,dir1*(explosionRadius-deathDistance));
        Gizmos.DrawRay(transform.position + dir2 * deathDistance, dir2 * (explosionRadius - deathDistance));
    }
}
