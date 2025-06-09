using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

[Serializable]
public struct TileIcon
{
    public TileType tileType;
    public Sprite icon;
}

public class LevelGenerator : MonoBehaviour
{
    [SerializeField] private TextAsset[] jsonFiles;
    [SerializeField] private Pool pool;
    [SerializeField] private TileIcon[] tileIcons;
    [Space] 
    [SerializeField] private float layerHeight;
    [Space]
    [SerializeField] private float overlapThreshold = 0.1f;
    [SerializeField] private float zAlignmentThreshold = 0.1f;
    [Space] 
    [SerializeField] private Button shufflingButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button autoButton;
    
    private List<Tile> _allTiles = new List<Tile>();
    private float _height;
    private Dictionary<int, List<Tile>> _tilePairs = new Dictionary<int, List<Tile>>();
    private int _pairIdCounter;
    private TextAsset _currentLevel;
    
    private void Start()
    {
        _currentLevel = jsonFiles[Random.Range(0, jsonFiles.Length)];
        
        LoadLevelFromJson();
        CheckNeighbors();
        AssignTileIcons();
        
        shufflingButton.onClick.RemoveAllListeners();
        shufflingButton.onClick.AddListener(AssignTileIcons);
        restartButton.onClick.RemoveAllListeners();
        restartButton.onClick.AddListener(RestartLevel);
        autoButton.onClick.RemoveAllListeners();
        autoButton.onClick.AddListener(StartAuto);
    }

    void LoadLevelFromJson()
    {
        LevelData levelData = JsonUtility.FromJson<LevelData>(_currentLevel.text);
        int layer = 0;
        
        foreach (TileData tileData in levelData.tiles)
        {
            if (layer < tileData.layer)
            {
                _height += layerHeight;
                layer = tileData.layer;
            }
                
            Vector3 position = new Vector3(tileData.x, _height, tileData.z);
            Tile tile = pool.GetTile();
            tile.gameObject.SetActive(true);
            tile.transform.position = position;
            tile.transform.rotation = Quaternion.identity;
            tile.Init(layer);
            _allTiles.Add(tile);
        }
    }
    
    public void CheckNeighbors()
    {
        foreach (Tile tile in _allTiles)
        {
            if(!tile.IsActivate) continue;
            
            CheckHorizontalNeighbors(tile);
            CheckVerticalNeighbors(tile);
        }
    }
    
    private void CheckHorizontalNeighbors(Tile tile)
    {
        tile.HasRightNeighbor = false;
        tile.HasLeftNeighbor = false;
    
        float tileWidth = tile.MeshRenderer.transform.localScale.x;
        float tileRightEdge = tile.transform.position.x + tileWidth * 0.5f;
        float tileLeftEdge = tile.transform.position.x - tileWidth * 0.5f;
        float tileZ = tile.transform.position.z;
        
        var sameLayerTiles = _allTiles.Where(t => t != null && t.Layer == tile.Layer && t != tile);

        foreach (Tile other in sameLayerTiles)
        {
            if(other == tile || !other.IsActivate) continue;
            
            float otherWidth = other.MeshRenderer.transform.localScale.x;
            float otherLeftEdge = other.transform.position.x - otherWidth * 0.5f;
            float otherRightEdge = other.transform.position.x + otherWidth * 0.5f;
            float otherZ = other.transform.position.z;
            
            bool isZAligned = Mathf.Abs(tileZ - otherZ) <= zAlignmentThreshold;

            if (!isZAligned) continue;
            
            if (Mathf.Abs(tileRightEdge - otherLeftEdge) <= overlapThreshold + float.Epsilon)
            {
                tile.HasRightNeighbor = true;
            }
            
            if (Mathf.Abs(tileLeftEdge - otherRightEdge) <= overlapThreshold + float.Epsilon)
            {
                tile.HasLeftNeighbor = true;
            }

            if (tile.HasRightNeighbor && tile.HasLeftNeighbor) break;
        }
    }
    
