using UnityEngine;

/// <summary>
/// Пара труб с проходом между ними. Едет влево с постоянной скоростью,
/// за левым краем экрана выключается и ждёт повторного использования в пуле.
/// Дочерний триггер ScoreZone в проходе начисляет очко за пролёт.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class Pipe : MonoBehaviour
{
    // Скорость движения влево (ед/с); совпадает со скоростью прокрутки земли.
    [SerializeField] private float speed = 4f;
    // Левее этой координаты труба уже не видна и выключается.
    [SerializeField] private float despawnX = -7f;

    private Rigidbody2D rb;
    private bool scored;

    private void Awake()
    {
        // Kinematic Rigidbody2D: трубу двигает скрипт, а не гравитация,
        // но физика знает о движении и правильно считает касания с птицей.
        rb = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// Ставит трубу в точку появления и включает её. Вызывается спавнером.
    /// </summary>
    public void Launch(Vector2 position)
    {
        scored = false;
        // Позиция меняется до включения объекта: Rigidbody2D появится уже в новой точке.
        transform.position = position;
        gameObject.SetActive(true);
    }

    private void FixedUpdate()
    {
        // После проигрыша трубы останавливаются вместе со всем миром.
        if (!GameManager.Instance.IsRunning) return;

        Vector2 next = rb.position + Vector2.left * (speed * Time.fixedDeltaTime);
        rb.MovePosition(next);

        if (next.x < despawnX)
            gameObject.SetActive(false);
    }

    // Сообщение приходит и от дочернего триггера ScoreZone,
    // потому что его коллайдер прикреплён к Rigidbody2D этого объекта.
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (scored || !other.CompareTag("Player")) return;

        scored = true;
        GameManager.Instance.AddScore(1);
    }
}
