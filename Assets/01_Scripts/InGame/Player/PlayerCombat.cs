using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    // public 필드는 Microsoft 컨벤션(PascalCase) 준수
    public float Speed = 1.0f;
    public GameObject BulletPrefab; // 총알 프리팹을 담을 변수
    public GameObject DeathEffectPrefab;


    // 자동 연사를 위한 타이머 변수
    public float FireRate = 0.2f;   // 총알 발사 간격 (0.2초에 한 번씩 발사)
    private float lastFireTime = 0f; // 마지막으로 총알을 발사한 시각을 저장

    private SpriteRenderer spriteRenderer;
    private bool isColliding = false;

    public int PlayerHp = 5;

    public bool IsDead = false;

    public int WeaponLevel = 1;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        GameManager.Instance.UpdateLife(PlayerHp);
    }

    void Update()
    {
        Vector3 moveVector = new Vector3(0, 0, 0);

        if (Keyboard.current.upArrowKey.isPressed)
        {
            moveVector.y = moveVector.y + 1f;
        }

        if (Keyboard.current.downArrowKey.isPressed)
        {
            moveVector.y = moveVector.y - 1f;
        }

        if (Keyboard.current.leftArrowKey.isPressed)
        {
            moveVector.x = moveVector.x - 1f;
        }

        if (Keyboard.current.rightArrowKey.isPressed)
        {
            moveVector.x = moveVector.x + 1f;
        }

        moveVector = moveVector * Speed * Time.deltaTime;
        transform.position += moveVector;

        // [새로 추가된 로직] 플레이어 화면 밖 이탈 방지 (화면 가두기)
        // 일반적인 16:9 해상도의 종스크롤 슈팅 기준 안전 구역입니다.
        // Mathf.Clamp(현재값, 최소값, 최대값) -> 현재값이 최소와 최대 범위를 절대 벗어나지 못하게 꽉 잡아줍니다.
        float clampedX = Mathf.Clamp(transform.position.x, -2.8f, 2.8f);
        float clampedY = Mathf.Clamp(transform.position.y, -4.8f, 4.8f);

        // 제한된 좌표를 플레이어 위치에 다시 강제로 대입합니다.
        transform.position = new Vector3(clampedX, clampedY, 0f);

        // 2. 꾹 누르면 자동 연사되는 총알 발사 로직
        if (Keyboard.current.spaceKey.isPressed)
        {
            // 현재 게임 시간이 [마지막 발사 시간 + 연사 간격]보다 커졌는지 확인
            if (Time.time >= lastFireTime + FireRate)
            {
                if (BulletPrefab != null)
                {
                    // 플레이어의 현재 위치에 총알 생성 및 발사 시간 기록

                    if (WeaponLevel == 1)
                    {
                        Instantiate(BulletPrefab, transform.position, Quaternion.identity);
                    }

                    else if (WeaponLevel == 2)
                    {
                        Instantiate(BulletPrefab, transform.position + new Vector3(-0.1f, 0f, 0f), Quaternion.identity);
                        Instantiate(BulletPrefab, transform.position + new Vector3(0.1f, 0f, 0f), Quaternion.identity);
                    }

                    else if (WeaponLevel == 3)
                    {
                        Instantiate(BulletPrefab, transform.position, Quaternion.identity);
                        Instantiate(BulletPrefab, transform.position + new Vector3(-0.2f, 0f, 0f), Quaternion.identity);
                        Instantiate(BulletPrefab, transform.position + new Vector3(0.2f, 0f, 0f), Quaternion.identity);
                    }

                    lastFireTime = Time.time;
                }
                else
                {
                    Debug.LogWarning("PlayerCombat: BulletPrefab이 인스펙터 창에 연결되지 않았습니다!");
                }
            }
        }
    }

    // 3. 충돌 처리 (장애물이나 적 비행기와 부딪혔을 때)
    private void OnTriggerStay2D(Collider2D collider)
    {
        // 3. 아이템 획득
        if (collider.gameObject.CompareTag("Item"))
        {
            Debug.Log("총알 강화 아이템을 먹음");
            WeaponLevel += 1;
            if (WeaponLevel > 3) WeaponLevel = 3;
            Destroy(collider.gameObject);
        }

        // 4. 체력 회복 아이템 획득
        if (collider.gameObject.CompareTag("HpItem"))
        {
            Debug.Log("체력 회복 아이템을 먹음");
            PlayerHp += 1;
            if (PlayerHp > 9) PlayerHp = 9;
            Destroy(collider.gameObject);
            GameManager.Instance.UpdateLife(PlayerHp);
        }

        // 최상단 무적/사망 방어벽
        if (IsDead || isColliding) return;

        // 1. 장애물이나 적 비행기와 부딪혔을 때
        if (collider.gameObject.CompareTag("Obstacle") || collider.gameObject.CompareTag("Enemy"))
        {
            Destroy(collider.gameObject);
            ProcessDamage();
            return; // 처리 후 탈출
        }

        // 2. 보스 레이저에 맞았을 때
        if (collider.gameObject.CompareTag("BossLaser"))
        {
            Debug.Log("레이저에 맞음");
            ProcessDamage();
            return; // 처리 후 탈출
        }
    }

    private void ProcessDamage()
    {
        isColliding = true;
        PlayerHp -= 1;
        GameManager.Instance.UpdateLife(PlayerHp);

        if (PlayerHp <= 0)
        {
            Debug.Log("죽음");
            IsDead = true;

            if (DeathEffectPrefab != null)
            {
                GameObject eff = Instantiate(DeathEffectPrefab, transform.position, Quaternion.identity);

                eff.transform.position =
                    new Vector3(eff.transform.position.x, eff.transform.position.y, 0f);

                Animator effAnim = eff.GetComponent<Animator>();
                if (effAnim != null)
                {
                    effAnim.Play("Player DeathEffect", 0, 0.0f);
                }
            }

            GameManager.Instance.TriggerGameOver();

            Destroy(gameObject);
        }
        else
        {
            Debug.Log("체력이 1 까임");
            StartCoroutine(CollisionEffect());
        }
    }

    // 4. 피격 시 무적/깜빡임 연출 코루틴
    private IEnumerator CollisionEffect()
    {
        isColliding = true;

        Color origin = spriteRenderer.color;
        Color effect = spriteRenderer.color;
        effect.a = 0.2f; // 알파값을 흐리게 변경

        float term = 0.1f;

        for (int i = 0; i < 10; ++i)
        {
            spriteRenderer.color = effect;
            yield return new WaitForSeconds(term);
            spriteRenderer.color = origin;
            yield return new WaitForSeconds(term);
        }

        isColliding = false;
    }
}