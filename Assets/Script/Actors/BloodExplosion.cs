using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BloodExplosion : MonoBehaviour
{
    [SerializeField]
    private GameObject bloodSplashPreFab;
    [SerializeField]
    private int amountOfSplashes = 30;
    [SerializeField]
    private LayerMask canLeaveSplashOn;
    [SerializeField]
    private float distanceToLeaveSpalsh=5;
    //[SerializeField]
    //[Range(0f, 360f)]
    //private float arc=220f;
    [SerializeField]
    [Range(0f, 1f)]
    private float probabilityToLeaveSplash=0.6f;
    [SerializeField]
    private LayerMask canLeaveSplashesOn;
    private List<GameObject> _spawnedSplashes;
    private void OnEnable()
    {
        if (_spawnedSplashes==null)
            _spawnedSplashes = new List<GameObject>();
        Main.inputManager.Game.OnRestartLevel.performed += OnResatartLevel;
        float angleToRotateBy = 360 / amountOfSplashes;
        float currAngle = 0;
        while (currAngle< 360)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Utils.RotateVector(Vector2.down, currAngle), distanceToLeaveSpalsh, canLeaveSplashOn);
            if (hit&& Random.Range(0f, 1f) <= probabilityToLeaveSplash)
            {
                _spawnedSplashes.Add(Instantiate(bloodSplashPreFab,hit.point,Quaternion.identity));
            }
            currAngle += angleToRotateBy;
        }
    }
    void OnResatartLevel(InputAction.CallbackContext value)
    {
        Main.inputManager.Game.OnRestartLevel.performed -= OnResatartLevel;
        while (_spawnedSplashes.Count>0) 
        {
            Destroy(_spawnedSplashes[0]);
            _spawnedSplashes.RemoveAt(0);
        }
    }
}
