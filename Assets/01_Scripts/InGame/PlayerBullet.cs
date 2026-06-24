using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerBullet : MonoBehaviour
{
    public float Speed = 8.0f;
    public int Damage = 1;
    public GameObject BulletPrefab;

    void Update()
    {
        Vector3 moveVector = new Vector3(0, 1f, 0);
        moveVector = moveVector * Speed * Time.deltaTime;
        transform.position += moveVector;

        if (transform.position.y > 6.0f)
        {
            Destroy(gameObject);
        }

        // 스페이스바가 이번 프레임에 눌렸다면 총알 생성 (인풋 시스템 방식)
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            // 현재 플레이어 위치(transform.position)에 회전 없이(Quaternion.identity) 총알 프리팹 생성
            Instantiate(BulletPrefab, transform.position, Quaternion.identity);
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("Enemy") || collider.gameObject.CompareTag("Boss"))
        {
            Destroy(gameObject);
        }
    }
}