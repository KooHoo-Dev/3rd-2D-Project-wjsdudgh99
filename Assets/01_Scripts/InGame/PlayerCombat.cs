using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    // public 필드는 Microsoft 컨벤션(PascalCase) 준수
    public float Speed = 1.0f;
    public GameObject BulletPrefab; // 총알 프리팹을 담을 변수


    // 자동 연사를 위한 타이머 변수
    public float FireRate = 0.2f;   // 총알 발사 간격 (0.2초에 한 번씩 발사)
    private float lastFireTime = 0f; // 마지막으로 총알을 발사한 시각을 저장

    private SpriteRenderer spriteRenderer;
    private bool isColliding = false;

    public int PlayerHp = 5;

    public bool IsDead = false;

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
                    Instantiate(BulletPrefab, transform.position, Quaternion.identity);
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
    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("Obstacle") || collider.gameObject.CompareTag("Enemy"))
        {
            if (IsDead == true)
            {
                return;
            }

            if (isColliding == true)
            {
                return;
            }

            isColliding = true;

            PlayerHp -= 1;

            GameManager.Instance.UpdateLife(PlayerHp);

            Destroy(collider.gameObject);

            if (PlayerHp <= 0)
            {
                IsDead = true;

                GameManager.Instance.TriggerGameOver();

                Destroy(gameObject);

                return;
            }

            if (PlayerHp > 0)
            {
                StartCoroutine(CollisionEffect());
            }
        }

        //if (CompareTag = Item)
        //{
        //    WeaponLevel += 1;
        //}
        
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