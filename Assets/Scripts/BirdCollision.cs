using UnityEngine;

/// <summary>
/// Столкновение птицы с трубой или землёй завершает игру.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class BirdCollision : MonoBehaviour
{
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (GameManager.Instance.IsGameOver) return;

        // Тег проверяется у коллайдера, а не у collision.gameObject: у трубы
        // Rigidbody2D висит на корне без тега, а теги Pipe — у PipeTop/PipeBottom.
        Collider2D other = collision.collider;
        if (other.CompareTag("Pipe") || other.CompareTag("Ground"))
        {
            // Выключаем физику — птица замирает в точке удара.
            rb.simulated = false;
            GameManager.Instance.GameOver();
        }
    }
}
