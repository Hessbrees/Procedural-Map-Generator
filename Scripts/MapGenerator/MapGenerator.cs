using UnityEngine;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour
{
    [SerializeField] private MapSprites mapSprites;
    [SerializeField] private MapGenerationSettings mapGenerationSettings;
    [SerializeField] private GameObject tilePrefab;

    public Transform dungeonParent;

    private Dictionary<Vector2Int, TileData> mapTiles = new Dictionary<Vector2Int, TileData>();
    private Dictionary<Vector2Int, TileData> dungeonTiles = new Dictionary<Vector2Int, TileData>();
    TerrainGenerator terrainGenerator;
    StructureGenerator structureGenerator;
    DungeonGenerator dungeonGenerator;
    public void GenerateMap()
    {
        if (mapSprites == null)
        {
            Debug.LogError("MapSprites reference is missing!");
            return;
        }

        if (mapGenerationSettings == null)
        {
            Debug.LogError("MapGenerationSettings reference is missing!");
            return;
        }

        CreateEmptyTilesList();
        GenerateTerrain();
        GenerateStructures();

        GenerateDungeon();
    }
    private void GenerateDungeon()
    {
        var dungeonData = CreateDungeonData();
        dungeonGenerator = new DungeonGenerator(mapGenerationSettings, mapSprites, mapGenerationSettings.seed, dungeonTiles,this);
        dungeonGenerator.GenerateDungeon(dungeonData.x0, dungeonData.y0, dungeonData.xmax, dungeonData.ymax);
    }
    private void GenerateTerrain()
    {
        terrainGenerator = new TerrainGenerator(mapGenerationSettings, mapSprites, mapGenerationSettings.seed, mapTiles);
        terrainGenerator.GenerateMap(mapTiles);
        terrainGenerator.GenerateEnvironment();
    }
    private void GenerateStructures()
    {
        structureGenerator = new StructureGenerator(mapGenerationSettings, mapSprites, mapGenerationSettings.seed, mapTiles, terrainGenerator.dirtTiles, terrainGenerator.grassTiles);
        structureGenerator.GenerateStructures();
    }
    private void CreateEmptyTilesList()
    {
        for(int x = 0; x < mapGenerationSettings.mapWidth; x++)
        {
            for(int y = 0; y < mapGenerationSettings.mapHeight; y++)
            {
                mapTiles[new Vector2Int(x, y)] = GenerateTile(x, y);
            }
        }
    }

    private DungeonData CreateDungeonData()
    {
        int dungeonStartingPoint = mapGenerationSettings.mapWidth + MapGeneratorConsts.DUNGEON_STARTING_POINT;
        int dungeonEndPoint = dungeonStartingPoint + mapGenerationSettings.dungeonWidth;

        return new DungeonData{
            x0 = dungeonStartingPoint,
            y0 = 0,
            xmax = dungeonEndPoint,
            ymax = mapGenerationSettings.dungeonHeight
        };
    }
    private TileData GenerateTile(int x, int y)
    {
        GameObject tile = Instantiate(tilePrefab);
        tile.transform.parent = transform;
        TileData tileData = tile.GetComponent<TileData>();
        tileData.SetTilePosition(x, y);
        return tileData;
    }
    public TileData GenerateDungeonTile(int x, int y)
    {
        GameObject tile = Instantiate(tilePrefab);
        tile.transform.parent = dungeonParent;
        TileData tileData = tile.GetComponent<TileData>();
        tileData.SetTilePosition(x, y);
        return tileData;
    }
    [ContextMenu("Clear Map")]
    public void ClearMap()
    {
        if(mapTiles.Count > 0)
        {
           foreach(var tile in mapTiles.Values)
            {
                DestroyImmediate(tile.gameObject);
            }

            mapTiles.Clear();
        }

        if(transform.childCount > 0)
        {
            List<GameObject> children = new List<GameObject>();

            foreach(Transform child in transform)
            {
                children.Add(child.gameObject);
            }

            foreach(var child in children)
            {
                DestroyImmediate(child);
            }
        }

        ClearDungeon();
    }
    public void ClearDungeon()
    {
        if(dungeonTiles.Count > 0)
        {
            foreach(var tile in dungeonTiles.Values)
            {
                DestroyImmediate(tile.gameObject);
            }

            dungeonTiles.Clear();
        }

        if(dungeonParent.childCount > 0)
        {
            List<GameObject> children = new List<GameObject>();

            foreach(Transform child in dungeonParent)
            {
                children.Add(child.gameObject);
            }

            foreach(var child in children)
            {
                DestroyImmediate(child);
            }
        }
    }
    [ContextMenu("Generate New Map")]
    public void GenerateNewMap()
    {
        GenerateMap();
    }
}

public class DungeonData
{
    public int x0;
    public int y0;
    public int xmax;
    public int ymax;
}