using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehavior : MonoBehaviour
{
    private Transform _playerTransform;
    // Start is called before the first frame update
    void Start()
    {
        _playerTransform=PlayerCharacter.S.gameObject.transform;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.position=new Vector3(_playerTransform.position.x,_playerTransform.position.y,transform.position.z);
    }
}
