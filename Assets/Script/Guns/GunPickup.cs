using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunPickup : MonoBehaviour
{
    private PlayerGun _gun;

    void Start()
    {
        _gun = transform.GetChild(0).GetComponent<PlayerGun>();
        _gun.GunPickup = this;
        _gun.gameObject.SetActive(false);
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (PlayerCharacter.S.HasGun||(!collision.gameObject.CompareTag("Player")&&!collision.gameObject.CompareTag("IFramesBody")))
            return;
        _gun.gameObject.SetActive(true);
        PlayerCharacter.S.PickGun(_gun);
        gameObject.SetActive(false);
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (PlayerCharacter.S.HasGun || (!other.gameObject.CompareTag("Player") && !other.gameObject.CompareTag("IFramesBody")))
            return;
        _gun.gameObject.SetActive(true);
        PlayerCharacter.S.PickGun(_gun);
        gameObject.SetActive(false);
    }
}
