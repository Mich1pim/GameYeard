using UnityEngine;

public class TIme : MonoBehaviour
{
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        animator.SetBool("isDay", GlobalTime.Instance.IsDay());
        animator.SetBool("isNight", GlobalTime.Instance.IsNight());
        animator.SetBool("isMorning", GlobalTime.Instance.IsMorning());
        animator.SetBool("isEvening", GlobalTime.Instance.IsEvening());
    }
}
