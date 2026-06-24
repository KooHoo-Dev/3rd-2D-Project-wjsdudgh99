using System.Collections;
using UnityEngine;

public class Enemy2 : MonoBehaviour
{
    // 기획서 속성 반영
    public int Hp = 10;
    public float Speed = 3.0f;

    public GameObject EnemyBulletPrefab;
    public float FireRate = 1.5f;
    private float lastFireTime = 0f;

    // 이동 패턴 제어를 위한 변수들
    private float targetY = 3.5f;
    private bool isArrived = false;

    private float leftX;
    private float rightX;
    private Vector3 targetPosition;
    public float PatrolRange = 1.5f;

    // [추가] 처음 출발할 방향을 결정하기 위한 변수 (true면 오른쪽 우선, false면 왼쪽 우선)
    private bool isHeadingRightFirst = true;

    void Awake()
    {
        float startX = transform.position.x;

        // [핵심 변경] 생성된 X 좌표를 기반으로 첫 이동 방향을 결정합니다.
        if (startX < 0)
        {
            // 1. 왼쪽 공간(X가 0보다 작음)에서 태어난 경우
            // 맵 왼쪽을 뚫지 않도록, 순찰 범위를 '현재 위치 ~ 오른쪽'으로만 제한합니다.
            leftX = startX;
            rightX = startX + (PatrolRange * 2f); // 오른쪽으로 길게 범위를 잡음
            isHeadingRightFirst = true;           // 처음엔 무조건 오른쪽으로 출발!
        }
        else
        {
            // 2. 오른쪽 공간(X가 0보다 크거나 같음)에서 태어난 경우
            // 맵 오른쪽을 뚫지 않도록, 순찰 범위를 '왼쪽 ~ 현재 위치'로 제한합니다.
            leftX = startX - (PatrolRange * 2f); // 왼쪽으로 길게 범위를 잡음
            rightX = startX;
            isHeadingRightFirst = false;          // 처음엔 무조건 왼쪽으로 출발!
        }
    }

    void Update()
    {
        // 1. 아직 목적지 Y에 도달하지 않았다면 아래로 직선 이동
        if (isArrived == false)
        {
            Vector3 moveVector = new Vector3(0, -1f, 0);
            transform.position += moveVector * Speed * Time.deltaTime;

            if (transform.position.y <= targetY)
            {
                transform.position = new Vector3(transform.position.x, targetY, 0f);
                isArrived = true;

                // 자리에 도착하면 순찰 시작
                StartCoroutine(PatrolRoutine());
            }
        }
        else
        {
            // 2. 코루틴이 정해주는 targetPosition을 향해 부드럽게 이동
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, Speed * Time.deltaTime);
        }

        // 3. 3갈래 총알 발사 로직 (기존 유지)
        if (Time.time >= lastFireTime + FireRate)
        {
            if (EnemyBulletPrefab != null)
            {
                FireThreeWay();
                lastFireTime = Time.time;
            }
        }
    }

    // C# 표준에 맞게 수정된 안전한 순찰 코루틴
    private IEnumerator PatrolRoutine()
    {
        // 처음 도착했을 때는 일단 현재 위치를 유지
        targetPosition = transform.position;

        // [핵심 변경] 오른쪽에서 스폰된 적(isHeadingRightFirst가 false인 적)은 
        // 처음 게임이 시작될 때 '오른쪽 이동'을 건너뛰도록 flag 역할을 해줄 변수입니다.
        bool skipRightFirst = !isHeadingRightFirst;

        while (true)
        {
            // 1. 오른쪽 끝으로 이동 (skipRightFirst가 true라면 이 단계를 완전히 건너뜁니다)
            if (skipRightFirst == false)
            {
                targetPosition = new Vector3(rightX, targetY, 0f);
                while (transform.position.x < rightX - 0.05f)
                {
                    yield return null;
                }

                // 오른쪽 끝에 완전히 도착했으므로 1초 동안 정지
                yield return new WaitForSeconds(1.0f);
            }

            // 첫 루프에서 한 번 건너뛰었다면, 다음 루프부터는 정상 작동하도록 변수를 풀어줍니다.
            skipRightFirst = false;

            // 2. 왼쪽 끝으로 이동
            targetPosition = new Vector3(leftX, targetY, 0f);
            while (transform.position.x > leftX + 0.05f)
            {
                yield return null;
            }

            // 왼쪽 끝에 완전히 도착했으므로 1초 동안 정지
            yield return new WaitForSeconds(1.0f);
        }
    }

    // 3갈래 발사 로직 (기존 유지)
    void FireThreeWay()
    {
        Quaternion centerRotation = Quaternion.Euler(0, 0, 180f);
        Quaternion leftRotation = Quaternion.Euler(0, 0, 160f);
        Quaternion rightRotation = Quaternion.Euler(0, 0, 200f);

        Instantiate(EnemyBulletPrefab, transform.position, centerRotation);
        Instantiate(EnemyBulletPrefab, transform.position, leftRotation);
        Instantiate(EnemyBulletPrefab, transform.position, rightRotation);
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (Hp <= 0)
        {
            GameManager.Instance.AddScore(300); // 맷집 강한 엘리트 적 격추 시 300점 추가
            Destroy(gameObject);
        }

        if (collider.gameObject.CompareTag("PlayerBullet"))
        {
            Destroy(collider.gameObject);
            Hp -= 1;

            if (Hp <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}