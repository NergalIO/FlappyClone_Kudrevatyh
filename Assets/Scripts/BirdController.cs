using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Управление птицей: прыжок по тапу, ограничение скорости падения,
/// «потолок» над экраном и наклон спрайта по направлению полёта.
/// До первого тапа птица покачивается на месте, физика выключена.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class BirdController : MonoBehaviour
{
    // Вертикальная скорость, которую получает птица при тапе (ед/с).
    [SerializeField] private float jumpForce = 13f;
    // Максимальная скорость падения, чтобы птица не «проваливалась» слишком быстро.
    [SerializeField] private float maxFallSpeed = 18f;
    // Выше этой высоты птица не поднимается (верхний край экрана — 9,6).
    [SerializeField] private float ceilingY = 9f;

    // Дочерний объект со спрайтом: наклоняется он, а не Rigidbody2D (Freeze Rotation Z).
    [SerializeField] private Transform visual;
    [SerializeField] private float maxTiltUp = 25f;
    [SerializeField] private float maxTiltDown = -70f;
    [SerializeField] private float tiltSmoothing = 10f;

    // Покачивание в режиме ожидания первого тапа.
    [SerializeField] private float bobAmplitude = 0.25f;
    [SerializeField] private float bobFrequency = 2f;

    private Rigidbody2D rb;
    private Vector3 startPosition;
    private bool jumpRequested;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        startPosition = transform.position;
        // Пока игра не началась, гравитация на птицу не действует.
        rb.simulated = false;
    }

    private void Update()
    {
        if (GameManager.Instance.IsGameOver) return;

        // Pointer.current — мышь в редакторе и тачскрин на телефоне (новый Input System).
        Pointer pointer = Pointer.current;
        if (pointer != null && pointer.press.wasPressedThisFrame)
            Flap();

        if (GameManager.Instance.IsReady)
        {
            float offset = Mathf.Sin(Time.time * bobFrequency * Mathf.PI) * bobAmplitude;
            transform.position = startPosition + Vector3.up * offset;
            return;
        }

        UpdateTilt();
    }

    // Ввод читается в Update, а скорость меняется в FixedUpdate — в шаге физики.
    private void FixedUpdate()
    {
        if (!rb.simulated) return;

        Vector2 velocity = rb.linearVelocity;
        if (jumpRequested)
        {
            // Скорость задаётся напрямую, а не через AddForce: высота прыжка
            // одинакова, падает птица в момент тапа или поднимается.
            velocity.y = jumpForce;
            jumpRequested = false;
        }

        velocity.y = Mathf.Max(velocity.y, -maxFallSpeed);

        if (rb.position.y > ceilingY)
        {
            rb.position = new Vector2(rb.position.x, ceilingY);
            velocity.y = Mathf.Min(velocity.y, 0f);
        }

        rb.linearVelocity = velocity;
    }

    /// <summary>
    /// Взмах крыльями. Первый взмах запускает игру и включает физику.
    /// Публичный, чтобы прыжок можно было вызвать не только тапом.
    /// </summary>
    public void Flap()
    {
        GameManager gm = GameManager.Instance;
        if (gm.IsGameOver) return;

        if (gm.IsReady)
        {
            transform.position = startPosition;
            rb.simulated = true;
            gm.StartGame();
        }
        jumpRequested = true;
    }

    private void UpdateTilt()
    {
        // При подъёме нос смотрит вверх, при падении плавно опускается вниз.
        float vy = rb.linearVelocity.y;
        float t = Mathf.InverseLerp(-maxFallSpeed * 0.6f, jumpForce * 0.5f, vy);
        float targetAngle = Mathf.Lerp(maxTiltDown, maxTiltUp, t);
        float angle = Mathf.LerpAngle(visual.localEulerAngles.z, targetAngle,
            tiltSmoothing * Time.deltaTime);
        visual.localRotation = Quaternion.Euler(0f, 0f, angle);
    }
}
