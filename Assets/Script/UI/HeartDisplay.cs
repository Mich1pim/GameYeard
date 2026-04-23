using UnityEngine;
using UnityEngine.UI;

public class HeartDisplay : MonoBehaviour
{
    [Header("Спрайты")]
    [SerializeField] private Sprite fullHeart;
    [SerializeField] private Sprite halfHeart;
    [SerializeField] private Sprite emptyHeart;

    [Header("Иконки сердец (слева направо)")]
    [SerializeField] private Image[] hearts;

    private int _lastHealth = -1;

    private void Update()
    {
        if (PlayerHealth.Instance == null) return;

        int current = PlayerHealth.Instance.CurrentHealth;
        if (current == _lastHealth) return;

        _lastHealth = current;
        UpdateHearts(current);
    }

    private void UpdateHearts(int health)
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] == null) continue;

            int remaining = health - i * 2;

            if (remaining >= 2)
                hearts[i].sprite = fullHeart;
            else if (remaining == 1)
                hearts[i].sprite = halfHeart;
            else
                hearts[i].sprite = emptyHeart;
        }
    }
}
