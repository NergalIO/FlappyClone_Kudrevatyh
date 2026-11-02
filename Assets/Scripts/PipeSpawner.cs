using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Создаёт трубы через равные промежутки времени (InvokeRepeating)
/// со случайной высотой прохода. Выключенные трубы переиспользуются (пул объектов).
/// </summary>
public class PipeSpawner : MonoBehaviour
{
    [SerializeField] private Pipe pipePrefab;
    // Интервал между трубами (с) и задержка перед первой.
    [SerializeField] private float spawnInterval = 1.5f;
    [SerializeField] private float firstDelay = 1f;
    // X появления — правее видимой области камеры.
    [SerializeField] private float spawnX = 7f;
    // Диапазон высоты центра прохода.
    [SerializeField] private float minY = -4f;
    [SerializeField] private float maxY = 4.5f;
    // Наибольший перепад высоты между соседними проходами.
    [SerializeField] private float maxStepY = 3.5f;

    private readonly List<Pipe> pool = new List<Pipe>();
    private float lastY;

    public void StartSpawning()
    {
        lastY = 0f;
        InvokeRepeating(nameof(SpawnPipe), firstDelay, spawnInterval);
    }

    public void StopSpawning()
    {
        CancelInvoke(nameof(SpawnPipe));
    }

    private void SpawnPipe()
    {
        // Новая высота — случайный сдвиг от предыдущей, но в пределах minY..maxY:
        // так не бывает перепада с самого низа на самый верх за одну трубу.
        float y = Mathf.Clamp(lastY + Random.Range(-maxStepY, maxStepY), minY, maxY);
        lastY = y;

        Vector2 position = new Vector2(spawnX, y);
        Pipe pipe = pool.Find(p => !p.gameObject.activeSelf);
        if (pipe == null)
        {
            // Свободной трубы нет — создаём новую. После разгона пул перестаёт расти.
            pipe = Instantiate(pipePrefab, position, Quaternion.identity, transform);
            pool.Add(pipe);
            Debug.Log("Пул труб: " + pool.Count);
        }
        pipe.Launch(position);
    }
}
