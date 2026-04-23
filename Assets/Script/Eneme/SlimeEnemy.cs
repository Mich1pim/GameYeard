using UnityEngine;
using UnityEngine.AI;

public class SlimeEnemy : MonoBehaviour
{
    [Header("Характеристики")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private float walkSpeed = 1.2f;
    [SerializeField] private float chaseSpeed = 2.5f;

    [Header("Поведение")]
    [SerializeField] private float detectionRange = 4f;
    [SerializeField] private float attackRange = 0.8f;
    [SerializeField] private float roamRadius = 5f;
    [SerializeField] private float roamInterval = 3f;

    [Header("Урон от игрока")]
    [SerializeField] private float playerHitRange = 1.5f;
    [SerializeField] private float hitCooldown = 0.5f;

    [Header("Урон игроку")]
    [SerializeField] private int damageToPlayer = 1;
    // 0.5 игровой минуты = 0.5 реальной секунды (1 игровая минута = 1 реальная секунда)
    [SerializeField] private float attackPlayerCooldown = 0.5f;

    [Header("Лут")]
    [SerializeField] private GameObject lootPrefab;

    private Animator _anim;
    private NavMeshAgent _nav;
    private Transform _player;
    private int _hp;
    private bool _dead;
    private float _hitTimer;
    private float _roamTimer;
    private float _damagePlayerTimer;
    private Vector3 _spawnPos;

    private enum State { Idle, Roam, Chase, Attack }
    private State _state;

    private void Awake()
    {
        _anim = GetComponent<Animator>();
        _nav = GetComponent<NavMeshAgent>();
        if (_nav != null)
        {
            _nav.updateRotation = false;
            _nav.updateUpAxis = false;
        }
    }

    private void Start()
    {
        _hp = maxHealth;
        _spawnPos = transform.position;
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) _player = p.transform;
    }

    private void Update()
    {
        if (_dead) return;
        CheckPlayerHit();
        UpdateAI();
        UpdateAnimator();
        FlipSprite();
    }

    private void CheckPlayerHit()
    {
        if (_player == null || ItemsUsing.Instance == null) return;
        if (!ItemsUsing.Instance.IsAxe() && !ItemsUsing.Instance.IsPickAxe()) return;
        if (!Input.GetMouseButton(0)) return;

        _hitTimer -= Time.deltaTime;
        if (_hitTimer > 0) return;

        if (Vector2.Distance(transform.position, _player.position) <= playerHitRange)
        {
            _hitTimer = hitCooldown;
            TakeDamage(1);
        }
    }

    private void UpdateAI()
    {
        if (_player == null) return;
        float dist = Vector2.Distance(transform.position, _player.position);

        switch (_state)
        {
            case State.Idle:
                _nav?.ResetPath();
                _roamTimer -= Time.deltaTime;
                if (_roamTimer <= 0) StartRoam();
                if (dist <= detectionRange) _state = State.Chase;
                break;

            case State.Roam:
                if (dist <= detectionRange) { _state = State.Chase; break; }
                if (ReachedDestination()) { _roamTimer = roamInterval; _state = State.Idle; }
                break;

            case State.Chase:
                if (dist > detectionRange * 1.5f) { _state = State.Idle; break; }
                if (dist <= attackRange) { _nav?.ResetPath(); _state = State.Attack; break; }
                if (_nav != null) { _nav.speed = chaseSpeed; _nav.SetDestination(_player.position); }
                break;

            case State.Attack:
                if (dist > attackRange * 1.3f) { _state = State.Chase; break; }
                DamagePlayer();
                break;
        }
    }

    private void StartRoam()
    {
        if (_nav == null) { _state = State.Idle; return; }
        Vector3 dir = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
        Vector3 target = _spawnPos + dir * Random.Range(2f, roamRadius);
        _nav.speed = walkSpeed;
        _nav.SetDestination(target);
        _state = State.Roam;
    }

    private bool ReachedDestination()
    {
        if (_nav == null) return true;
        return !_nav.pathPending
            && _nav.remainingDistance <= _nav.stoppingDistance
            && (!_nav.hasPath || _nav.velocity.sqrMagnitude < 0.01f);
    }

    private void UpdateAnimator()
    {
        if (_anim == null) return;
        _anim.SetBool("Idle", _state == State.Idle);
        _anim.SetBool("Walk", _state == State.Roam || _state == State.Chase);
        _anim.SetBool("Atack", _state == State.Attack);
    }

    private void FlipSprite()
    {
        if (_nav == null) return;
        float vx = _nav.velocity.x;
        if (Mathf.Abs(vx) < 0.1f) return;
        Vector3 s = transform.localScale;
        s.x = vx > 0 ? Mathf.Abs(s.x) : -Mathf.Abs(s.x);
        transform.localScale = s;
    }

    public void TakeDamage(int dmg)
    {
        if (_dead) return;
        _hp -= dmg;
        if (_hp <= 0) Die();
    }

    private void Die()
    {
        _dead = true;
        if (_nav != null) { _nav.ResetPath(); _nav.enabled = false; }
        _anim.SetBool("Idle", false);
        _anim.SetBool("Walk", false);
        _anim.SetBool("Atack", false);
        _anim.SetTrigger("Die");
        SpawnLoot();
        SlimeSpawner.Instance?.OnSlimeDied(this);
        Destroy(gameObject, 2f);
    }

    private void DamagePlayer()
    {
        _damagePlayerTimer -= Time.deltaTime;
        if (_damagePlayerTimer > 0) return;
        _damagePlayerTimer = attackPlayerCooldown;
        PlayerHealth.Instance?.TakeDamage(damageToPlayer);
    }

    private void SpawnLoot()
    {
        if (lootPrefab == null) return;
        Vector2 randomOffset = new Vector2(
            Random.Range(-0.5f, 0.5f),
            Random.Range(-0.5f, 0.5f)
        );
        Vector2 spawnPosition = (Vector2)transform.position + randomOffset;
        Instantiate(lootPrefab, spawnPosition, Quaternion.identity);
    }

    public void DespawnImmediate()
    {
        Destroy(gameObject);
    }
}
