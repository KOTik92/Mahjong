using DG.Tweening;
using UnityEngine;

public class InteractionLevel : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LevelGenerator levelGenerator;

    private Tile _tile;
    
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                hit.collider.TryGetComponent(out Tile tile);
            
                if (tile != null)
                {
                    if (_tile != null && _tile != tile)
                    {
                        _tile.transform.DOLocalMove(
                            new Vector3(
                                _tile.transform.localPosition.x,
                                _tile.transform.localPosition.y - 0.5f,
                                _tile.transform.localPosition.z), 0.5f);
                    }
                    
                    if (tile.HasUpNeighbor || levelGenerator.CountBlockedSides(tile) >= 2)
                    {
                        _tile = null;
                        return;
                    }
                    
                    Debug.Log("Clicked on tile: " + tile.Type);
                    if (_tile != null)
                    {
                        if(_tile == tile) return;
                        
                        if (_tile.Type != tile.Type)
                        {
                            _tile = null;
                            return;
                        }

                        _tile.Deactivate();
                        tile.Deactivate();
                        _tile = null;
                        levelGenerator.CheckNeighbors();
                        levelGenerator.CheckCompletionLevel();
                        return;
                    }

                    _tile = tile;
                    _tile.transform.DOLocalMove(
                        new Vector3(
                            _tile.transform.localPosition.x,
                            _tile.transform.localPosition.y + 0.5f,
                            _tile.transform.localPosition.z), 0.5f);
                }
            }
        }
    }
}
