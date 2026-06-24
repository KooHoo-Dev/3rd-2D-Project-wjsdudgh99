using UnityEngine;
using TMPro; // TextMeshPro를 사용하기 위해 반드시 필수로 추가해야 합니다.

public class GameManager : MonoBehaviour
{
    // 싱글톤 패턴: 어디서나 GameManager.Instance로 쉽게 접근할 수 있게 만듭니다.
    public static GameManager Instance { get; private set; }

    [Header("Stage Settings")]
    public float SurviveTime = 60.0f; // 60초 생존 제한시간
    private int playerScore = 0;      // private 필드는 camelCase 준수
    private bool isBossSpawned = false;
    private bool isGameOver = false;
    public bool IsBossStage { get; private set; } = false; // 보스 스테이지 여부 (기본값 false)

    [Header("Cached References")]
    public GameObject EnemySpawner;   // 꺼버릴 일반 적 스포너 오브젝트
    public GameObject BossPrefab;     // 스폰할 보스 비행기 프리팹

    [Header("UI Elements")]
    public TextMeshProUGUI TimerText; // 타이머 UI 연결용
    public TextMeshProUGUI ScoreText; // 점수 UI 연결용

    private void Awake()
    {
        // 싱글톤 세팅
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 게임 시작 시 점수판 초기화
        UpdateScoreUI();
    }

    void Update()
    {
        if (isGameOver) return;

        // 1. 60초 타이머 흐르기 및 실시간 UI 갱신
        if (SurviveTime > 0)
        {
            SurviveTime -= Time.deltaTime;

            // 타이머 UI 가독성을 위해 정수(int)형태로 포맷팅하여 갱신합니다.
            if (TimerText != null)
            {
                TimerText.text = Mathf.Max(0, (int)SurviveTime) + "s";
            }
        }
        // 2. 0초 도달 시 일반 적 스폰을 끄고 보스 소환
        else if (isBossSpawned == false)
        {
            SpawnBossStage();
        }
    }

    // 다른 스크립트(Enemy)에서 적이 죽었을 때 호출할 점수 추가 메서드
    public void AddScore(int amount)
    {
        if (isGameOver) return;

        playerScore += amount;
        UpdateScoreUI();
    }

    // 점수 UI를 실시간으로 새로고침하는 메서드
    private void UpdateScoreUI()
    {
        if (ScoreText != null)
        {
            ScoreText.text =  playerScore.ToString("N0"); // 1,000 단위 쉼표 포맷팅
        }
    }

    private void SpawnBossStage()
    {
        isBossSpawned = true;
        IsBossStage = true; // ◀◀ [이 줄을 추가!] 보스 스테이지가 시작됨을 선언

        if (TimerText != null)
        {
            TimerText.text = "WARNING";
            TimerText.color = Color.red;
        }

        if (EnemySpawner != null)
        {
            EnemySpawner.SetActive(false);
        }

        // 화면에 남은 적들 전멸 로직 (기존 동일)
        GameObject[] remainingEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in remainingEnemies)
        {
            Destroy(enemy);
        }

        if (BossPrefab != null)
        {
            Vector3 bossSpawnPos = new Vector3(0f, 4.0f, 0f);
            Instantiate(BossPrefab, bossSpawnPos, Quaternion.identity);
        }
    }
}