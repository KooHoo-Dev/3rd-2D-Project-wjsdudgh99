using System.Collections;
using UnityEngine;

public class Boss : MonoBehaviour
{
    [Header("Boss Stats")]
    public int Hp = 350;                 // 기획서 반영: 높은 체력
    public float Speed = 1.5f;           // 기획서 반영: 대형기 특유의 느릿하고 묵직한 속도

    [Header("Prefabs")]
    public GameObject EnemyBulletPrefab; // 일반 3갈래 적 총알 프리팹
    public GameObject BossMissilePrefab; // 보스 전용 대형 미사일 프리팹

    [Header("Attack Settings")]
    public float FireRate = 1.5f;        // 공격 주기 (1.5초마다 무기 번갈아 발사)
    private float lastFireTime = 0f;
    private bool toggleWeapon = false;   // 두 가지 무기를 번갈아 쏘기 위한 스위치

    [Header("Movement Settings")]
    private float targetY = 3.5f;        // 화면 상단에 알박기할 목적지 Y 좌표
    private bool isArrived = false;

    // 보스는 맵 중앙(X=0)에서 넓게 왕복하도록 한계점을 -2.0에서 +2.0으로 고정합니다.
    private float leftX = -2.0f;
    private float rightX = 2.0f;
    private Vector3 targetPosition;

    void Start()
    {
        // 처음 등장 시 일단 현재 위치(중앙 상단)를 목표 지점으로 설정
        targetPosition = transform.position;
    }

    void Update()
    {
        // 1. 등장 연출: 지정한 Y 고도(3.5f)까지 위에서 스르륵 내려옵니다.
        if (isArrived == false)
        {
            Vector3 moveVector = new Vector3(0, -1f, 0);
            transform.position += moveVector * Speed * Time.deltaTime;

            if (transform.position.y <= targetY)
            {
                transform.position = new Vector3(0f, targetY, 0f);
                isArrived = true;

                // 고도 고정이 완료되면 보스 순찰 및 공격 루프 가동!
                StartCoroutine(BossPatrolRoutine());
            }
        }
        else
        {
            // 2. 고도 도착 후: 코루틴이 지정하는 좌우 targetPosition으로 묵직하게 이동
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, Speed * Time.deltaTime);
        }

        // 3. 공격 패턴 제어: 자리에 도착한 직후부터 1.5초마다 패턴 난사
        if (isArrived && Time.time >= lastFireTime + FireRate)
        {
            FireBossWeapon();
            lastFireTime = Time.time;
        }
    }

    // 좌로 이동 후 1초 대기, 우로 이동 후 1초 대기하는 보스 웅장한 무빙 루틴
    private IEnumerator BossPatrolRoutine()
    {
        while (true)
        {
            // 왼쪽 한계점(-2.0f)으로 이동 지시
            targetPosition = new Vector3(leftX, targetY, 0f);
            while (transform.position.x > leftX + 0.05f)
            {
                yield return null;
            }
            // 왼쪽 도착 후 1초 숨 고르기 및 플레이어에게 딜 타임 제공
            yield return new WaitForSeconds(1.0f);

            // 오른쪽 한계점(2.0f)으로 이동 지시
            targetPosition = new Vector3(rightX, targetY, 0f);
            while (transform.position.x < rightX - 0.05f)
            {
                yield return null;
            }
            // 오른쪽 도착 후 1초 대기
            yield return new WaitForSeconds(1.0f);
        }
    }

    // 보스의 하이브리드 공격 패턴 로직
    void FireBossWeapon()
    {
        // 스위치(toggleWeapon)를 이용해 3갈래 탄막과 대형 미사일 난사를 번갈아 실행합니다.
        if (toggleWeapon == false)
        {
            // 패턴 A: 일반 총알 부채꼴 3갈래 발사
            if (EnemyBulletPrefab != null)
            {
                Quaternion centerRot = Quaternion.Euler(0, 0, 180f);
                Quaternion leftRot = Quaternion.Euler(0, 0, 160f);
                Quaternion rightRot = Quaternion.Euler(0, 0, 200f);

                Instantiate(EnemyBulletPrefab, transform.position, centerRot);
                Instantiate(EnemyBulletPrefab, transform.position, leftRot);
                Instantiate(EnemyBulletPrefab, transform.position, rightRot);
            }
        }
        else
        {
            // 패턴 B: 보스 전용 미사일 일직선 하방 발사! (살짝 벌려진 2개 열로 뿜어내기)
            if (BossMissilePrefab != null)
            {
                Quaternion downRot = Quaternion.Euler(0, 0, 180f);
                Vector3 leftWing = transform.position + new Vector3(-0.5f, -0.5f, 0f);
                Vector3 rightWing = transform.position + new Vector3(0.5f, -0.5f, 0f);

                Instantiate(BossMissilePrefab, leftWing, downRot);
                Instantiate(BossMissilePrefab, rightWing, downRot);
            }
        }

        // 다음 공격 때는 다른 무기가 나가도록 스위치 전환
        toggleWeapon = !toggleWeapon;
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        // 플레이어의 총알에 맞았을 때
        if (collider.gameObject.CompareTag("PlayerBullet"))
        {
            Destroy(collider.gameObject); // 플레이어 총알 소멸

            Hp -= 1; // 보스 체력 차감

            // 보스가 피격될 때마다 점수를 소량 가산하는 기획 반영 (과제 피드백 만족)
            GameManager.Instance.AddScore(10);

            // 350방을 모두 버텨내고 격추당했을 때
            if (Hp <= 0)
            {
                GameManager.Instance.AddScore(5000); // 보스 클리어 대량 보너스 점수!

                GameManager.Instance.TriggerVictoryClear();

                Destroy(gameObject); // 보스 파괴

            }
        }
    }
}