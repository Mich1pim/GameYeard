using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SlimeSpawner : MonoBehaviour
{
    public static SlimeSpawner Instance { get; private set; }

    [Header("Слайм")]
    [SerializeField] private GameObject slimePrefab;

    [Header("TileMap для спавна")]
    [SerializeField] private Tilemap spawnTilemap;

    [Header("Лимиты")]
    [SerializeField] private int maxSlimes = 10;

    [Header("Тайминги")]
    [SerializeField] private float initialSpawnDelay = 5f;    // задержка первого спавна в начале ночи
    [SerializeField] private float fillSpawnInterval = 20f;   // интервал пополнения до лимита
    [SerializeField] private float respawnAfterKill = 30f;    // задержка после убийства слайма

    private const int MaxSpawnAttempts = 50;

    private readonly List<SlimeEnemy> _slimes = new List<SlimeEnemy>();
    private bool _wasNight;
    private float _fillTimer;
    private bool _respawnPending;
    private float _respawnTimer;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (GlobalTime.Instance == null) return;

        bool isNight = GlobalTime.Instance.IsNight();

        if (!_wasNight && isNight)
            OnNightStart();

        if (_wasNight && !isNight)
            OnNightEnd();

        _wasNight = isNight;

        if (!isNight) return;

        _slimes.RemoveAll(s => s == null);

        // Постепенно заполняем до лимита
        if (_slimes.Count < maxSlimes)
        {
            _fillTimer -= Time.deltaTime;
            if (_fillTimer <= 0)
            {
                SpawnSlime();
                _fillTimer = fillSpawnInterval;
            }
        }

        // Отдельный таймер на респавн после смерти
        if (_respawnPending)
        {
            _respawnTimer -= Time.deltaTime;
            if (_respawnTimer <= 0)
            {
                _respawnPending = false;
                if (_slimes.Count < maxSlimes)
                    SpawnSlime();
            }
        }
    }

    private void OnNightStart()
    {
        _fillTimer = initialSpawnDelay;
        _respawnPending = false;
    }

    private void OnNightEnd()
    {
        _slimes.RemoveAll(s => s == null);
        foreach (SlimeEnemy slime in _slimes)
        {
            if (slime != null) slime.DespawnImmediate();
        }
        _slimes.Clear();
        _respawnPending = false;
    }

    public void OnSlimeDied(SlimeEnemy slime)
    {
        _slimes.Remove(slime);
        if (GlobalTime.Instance != null && GlobalTime.Instance.IsNight() && _slimes.Count < maxSlimes)
        {
            _respawnPending = true;
            _respawnTimer = respawnAfterKill;
        }
    }

    public SlimeSpawnData[] GetSaveData()
    {
        _slimes.RemoveAll(s => s == null);
        var result = new SlimeSpawnData[_slimes.Count];
        for (int i = 0; i < _slimes.Count; i++)
        {
            result[i] = new SlimeSpawnData
            {
                posX = _slimes[i].transform.position.x,
                posY = _slimes[i].transform.position.y
            };
        }
        return result;
    }

    public void LoadSlimes(SlimeSpawnData[] data)
    {
        if (data == null || slimePrefab == null || data.Length == 0) return;
        foreach (SlimeSpawnData sd in data)
        {
            Vector2 pos = new Vector2(sd.posX, sd.posY);
            GameObject go = Instantiate(slimePrefab, pos, Quaternion.identity);
            SlimeEnemy enemy = go.GetComponent<SlimeEnemy>();
            if (enemy != null) _slimes.Add(enemy);
        }
        // Не запускаем таймер заполнения — слаймы уже восстановлены
        _fillTimer = fillSpawnInterval;
        _respawnPending = false;
        Debug.Log($"[SlimeSpawner] Загружено {data.Length} слаймов из сохранения");
    }

    private void SpawnSlime()
    {
        if (slimePrefab == null) return;
        Vector2 pos;
        if (!TryGetTilemapPosition(out pos)) return;
        GameObject go = Instantiate(slimePrefab, pos, Quaternion.identity);
        SlimeEnemy enemy = go.GetComponent<SlimeEnemy>();
        if (enemy != null)
            _slimes.Add(enemy);
    }

    private bool TryGetTilemapPosition(out Vector2 result)
    {
        if (spawnTilemap == null)
        {
            Debug.LogWarning("SlimeSpawner: Tilemap не назначен в инспекторе!");
            result = Vector2.zero;
            return false;
        }

        BoundsInt bounds = spawnTilemap.cellBounds;

        for (int attempt = 0; attempt < MaxSpawnAttempts; attempt++)
        {
            int x = Random.Range(bounds.xMin, bounds.xMax);
            int y = Random.Range(bounds.yMin, bounds.yMax);
            Vector3Int cell = new Vector3Int(x, y, 0);

            if (spawnTilemap.HasTile(cell))
            {
                result = spawnTilemap.GetCellCenterWorld(cell);
                return true;
            }
        }

        Debug.LogWarning("SlimeSpawner: не удалось найти тайл для спавна за " + MaxSpawnAttempts + " попыток.");
        result = Vector2.zero;
        return false;
    }
}
