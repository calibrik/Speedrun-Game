using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Gore : MonoBehaviour
{
    struct LimbInfo
    {
        public Transform transform;
        public Rigidbody2D rb;
        public Collider2D colllider;
        public Vector2 origPos;
        public Quaternion origRot;
    }

    private List<LimbInfo> _limbs;
    private List<GameObject> _vfxs;
    //private List<LimbInfo> _chunks;
    private CapsuleCollider2D _capsuleCollider;
    private Rigidbody2D _rb;
    private Animator _animator;
    private LayerMask _origLayer;
    private RigidbodyType2D _origRbType;
    // Start is called before the first frame update
    void FindAllLimbsAndVFX(Transform root)
    {
        for (int i = 0; i < root.childCount; i++)
        {
            Transform child = root.GetChild(i);
            if (child.CompareTag("VFX"))
            {
                _vfxs.Add(child.gameObject);
                continue;
            }
            if (child.CompareTag("Limb"))
            {
                LimbInfo limbInfo = new LimbInfo();
                limbInfo.rb = child.GetComponent<Rigidbody2D>();
                limbInfo.transform = child;
                limbInfo.colllider = child.GetComponent<Collider2D>();
                limbInfo.origPos = child.localPosition;
                limbInfo.origRot = child.localRotation;
                _limbs.Add(limbInfo);
            }
            //if (limb.CompareTag("Chunk"))
            //{
            //    _chunks.Add(limbInfo);
            //    continue;
            //}
            if (child.childCount > 0)
                FindAllLimbsAndVFX(child);
        }
    }
    void Start()
    {
        _origLayer=gameObject.layer;
        _animator = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody2D>();
        _vfxs=new List<GameObject>();
        if (_rb)
            _origRbType = _rb.bodyType;
        //_chunks=new List<LimbInfo>();
        _limbs = new List<LimbInfo>();
        _capsuleCollider = GetComponent<CapsuleCollider2D>();
        FindAllLimbsAndVFX(transform);
    }
    void OnRestartLevel(InputAction.CallbackContext value)
    {
        Main.inputManager.Game.OnRestartLevel.performed -= OnRestartLevel;
        if (_animator)
            _animator.enabled = true;
        if (_rb)
            _rb.bodyType=_origRbType;
        if (_capsuleCollider)
            _capsuleCollider.enabled = true;
        foreach (LimbInfo limb in _limbs)
        {
            if (!limb.rb)
            {
                limb.transform.gameObject.SetActive(true);
                continue;
            }
            limb.colllider.enabled = false;
            limb.transform.parent = transform;
            limb.rb.bodyType = RigidbodyType2D.Kinematic;
            limb.rb.linearVelocity = Vector2.zero;
            limb.rb.angularVelocity = 0;
            limb.transform.SetLocalPositionAndRotation(limb.origPos, limb.origRot);
            limb.transform.gameObject.layer = _origLayer;
        }
        //foreach (LimbInfo chunk in _chunks)
        //{
        //    chunk.rb.velocity = Vector2.zero;
        //    chunk.rb.angularVelocity = 0;
        //    chunk.transform.SetLocalPositionAndRotation(chunk.origPos, chunk.origRot);
        //    chunk.transform.parent = transform;
        //    chunk.transform.gameObject.SetActive(false);
        //}
        foreach (GameObject vfx in _vfxs)
        {
            vfx.SetActive(false);
        }
    }
    // Update is called once per frame
    public void ApplyGore(Vector2 punch)
    {
        Main.inputManager.Game.OnRestartLevel.performed += OnRestartLevel;
        if (_animator)
            _animator.enabled = false;
        if (_rb)
            _rb.bodyType=RigidbodyType2D.Static;
        if (_capsuleCollider)
            _capsuleCollider.enabled = false;
        foreach (LimbInfo limb in _limbs)
        {
            if (!limb.rb)
            {
                limb.transform.gameObject.SetActive(false);
                continue;
            }
            limb.colllider.enabled = true;
            limb.transform.parent=null;
            limb.transform.gameObject.layer = LayerMask.NameToLayer("Dynamic");
            limb.rb.bodyType = RigidbodyType2D.Dynamic;
            limb.rb.AddForce(punch,ForceMode2D.Impulse);
        }
        //foreach (LimbInfo chunk in _chunks)
        //{
        //    chunk.transform.parent = null;
        //    chunk.transform.gameObject.SetActive(true);
        //    chunk.rb.AddForce(punch, ForceMode2D.Impulse);
        //}
        foreach (GameObject vfx in _vfxs)
        {
            vfx.SetActive(true);
        }
    }
}
