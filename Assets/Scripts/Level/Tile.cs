using DG.Tweening;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private Color mainColor, darkenedColor;

    public bool HasRightNeighbor
    {
        get { return _hasRightNeighbor;}
        set
        {
            _hasRightNeighbor = value;
            Blackout();
        }
    }

    public bool HasLeftNeighbor
    {
        get { return _hasLeftNeighbor;}
        set
        {
            _hasLeftNeighbor = value;
            Blackout();
        }
    }

    public bool HasUpNeighbor
    {
        get { return _hasUpNeighbor;}
        set
        {
            _hasUpNeighbor = value;
            Blackout();
        }
    }

    public int Layer => _layer;
    public TileType Type => _tileType;
    public MeshRenderer MeshRenderer => meshRenderer;
    public bool IsActivate => _isActivate;

    private bool _hasRightNeighbor;
    private bool _hasLeftNeighbor;
    private bool _hasUpNeighbor;
    private int _layer;
    private TileType _tileType = TileType.None;
    private Sprite _icon;
    private bool _isActivate;

    public void Init(int layer)
    {
        _layer = layer;
        _isActivate = true;
        transform.DOKill();
        transform.localScale = Vector3.one;
    }
    
    public void Deactivate()
    {
        _isActivate = false;
        
        transform.DOScale(Vector3.zero, 0.3f).OnComplete(() =>
        {
            gameObject.SetActive(false);
        });
    }

    public void SetIcon(TileType type, Sprite icon)
    {
        _tileType = type;
        _icon = icon;
        Material material = meshRenderer.material;
        material.SetTexture("_Sprite", _icon.texture);
        meshRenderer.material = material;
    }

    private void Blackout()
    {
        int count = 0;
        if (HasLeftNeighbor) count++;
        if (HasRightNeighbor) count++;
        if (HasUpNeighbor) count++;
        
        Material material = meshRenderer.material;
        material.SetColor("_Color", count >= 2 || HasUpNeighbor ? darkenedColor : mainColor);
        meshRenderer.material = material;
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = HasRightNeighbor ? Color.red : Color.green;
        Gizmos.DrawWireCube(meshRenderer.transform.position + Vector3.right * meshRenderer.transform.localScale.x/2, Vector3.one * 0.1f);
        
        Gizmos.color = HasLeftNeighbor ? Color.red : Color.green;
        Gizmos.DrawWireCube(meshRenderer.transform.position + Vector3.left * meshRenderer.transform.localScale.x/2, Vector3.one * 0.1f);
        
        Gizmos.color = HasUpNeighbor ? Color.red : Color.green;
        Gizmos.DrawWireCube(meshRenderer.transform.position + Vector3.up * meshRenderer.transform.localScale.y/2, Vector3.one * 0.1f);
    }
}
