using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EasyButtons;
using UnityEngine;

[Serializable]
public struct TileData
{
    public int layer;
    public float x, z;
}

[Serializable]
public struct LevelData
{
    public string levelName;
    public TileData[] tiles;
}

public class LevelExporter : MonoBehaviour
{
    [SerializeField] private string levelName;
    [SerializeField] private Transform[] layers;
    
    [Button]
    private void Export()
    {
        LevelData level = new LevelData();
        level.levelName = levelName;

        List<(int, Vector2)> tiles = new List<(int, Vector2)>();

        for (int i = 0; i < layers.Length; i++)
        {
            List<Vector2> tempTiles = new List<Vector2>();
            
            for (int j = 0; j < layers[i].childCount; j++)
            {
                Vector2 pos = new Vector2(
                    layers[i].GetChild(j).localPosition.x, 
                    layers[i].GetChild(j).localPosition.z);
                
                tempTiles.Add(pos);
            }

            var sortedTempTiles = tempTiles.OrderBy(b => b.x + b.y * 1000);
            foreach (var vector2 in sortedTempTiles)
            {
                tiles.Add((i, vector2));
            }
        }

        if (tiles.Count % 2 == 0)
        {
            level.tiles = new TileData[tiles.Count];
            for (int i = 0; i < tiles.Count; i++)
            {
                level.tiles[i] = new TileData
                {
                    layer = tiles[i].Item1,
                    x = tiles[i].Item2.x,
                    z = tiles[i].Item2.y,
                };
            }

            string json = JsonUtility.ToJson(level, true);
            File.WriteAllText(Application.dataPath + $"/Levels/{levelName}.json", json);
            Debug.Log($"JSON saved! (Number of tiles: {tiles.Count})");
        }
        else
        {
            Debug.LogError($"An odd number of tiles per level! (Number of tiles: {tiles.Count})");
        }
    }
}
