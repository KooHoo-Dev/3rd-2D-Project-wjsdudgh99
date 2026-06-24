using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public float Speed = 5.0f;

    void Update()
    {
        // 총알이 생성된 방향(로컬 앞방향인 transform.up)을 기준으로 직선 이동합니다.
        // 이렇게 해야 3갈래로 회전되어 생성되었을 때 각자 부채꼴 방향으로 날아갑니다!
        transform.position += transform.up * Speed * Time.deltaTime;

        // 화면 밖으로 나가면 삭제
        if (transform.position.y < -6.0f || transform.position.x < -4.0f || transform.position.x > 4.0f)
        {
            Destroy(gameObject);
        }
    }
}