using UnityEngine;

/// <summary>
/// Бесконечная прокрутка слоя (фон, земля). Спрайт в режиме Tiled длиннее экрана;
/// объект едет влево и, сдвинувшись на ширину одного тайла, возвращается назад —
/// картинка при этом не меняется, поэтому скачка не видно.
/// Разные скорости слоёв дают эффект параллакса.
/// </summary>
public class Parallax : MonoBehaviour
{
    // Скорость слоя (ед/с): у земли равна скорости труб, у фона — намного меньше.
    [SerializeField] private float speed = 0.6f;
    // Ширина одного повторяющегося фрагмента спрайта в мировых единицах.
    [SerializeField] private float tileWidth = 10.8f;

    private Vector3 startPosition;
    private float distance;

    private void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        // Мир «едет» и до первого тапа, а после проигрыша останавливается.
        if (GameManager.Instance.IsGameOver) return;

        distance += speed * Time.deltaTime;
        // Mathf.Repeat возвращает остаток от деления: 0..tileWidth.
        float offset = Mathf.Repeat(distance, tileWidth);
        transform.position = startPosition + Vector3.left * offset;
    }
}