    private void CheckVerticalNeighbors(Tile tile)
    {
        tile.HasUpNeighbor = false;
        
        Bounds tileBounds = new Bounds(tile.transform.position, tile.MeshRenderer.transform.localScale);
        float topEdge = tileBounds.max.y;
    
        foreach (Tile other in _allTiles)
        {
            if(other == tile || !other.IsActivate) continue;
        
            if (other.Layer == tile.Layer + 1)
            {
                Bounds otherBounds = new Bounds(other.transform.position, other.MeshRenderer.transform.localScale);
                float otherBottomEdge = otherBounds.min.y;
                
                if (Mathf.Abs(topEdge - otherBottomEdge) <= overlapThreshold)
                {
                    bool xOverlap = Mathf.Abs(tile.transform.position.x - other.transform.position.x) < 
                                    (tileBounds.size.x/2f + otherBounds.size.x/2f);
                    bool zOverlap = Mathf.Abs(tile.transform.position.z - other.transform.position.z) < 
                                    (tileBounds.size.z/2f + otherBounds.size.z/2f);
                
                    if (xOverlap && zOverlap)
                    {
                        tile.HasUpNeighbor = true;
                        break;
                    }
                }
            }
        }
    }
    
    private void AssignTileIcons()
    {
        var openTiles = _allTiles.Where(t => CountBlockedSides(t) <= 1 && t.gameObject.activeSelf).ToList();
        var semiOpenTiles = _allTiles.Where(t => CountBlockedSides(t) == 2 && t.gameObject.activeSelf).ToList();
        var closedTiles = _allTiles.Where(t => CountBlockedSides(t) == 3 && t.gameObject.activeSelf).ToList();
        
        CreatePairsForTileGroup(openTiles);
        CreatePairsForTileGroup(semiOpenTiles);
        CreatePairsForTileGroup(closedTiles);
        
        var remainingTiles = new List<Tile>();
        remainingTiles.AddRange(openTiles);
        remainingTiles.AddRange(semiOpenTiles);
        remainingTiles.AddRange(closedTiles);
        
        CreatePairsForTileGroup(remainingTiles);

        Debug.Log($"Total pairs created: {_tilePairs.Count}");
    }
    
    private void CreatePairsForTileGroup(List<Tile> tileGroup)
    {
        Shuffle(tileGroup);

        while (tileGroup.Count > 1)
        {
            Tile firstTile = tileGroup[0];
            tileGroup.RemoveAt(0);
            
            Tile secondTile = null;
            
            secondTile = tileGroup[0];
            tileGroup.RemoveAt(0);

            if (secondTile != null)
            {
                int pairId = _pairIdCounter++;
                _tilePairs[pairId] = new List<Tile> { firstTile, secondTile };
            
                TileIcon icon = tileIcons[Random.Range(0, tileIcons.Length)];
                firstTile.SetIcon(icon.tileType, icon.icon);
                secondTile.SetIcon(icon.tileType, icon.icon);
            }
        }
    }
    
    private void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(i, list.Count);
            (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
        }
    }
    
    public int CountBlockedSides(Tile tile)
    {
        int count = 0;
        if (tile.HasLeftNeighbor) count++;
        if (tile.HasRightNeighbor) count++;
        if (tile.HasUpNeighbor) count++;
        return count;
    }

    private void RestartLevel()
    {
        ClearingLevel();
        LoadLevelFromJson();
        CheckNeighbors();
        AssignTileIcons();
    }

    private void ClearingLevel()
    {
        foreach (var tile in _allTiles)
        {
            tile.gameObject.SetActive(false);
        }
        
        _allTiles.Clear();
        _height = 0;
        _pairIdCounter = 0;
        _tilePairs.Clear();
    }

    public void CheckCompletionLevel()
    {
        var activeTiles = _allTiles.Where(t => t.IsActivate).ToList();

        if (activeTiles.Count == 0)
        {
            _currentLevel = jsonFiles[Random.Range(0, jsonFiles.Length)];
        
            ClearingLevel();
            LoadLevelFromJson();
            CheckNeighbors();
            AssignTileIcons();
        }
    }

    private void StartAuto()
    {
        StartCoroutine(Auto());
    }

    private IEnumerator Auto()
    {
        shufflingButton.interactable = false;
        restartButton.interactable = false;
        autoButton.interactable = false;

        foreach (var pairEntry in _tilePairs)
        {
            List<Tile> tiles = pairEntry.Value;
            foreach (var tile in tiles)
            {
                tile.Deactivate();
            }

            yield return new WaitForSeconds(0.1f);
        }
        
        shufflingButton.interactable = true;
        restartButton.interactable = true;
        autoButton.interactable = true;
        CheckCompletionLevel();
    }
}
