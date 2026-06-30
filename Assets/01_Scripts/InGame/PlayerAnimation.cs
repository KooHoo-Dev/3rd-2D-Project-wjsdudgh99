using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAnimation : MonoBehaviour
{
    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (Keyboard.current.leftArrowKey.isPressed)
        {
            anim.SetInteger("Direction", -1);
        }

        else if (Keyboard.current.rightArrowKey.isPressed)
        {
            anim.SetInteger("Direction", 1);
        }

        else
        {
            anim.SetInteger("Direction", 0);
        }
    }
}
