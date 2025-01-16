using UnityEngine;
using UnityEngine.InputSystem;


public class CheatSheet : MonoBehaviour
{
    [SerializeField] private GameObject gunPreFab;
    
    // Start is called before the first frame update
    void Start()
    {
        Main.inputManager.Cheats.OnSpawnGun.performed += SpawnGun;
    }

    void SpawnGun(InputAction.CallbackContext value)
    {
        if (PlayerCharacter.S.HasGun)
        {
            Debug.LogError("Player already has gun");
            return;
        }
        GameObject gun = Instantiate(gunPreFab);
        gun.transform.position = PlayerCharacter.S.transform.position;
        Gun gunComponent=gun.GetComponent<Gun>();
        PlayerCharacter.S.PickGun(gunComponent);
        Debug.Log("Gun Spawned");
    }
}