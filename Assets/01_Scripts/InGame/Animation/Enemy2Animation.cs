using UnityEngine;

public class Enemy2Animation : MonoBehaviour
{
    private Animator anim;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void Enemy2Anim(int stateNumber)
    {
        if (anim == null) return;

        anim.SetInteger("Move", stateNumber);
    }
}
