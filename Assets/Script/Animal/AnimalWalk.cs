using UnityEngine;
using UnityEngine.AI;

public class AnimalWalk : MonoBehaviour
{
    [SerializeField] private State startState;
    [SerializeField] private float roamingMax = 7f;
    [SerializeField] private float roamingMin = 4f;
    [SerializeField] private float roamingTimeMax = 2f;

    private NavMeshAgent navMeshAgent;
    private Animator animator;
    private State state;
    private float roamingTime;

    private Vector3 roamPosition;
    private Vector3 startPosition;

    private enum State
    {
        Idle,
        Roaming
    }

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        if (navMeshAgent != null)
        {
            navMeshAgent.updateRotation = false;
            navMeshAgent.updateUpAxis = false;
        }
        state = startState;
    }

    private void Start()
    {
        startPosition = transform.position;
        if (state == State.Roaming)
        {
            StartRoaming();
        }
    }

    private void Update()
    {
        if (state != State.Roaming) return;

        roamingTime -= Time.deltaTime;

        bool hasReached = HasReachedDestination();
        bool isMoving = hasReached ? false : navMeshAgent != null && navMeshAgent.velocity.sqrMagnitude > 0.01f;

        if (hasReached && navMeshAgent != null)
        {
            if (roamingTime <= 0)
            {
                navMeshAgent.SetDestination(GetRoamingPosition());
                roamingTime = roamingTimeMax;
                isMoving = true;
            }
        }

        UpdateAnimator(isMoving);
        UpdateSpriteFlip();
    }

    private bool HasReachedDestination()
    {
        return navMeshAgent != null &&
               !navMeshAgent.pathPending &&
               navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance &&
               (!navMeshAgent.hasPath || navMeshAgent.velocity.sqrMagnitude == 0f);
    }

    private void StartRoaming()
    {
        roamingTime = 0;
    }

    private Vector3 GetRoamingPosition()
    {
        return startPosition + GetRandomDir() * Random.Range(roamingMin, roamingMax);
    }

    private Vector3 GetRandomDir()
    {
        return new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
    }

    private void UpdateAnimator(bool isRoaming)
    {
        if (animator != null)
        {
            animator.SetBool("IsRoaming", isRoaming);
        }
    }

    private void UpdateSpriteFlip()
    {
        if (navMeshAgent == null) return;

        float velocityX = navMeshAgent.velocity.x;
        float threshold = 0.1f;

        if (Mathf.Abs(velocityX) < threshold) return;

        Vector3 localScale = transform.localScale;
        localScale.x = velocityX > 0 ? Mathf.Abs(localScale.x) : -Mathf.Abs(localScale.x);
        transform.localScale = localScale;
    }
}