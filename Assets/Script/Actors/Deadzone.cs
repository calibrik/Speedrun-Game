using UnityEngine;

public class Deadzone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.CompareTag("Enemy"))
        {
            collision.GetComponent<Character>().Kill(Vector2.zero);
            return;
        }
        if (collision.CompareTag("Gun") || collision.CompareTag("Limb"))
        {
            Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Static;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0;
        }
    }
}
