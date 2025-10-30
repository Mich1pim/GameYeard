using UnityEngine;

public class Money : MonoBehaviour
{
    public int cost = 5;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Destroy(gameObject);
            Player.Instance.coin += cost;
        }
    }
}
