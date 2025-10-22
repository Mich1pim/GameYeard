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
        navMeshAgent.updateRotation = false;
        navMeshAgent.updateUpAxis = false;
        state = startState;
    }

    private void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        switch (state)
        {
            default:
            case State.Idle:
                break;
            case State.Roaming:
                roamingTime -= Time.deltaTime;
                if (roamingTime < 0)
                {
                    Roaming();
                    roamingTime = roamingTimeMax;
                }
                break;
        }
    }

    private void Roaming()
    {
        roamPosition = GetRoamingPosition();
        navMeshAgent.SetDestination(roamPosition);
        if (transform.position == roamPosition)
        {
            isRoaming = false;
        }
        else
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
}
