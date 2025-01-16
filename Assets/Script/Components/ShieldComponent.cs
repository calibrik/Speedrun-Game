using UnityEngine;

public class ShieldComponent : MonoBehaviour
{
    public bool isActive=>_collider.enabled;
    [SerializeField]
    private float shieldSpeedThreshold = 20;
    [SerializeField]
    private float fullShieldSpeedThreshold = 30;
    [SerializeField]
    private bool isAlwaysFull = false;

    private Collider2D _collider;
    private Material _material;
    // Start is called before the first frame update
    void Start()
    {
        _material = GetComponent<Renderer>().material;
        _collider = GetComponent<Collider2D>();
        if (!isAlwaysFull)
        {
            _collider.enabled = false;
            PlayerCharacter.S.controller.OnMove += SetShield;
            _material.SetFloat("_Transparency", 0);
        }
        else
        {
            _collider.enabled = true;
            _material.SetFloat("_Transparency", 1);
        }
    }

    void SetShield(Vector2 velocity)
    {
        float speed=velocity.magnitude;
        if (speed<shieldSpeedThreshold)
        {
            _material.SetFloat("_Transparency", 0);
            _collider.enabled = false;
            return;
        }
        _collider.enabled = true;
        float transparency=(speed-shieldSpeedThreshold)/(fullShieldSpeedThreshold-shieldSpeedThreshold);
        _material.SetFloat("_Transparency", Mathf.Clamp(transparency,0,1));
    }
}
