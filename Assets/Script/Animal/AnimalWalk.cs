using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class AnimalWalk : MonoBehaviour
{
    [SerializeField] private State startState;
    [SerializeField] private float roamingMax = 7f;
    [SerializeField] private float roamingMin = 4f;
    [SerializeField] private float roamingTimeMax = 2f;
    [SerializeField] private bool isRoaming = false;

    private NavMeshAgent navMeshAgent;
    private Animator animator;
    private State state;
    private float roamingTime;

    private UnityEngine.Vector3 roamPosition;
    private UnityEngine.Vector3 startPosition;

    private enum State
    {
        Idle,
        Roaming
    }

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        navMeshAgent.updateRotation = false;
        navMeshAgent.updateUpAxis = false;
        state = startState;
    }

    private void Start()
    {
        startPosition = transform.position;
        UpdateAnimator();
    }

    private void Update()
{
    switch (state)
    {
        case State.Roaming:
            roamingTime -= Time.deltaTime;
            if (HasReachedDestination())
            {
                isRoaming = false;
                
                if (roamingTime < 0)
                {
                    Roaming();
                    roamingTime = roamingTimeMax;
                }
            }
            else
            {
                isRoaming = true;
            }
            break;
    }

    UpdateAnimator();
    UpdateSpriteFlip(); // Этот метод теперь будет вызываться каждый кадр, 
                        // но с порогом чувствительности проблема дрожания решена
}

    private bool HasReachedDestination()
    {
        return !navMeshAgent.pathPending 
                && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance 
                && (!navMeshAgent.hasPath || navMeshAgent.velocity.sqrMagnitude == 0f);
    }

    private void Roaming()
    {
        roamPosition = GetRoamingPosition();
        navMeshAgent.SetDestination(roamPosition);
        isRoaming = true;
    }

    private UnityEngine.Vector3 GetRoamingPosition()
    {
        return startPosition + GetRandomDir() * UnityEngine.Random.Range(roamingMin, roamingMax);
    }

    private UnityEngine.Vector3 GetRandomDir()
    {
        return new UnityEngine.Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
    }

    private void UpdateAnimator()
    {
        if (animator != null)
        {
            animator.SetBool("IsRoaming", isRoaming);
        }
    }
    private void UpdateSpriteFlip()
{
    if (navMeshAgent.hasPath && navMeshAgent.velocity != UnityEngine.Vector3.zero)
    {
        float velocityX = navMeshAgent.velocity.x;
        float threshold = 0.1f;
        
        // Получаем текущий масштаб
        UnityEngine.Vector3 localScale = transform.localScale;
        
        if (velocityX < -threshold)
        {
            // Движение влево
            localScale.x = -Mathf.Abs(localScale.x);
            transform.localScale = localScale;
        }
        else if (velocityX > threshold)
        {
            // Движение вправо
            localScale.x = Mathf.Abs(localScale.x);
            transform.localScale = localScale;
        }
        // Если velocity.x близка к нулю, не меняем масштаб
    }
}
}