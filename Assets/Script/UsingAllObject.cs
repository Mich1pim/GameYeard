using UnityEngine;

public class UsingAllObject : MonoBehaviour
{
    protected Animator animator;
    protected float distance;
    protected bool isOpened = false;
    protected float interactionDistance = 1.0f;
    protected KeyCode interactionKey = KeyCode.E;

    private Transform _playerTransform;

    protected virtual void Awake()
    {
        GameObject player = GameObject.Find("Player");
        if (player != null)
            _playerTransform = player.transform;
        else
            Debug.LogError($"UsingAllObject на {gameObject.name}: объект Player не найден в сцене!");
    }

    protected virtual void Start()
    {
        animator = gameObject.GetComponent<Animator>();
    }

    protected virtual void Update()
    {
        if (_playerTransform == null) return;

        distance = Vector3.Distance(_playerTransform.position, transform.position);
        if (Input.GetKeyDown(interactionKey) && distance < interactionDistance)
        {
            if (isOpened)
                UnUse();
            else
                Use();
        }
    }

    protected virtual void Use()
    {
        isOpened = true;
    }

    protected virtual void UnUse()
    {
        isOpened = false;
    }
}
