using UnityEngine;

public class Pool : MonoBehaviour
{
    [SerializeField] private int poolCount = 3;
    [SerializeField] private bool autoExpand = false;
    [SerializeField] private Tile prefab;

    private PoolMono<Tile> _pool;

    private void Awake()
    {
        _pool = new PoolMono<Tile>(prefab, poolCount, transform);
        _pool.AutoExpand = autoExpand;
    }

    public Tile GetTile()
    {
        return _pool.GetFreeElement();
    }
}
