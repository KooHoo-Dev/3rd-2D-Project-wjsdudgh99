using System.Collections;
using UnityEngine;

public class Boss : MonoBehaviour
{
    [Header("Boss Stats")]
    public int Hp = 350;                 // 기획서 반영: 높은 체력
    public float Speed = 1.5f;           // 기획서 반영: 대형기 특유의 느릿하고 묵직한 속도

    [Header("Prefabs")]
    //public GameObject EnemyBulletPrefab; // 일반 3갈래 적 총알 프리팹
    //public GameObject BossMissilePrefab; // 보스 전용 대형 미사일 프리팹
    public GameObject BossBulletPrefab;
    public GameObject LaserBeamObject;

    [Header("Attack Settings")]
    public float FireRate = 1.5f;        // 공격 주기 (1.5초마다 무기 번갈아 발사)
    private float lastFireTime = 0f;

    [Header("Movement Settings")]
    private float targetY = 3.5f;        // 화면 상단에 알박기할 목적지 Y 좌표
    private bool isArrived = false;

    [Header("BossFirePoint")]
    public Transform FirePoint;

    // 보스는 맵 중앙(X=0)에서 넓게 왕복하도록 한계점을 -2.0에서 +2.0으로 고정합니다.
    private float leftX = -2.0f;
    private float rightX = 2.0f;
    private Vector3 targetPosition;

    private int attackPattern = 0;
    private int patternRepeatCount = 0;
    private BossAnimation bossAnim;

    void Start()
    {
        // 처음 등장 시 일단 현재 위치(중앙 상단)를 목표 지점으로 설정
        targetPosition = transform.position;

        bossAnim = GetComponent<BossAnimation>();
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
            //lastFireTime = Time.time; 
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

    private IEnumerator BurstShotPattern()
    {
        bossAnim.BossBulletAnim(true);

        for (int i = 0; i < 3; ++i)
        {
            if (BossBulletPrefab != null)
            {
                Quaternion FourOClockRot = Quaternion.Euler(0, 0, 210f);
                Quaternion FiveOClockRot = Quaternion.Euler(0, 0, 195f);
                Quaternion SixOClockRot = Quaternion.Euler(0, 0, 180f);
                Quaternion SevenOClockRot = Quaternion.Euler(0, 0, 165f);
                Quaternion EightOClockRot = Quaternion.Euler(0, 0, 150f);

                Instantiate(BossBulletPrefab, FirePoint.position, FourOClockRot);
                Instantiate(BossBulletPrefab, FirePoint.position, FiveOClockRot);
                Instantiate(BossBulletPrefab, FirePoint.position, SixOClockRot);
                Instantiate(BossBulletPrefab, FirePoint.position, SevenOClockRot);
                Instantiate(BossBulletPrefab, FirePoint.position, EightOClockRot);
            }

            yield return new WaitForSeconds(0.2f);
        }

        bossAnim.BossBulletAnim(false);
    }

    private IEnumerator ClockwiseShotPattern()
    {
        bossAnim.BossBulletAnim(true);

        float currentAngle = 120f;

        for (int i = 0; i < 15; ++i)
        {
            if (BossBulletPrefab != null)
            {
                Quaternion rot = Quaternion.Euler(0, 0, currentAngle);

                Instantiate(BossBulletPrefab, FirePoint.position, rot);
            }

            currentAngle += 8.5f;

            yield return new WaitForSeconds(0.1f);
        }

        bossAnim.BossBulletAnim(false);
    }

    private IEnumerator CounterClockwiseShotPattern()
    {
        bossAnim.BossBulletAnim(true);

        float currentAngle = 210f;

        for (int i = 0; i < 15; ++i)
        {
            if (BossBulletPrefab != null)
            {
                Quaternion rot = Quaternion.Euler(0, 0, currentAngle);

                Instantiate(BossBulletPrefab, FirePoint.position, rot);
            }

            currentAngle -= 8.5f;

            yield return new WaitForSeconds(0.1f);
        }

        bossAnim.BossBulletAnim(false);
    }

    private IEnumerator LaserBeamPattern()
    {
        bossAnim.BossLaserAnim(true);
        bossAnim.BossLaserBeam(true);

        GameObject spawnedLaser = null;

        yield return new WaitForSeconds(0.5f);

        if (LaserBeamObject != null)
        {
            spawnedLaser = Instantiate
                (LaserBeamObject, FirePoint.position, Quaternion.identity, transform);

            spawnedLaser.transform.SetParent(FirePoint);

            spawnedLaser.transform.localPosition = Vector3.zero;

            Animator laserAnimator = spawnedLaser.GetComponentInChildren<Animator>();
        }

        yield return new WaitForSeconds(2.0f);

        if (spawnedLaser != null)
        {
            Destroy(spawnedLaser);
        }

        bossAnim.BossLaserAnim(false);
        bossAnim.BossLaserBeam(false);
    }

    // 보스의 하이브리드 공격 패턴 로직
    void FireBossWeapon()
    {
        if (attackPattern == 0)
        {
            StartCoroutine(BurstShotPattern());
        }
        else if (attackPattern == 1)
        {
            StartCoroutine(ClockwiseShotPattern());
        }
        else if (attackPattern == 2)
        {
            StartCoroutine(CounterClockwiseShotPattern());
        }
        else if (attackPattern == 3)
        {
            StartCoroutine(LaserBeamPattern());
            // 패턴 B: 보스 전용 미사일 일직선 하방 발사! (살짝 벌려진 2개 열로 뿜어내기)
            //if (BossMissilePrefab != null)
            //{
            //    Quaternion downRot = Quaternion.Euler(0, 0, 180f);
            //    Vector3 leftWing = transform.position + new Vector3(-0.5f, -0.5f, 0f);
            //    Vector3 rightWing = transform.position + new Vector3(0.5f, -0.5f, 0f);

            //    Instantiate(BossMissilePrefab, leftWing, downRot);
            //    Instantiate(BossMissilePrefab, rightWing, downRot);
            //}
        }
        patternRepeatCount += 1;

        if (patternRepeatCount == 3)
        {
            patternRepeatCount = 0;

            attackPattern = Random.Range(0, 4);

            lastFireTime = Time.time + 0.5f;
        }

        //else if (attackPattern == 3)
        //{

        //}

        else
        {
            lastFireTime = Time.time + 0.05f;
        }
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