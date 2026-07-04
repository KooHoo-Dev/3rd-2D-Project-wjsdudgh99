using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject Enemy1Prefab; // 일반 적 프리팹
    public GameObject Enemy2Prefab;      // 3갈래 적 프리팹

    public float SpawnRate = 1f;       // 적 생성 주기
    private float lastSpawnTime = 0f;

    void Update()
    {
        // [완벽 차단막] GameManager가 보스 스테이지라고 하면, 아래 소환 로직을 아예 실행하지 않고 나가버립니다.
        if (GameManager.Instance != null && GameManager.Instance.IsBossStage)
        {
            return;
        }

        if (Time.time >= lastSpawnTime + SpawnRate)
        {
            // 0 또는 1 중에서 랜덤 확률 뽑기
            int randomEnemy = Random.Range(0, 2);

            float randomX = Random.Range(-3.0f, 3.0f);
            Vector3 spawnPosition = new Vector3(randomX, 6.0f, 0);

            if (randomEnemy == 0 && Enemy1Prefab != null)
            {
                // 50% 확률로 일반 적 스폰
                Instantiate(Enemy1Prefab, spawnPosition, Quaternion.Euler(0f, 0f, 180f));
            }
            else if (randomEnemy == 1 && Enemy2Prefab != null)
            {
                // 50% 확률로 3갈래 적 스폰
                Instantiate(Enemy2Prefab, spawnPosition, Quaternion.Euler(0f, 0f, 180f));
            }

            lastSpawnTime = Time.time;
        }
    }
}