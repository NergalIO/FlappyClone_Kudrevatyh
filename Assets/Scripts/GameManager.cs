using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Управляет игровым циклом: ожидание первого тапа → игра → Game Over → перезапуск.
/// Хранит счёт и рекорд. Доступен из других скриптов через синглтон GameManager.Instance.
/// </summary>
public class GameManager : MonoBehaviour
{
    // Единственный экземпляр менеджера на сцене.
    public static GameManager Instance { get; private set; }

    private enum State { Ready, Running, GameOver }

    [SerializeField] private PipeSpawner spawner;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private GameObject hintText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI finalScoreText;

    private const string BestScoreKey = "BestScore";

    private State state = State.Ready;
    private int score;

    public bool IsReady => state == State.Ready;
    public bool IsRunning => state == State.Running;
    public bool IsGameOver => state == State.GameOver;

    // Awake вызывается раньше любых Start/Update, поэтому Instance
    // уже назначен, когда птица и трубы впервые обращаются к менеджеру.
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        gameOverPanel.SetActive(false);
        hintText.SetActive(true);
        UpdateScoreText();
    }

    // Вызывается птицей при первом тапе.
    public void StartGame()
    {
        if (state != State.Ready) return;

        state = State.Running;
        hintText.SetActive(false);
        spawner.StartSpawning();
    }

    public void AddScore(int value)
    {
        if (!IsRunning) return;
        score += value;
        UpdateScoreText();
    }

    public void GameOver()
    {
        if (IsGameOver) return;

        state = State.GameOver;
        spawner.StopSpawning();

        // Рекорд хранится в PlayerPrefs и переживает перезапуск приложения.
        int best = Mathf.Max(score, PlayerPrefs.GetInt(BestScoreKey, 0));
        PlayerPrefs.SetInt(BestScoreKey, best);
        PlayerPrefs.Save();

        finalScoreText.text = "Счёт: " + score + "\nРекорд: " + best;
        gameOverPanel.SetActive(true);
    }

    // Вызывается кнопкой «Играть снова»: перезагрузка сцены сбрасывает всё состояние.
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void UpdateScoreText()
    {
        scoreText.text = score.ToString();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
