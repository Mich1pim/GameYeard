using UnityEngine;

public class ProximityVisibility : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Объект, который будет показываться/скрываться")]
    public GameObject targetObject;
    
    [Tooltip("Расстояние активации")]
    public float activationDistance = 1.5f;
    
    [Tooltip("Тег игрока")]
    public string playerTag = "Player";
    
    private Transform player;
    private bool isVisible = false;

    void Start()
    {
        // Находим игрока по тегу
        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        
        // Скрываем объект в начале
        if (targetObject != null)
        {
            targetObject.SetActive(false);
            isVisible = false;
        }
    }

    void Update()
    {
        if (player == null || targetObject == null)
            return;
        
        // Вычисляем расстояние до игрока
        float distance = Vector3.Distance(transform.position, player.position);
        
        // Проверяем, нужно ли показать или скрыть объект
        if (distance <= activationDistance && !isVisible)
        {
            targetObject.SetActive(true);
            isVisible = true;
        }
        else if (distance > activationDistance && isVisible)
        {
            targetObject.SetActive(false);
            isVisible = false;
        }
    }

    // Визуализация радиуса в редакторе
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, activationDistance);
    }
}
