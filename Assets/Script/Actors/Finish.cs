using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Finish : MonoBehaviour
{
    private void Start()
    {
        gameObject.SetActive(false);
        Main.S.OnAllEnemiesDied += () =>
        {
            gameObject.SetActive(true);
        };
        Main.inputManager.Game.OnRestartLevel.performed += (value) =>
        {
            gameObject.SetActive(false);
        };
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.gameObject.CompareTag("Player"))
            return;
        Main.S.PLayerCrossedFinish();
    }
}
