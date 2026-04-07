using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Обновляет отображение денег игрока в UI магазина
/// </summary>
public class ShopMoneyUI : MonoBehaviour
{
    public Text moneyText;
    private int _lastMoney = -1;
    private Player _playerInstance;

    void Start()
    {
        _playerInstance = Player.Instance;
        if (_playerInstance == null)
        {
            Debug.LogWarning("ShopMoneyUI: Player.Instance не найден!");
        }
    }

    void Update()
    {
        if (_playerInstance == null)
        {
            _playerInstance = Player.Instance;
            if (_playerInstance == null) return;
        }

        int currentMoney = _playerInstance.coin;
        if (currentMoney != _lastMoney)
        {
            _lastMoney = currentMoney;
            if (moneyText != null)
            {
                moneyText.text = $"{currentMoney}$";
            }
        }
    }
}
