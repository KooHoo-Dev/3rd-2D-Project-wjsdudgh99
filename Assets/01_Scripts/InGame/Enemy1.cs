using UnityEngine;

public class NormalEnemy : MonoBehaviour
{
    // public 필드는 PascalCase 준수
    public float Speed = 3.0f;

    public GameObject HpUpItemPrefab;

    void Update()
    {
        // 매 프레임 아래(-Y 방향)로 일직선 이동
        Vector3 moveVector = new Vector3(0, -1f, 0);
        moveVector = moveVector * Speed * Time.deltaTime;
        transform.position += moveVector;

        // 화면 아래로 완전히 벗어나면 자동 삭제 (메모리 관리)
        // 일반적인 16:9 해상도에서 Y좌표가 -6.0f 미만이면 화면 아래를 벗어난 상태입니다.
        if (transform.position.y < -6.0f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("PlayerBullet"))
        {
            GameManager.Instance.AddScore(100); // 일반 적 격추 시 100점 추가

            float dice2 = Random.Range(0f, 100f);

            if (dice2 <= 5f || GameManager.Instance.DeadEnemyCount >= 10)
            {
                Instantiate(HpUpItemPrefab, transform.position, Quaternion.identity);

                GameManager.Instance.DeadEnemyCount = 0;
            }

            Destroy(gameObject); // 적 비행기 파괴

            // 플레이어 총알 스크립트에서도 Destroy를 해주지만, 
            // 확실한 처리를 위해 여기서도 총알을 지워줄 수 있습니다.
            Destroy(collider.gameObject);
        }
    }
}