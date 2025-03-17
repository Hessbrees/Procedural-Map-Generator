using UnityEngine;
using System;
using Random = System.Random;
using System.Collections.Generic;
using Unity.Mathematics;

public class TerrainGenerator
{
    private MapGenerationSettings settings;
    private MapSprites mapSprites;
    private Random random;
    // Perlin Noise
    private int[] permutation;
    private int PERMUTATION_SIZE;
    private float PERSISTENCE;
    private int OCTAVES;

    public List<TileData> stoneTiles;
    public List<TileData> grassTiles;
    public List<TileData> dirtTiles;
    public List<TileData> sandTiles;
    public List<TileData> oceanTiles;
    public List<TileData> waterTiles;

    public Dictionary<Vector2Int, TileData> mapTiles;

    float maxNoise = float.MinValue;
    float minNoise = float.MaxValue;
    float[,] noiseMap;
    public TerrainGenerator(MapGenerationSettings settings, MapSprites mapSprites, int seed, Dictionary<Vector2Int, TileData> mapTiles)
    {
        noiseMap = new float[settings.mapWidth, settings.mapHeight];
        this.settings = settings; 
        this.mapSprites = mapSprites; 
        this.mapTiles = mapTiles;
        random = new Random(seed);
        
        PERMUTATION_SIZE = settings.permutationSize;
        PERSISTENCE = settings.persistence;
        OCTAVES = settings.octaves;

        stoneTiles = new List<TileData>();
        grassTiles = new List<TileData>();
        dirtTiles = new List<TileData>();
        sandTiles = new List<TileData>();
        oceanTiles = new List<TileData>();
        waterTiles = new List<TileData>();

        GeneratePermutationTable();

        foreach(int value in permutation)
        {
            Debug.Log(value);
        }
    }


#region Perlin Noise
    private void GeneratePermutationTable()
    {
        permutation = new int[PERMUTATION_SIZE * 2];

        for (int i = 0; i < PERMUTATION_SIZE; i++)
            permutation[i] = i;
            
        for (int i = PERMUTATION_SIZE - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            int temp = permutation[i];
            permutation[i] = permutation[j];
            permutation[j] = temp;
        }
        for (int i = 0; i < PERMUTATION_SIZE; i++)
            permutation[PERMUTATION_SIZE + i] = permutation[i];
    }

    private float Fade(float t)
    {
        return t * t * t * (t * (t * 6 - 15) + 10);
    }

    private float Lerp(float a, float b, float t)
    {
        return a + t * (b - a);
    }

    private float Grad(int hash, float x, float y)
    {
        int h = hash & 15; 
        float u = h < 8 ? x : y; 
        float v = h < 4 ? y : h == 12 || h == 14 ? x : 0;
        return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
    }

    private float GeneratePerlinNoise(float x, float y)
    {
        int X = (int)Math.Floor(x) & PERMUTATION_SIZE - 1;
        int Y = (int)Math.Floor(y) & PERMUTATION_SIZE - 1;

        x -= (float)Math.Floor(x);
        y -= (float)Math.Floor(y);

        float u = Fade(x);
        float v = Fade(y);

        int A = permutation[X] + Y;
        int B = permutation[X + 1] + Y;

        return Lerp(
            Lerp(Grad(permutation[A], x, y),
                 Grad(permutation[B], x - 1, y),
                 u),
            Lerp(Grad(permutation[A + 1], x, y - 1),
                 Grad(permutation[B + 1], x - 1, y - 1),
                 u),
            v);
    }
#endregion
    public void GenerateEnvironment()
    {
        GenerateRandomWalkPathList(GetRandomPositions(stoneTiles, mapSprites.stoneTile), stoneTiles, new List<Sprite> { mapSprites.stoneTile }, mapSprites.stoneTile);
        GenerateRandomWalkPathList(GetRandomPositions(grassTiles, mapSprites.grassTile), grassTiles, new List<Sprite> { mapSprites.grassTile }, mapSprites.grassTile);
        GenerateRandomWalkPathList(GetRandomPositions(dirtTiles, mapSprites.dirtTile), dirtTiles, new List<Sprite> { mapSprites.dirtTile }, mapSprites.dirtTile);
        //GenerateRandomWalkPathList(GetRandomPositions(sandTiles, mapSprites.sandTile), sandTiles, new List<Sprite> { mapSprites.sandTile }, mapSprites.sandTile);
        //GenerateRandomWalkPathList(GetRandomPositions(oceanTiles, mapSprites.oceanTile), oceanTiles, new List<Sprite> { mapSprites.oceanTile }, mapSprites.oceanTile);
        //GenerateRandomWalkPathList(GetRandomPositions(waterTiles, mapSprites.waterTile), waterTiles, new List<Sprite> { mapSprites.waterTile }, mapSprites.waterTile);
    }

    private void GenerateRandomWalkPathList(List<Vector2Int> positions, List<TileData> tiles, List<Sprite> allowedTiles, Sprite spriteSetting)
    {
        if(positions.Count == 0) return;

        foreach(var position in positions)
        {
            GenerateRandomWalkPath(tiles, position, allowedTiles, spriteSetting);
        }
    }
    private List<Vector2Int> GetRandomPositions(List<TileData> tiles, Sprite spriteSetting)
    {
        if(tiles.Count == 0) return new List<Vector2Int>();

        int numberOfPositions = GetSpriteSetting(spriteSetting);
        List<Vector2Int> positions = new List<Vector2Int>();

        for(int i = 0; i < numberOfPositions; i++)
        {
            positions.Add(tiles[random.Next(0, tiles.Count)].Position);
        }

        return positions;

    }
    private int GetSpriteSetting(Sprite spriteSetting)
    {
        if(spriteSetting == mapSprites.grassTile) return settings.numberOfStartPositions_Grass;
        else if(spriteSetting == mapSprites.stoneTile) return settings.numberOfStartPositions_Stone;
        else if(spriteSetting == mapSprites.sandTile) return settings.numberOfStartPositions_Sand;
        else if(spriteSetting == mapSprites.dirtTile) return settings.numberOfStartPositions_Dirt;
        else if(spriteSetting == mapSprites.waterTile) return settings.numberOfStartPositions_Water;
        else if(spriteSetting == mapSprites.oceanTile) return settings.numberOfStartPositions_Ocean;
        return 0;
    }
    private float GetProbabilityOfEnvironment(Sprite spriteSetting)
    {
        if(spriteSetting == mapSprites.grassTile) return settings.probabilityOfEnvironment_Grass;
        else if(spriteSetting == mapSprites.stoneTile) return settings.probabilityOfEnvironment_Stone;
        else if(spriteSetting == mapSprites.sandTile) return settings.probabilityOfEnvironment_Sand;
        else if(spriteSetting == mapSprites.dirtTile) return settings.probabilityOfEnvironment_Dirt;
        else if(spriteSetting == mapSprites.waterTile) return settings.probabilityOfEnvironment_Water;
        else if(spriteSetting == mapSprites.oceanTile) return settings.probabilityOfEnvironment_Ocean;
        return 0;
    }
    private void GenerateRandomWalkPath(List<TileData> tiles, Vector2Int startPosition, List<Sprite> allowedTiles, Sprite spriteSetting)
    {
        var directions = new List<Vector2Int> { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right};

        Vector2Int currentPosition = startPosition;

        for(int i = 0; i < settings.pathLength; i++)
        {
            Vector2Int direction = directions[random.Next(0, directions.Count)];

            currentPosition += direction;

            if(currentPosition.x < 0 || currentPosition.x >= settings.mapWidth || currentPosition.y < 0 || currentPosition.y >= settings.mapHeight)
            {
                break;
            }
            else
            {
                var tile = mapTiles[currentPosition];

                if(allowedTiles.Contains(tile.Sprite))
                {
                    if(random.Next(0, 100) < GetProbabilityOfEnvironment(spriteSetting) * 100)
                        SetEnviromentTile(tile);
                }
                else
                {
                    break;
                }
            }
        }
    }
    private void SetWhiteTile(TileData tile)
    {
        tile.SetTileSprite(mapSprites.whiteTile);
    }

    private void SetEnviromentTile(TileData tile)

    {
        List<Sprite> grassEnviromentTiles = new List<Sprite> { mapSprites.grassRockTile, mapSprites.grassBushTile, mapSprites.grassFlowerTile, mapSprites.grassSeedTile, mapSprites.stumpTile};
        List<Sprite> stoneEnviromentTiles = new List<Sprite> { mapSprites.stoneRockTile, mapSprites.stoneCopperTile, mapSprites.stoneCoalTile };

        if(tile.Sprite == mapSprites.grassTile)
        {
            Sprite randomSprite = grassEnviromentTiles[random.Next(0, grassEnviromentTiles.Count)];
            SetWhiteTile(tile);
            //tile.SetTileSprite(randomSprite);
        }
        else if(tile.Sprite == mapSprites.stoneTile)
        {
            Sprite randomSprite = stoneEnviromentTiles[random.Next(0, stoneEnviromentTiles.Count)];
            SetWhiteTile(tile);
            //tile.SetTileSprite(randomSprite);
        }
    }
    private float AddParabolicNoise(int x, int y)

    {
        int xOffset = x;
        int yOffset = y;

        if(x > settings.mapWidth / 2)
        {
            xOffset = settings.mapWidth - x;
        }
        if(y > settings.mapHeight / 2)
        {
            yOffset = settings.mapHeight - y;
        }

        int maxSize = settings.mapWidth / 2 + settings.mapHeight / 2;
        int currentSize = xOffset + yOffset;
        float t = (float)currentSize / (float)maxSize;

        float b = settings.oceanStrength; 
        float a = 1 - b;
        float value = a * Mathf.Pow(t, 2) + b * t - 1;

        return value;
    }

    public void GenerateMap(Dictionary<Vector2Int, TileData> mapTiles)
    {

        for (int x = 0; x < settings.mapWidth; x++)
        {
            for (int y = 0; y < settings.mapHeight; y++)
            {
                float amplitude = 1; 
                float frequency = 1; 
                float noiseHeight = 0; 
                float amplitudeSum = 0; 


                // Generowanie hałasu w wielu oktawach
                for (int i = 0; i < OCTAVES; i++)
                {
                    float sampleX = x * frequency / 50f;
                    float sampleY = y * frequency / 50f;
                    
                    float perlinValue = GeneratePerlinNoise(sampleX, sampleY);
                    noiseHeight += perlinValue * amplitude;
                    
                    amplitudeSum += amplitude;
                    amplitude *= PERSISTENCE;
                    frequency *= 2;

                }

                noiseHeight /= amplitudeSum;
                noiseMap[x, y] = noiseHeight;
                

                maxNoise = Math.Max(maxNoise, noiseHeight);
                minNoise = Math.Min(minNoise, noiseHeight);
            }
        }


        for (int x = 0; x < settings.mapWidth; x++)
        {
            for (int y = 0; y < settings.mapHeight; y++)
            {
                TileData tile = mapTiles[new Vector2Int(x, y)];
                if (tile.IsEmpty)
                {
                    float normalizedNoise = (noiseMap[x, y] - minNoise) / (maxNoise - minNoise);
                    float parabolicNoise = AddParabolicNoise(x, y);

                    normalizedNoise += parabolicNoise;
                    //SwitchGrayScale(normalizedNoise, tile);
                    SwitchTileSprite(normalizedNoise, tile);
                    tile.value = normalizedNoise;
                }
            }
        }
    }


    // Zmiana sprite'a płytki na podstawie wartości hałasu
    private void SwitchTileSprite(float noise, TileData tile) 
    {      
        if (noise > 0.8f)
            SetTile(tile, mapSprites.stoneTile); 
        else if (noise > 0.5f)
            SetTile(tile, mapSprites.grassTile); 
        else if (noise > 0.4f)
            SetTile(tile, mapSprites.dirtTile); 
        else if (noise > 0.2f)
            SetTile(tile, mapSprites.grassTile); 
        else if (noise > 0.1f)
            SetTile(tile, mapSprites.sandTile); 
        else
            SetTile(tile, mapSprites.oceanTile); 
    }

    private void SetTile(TileData tile, Sprite sprite)
    {
        if(sprite == mapSprites.stoneTile)
        {
            stoneTiles.Add(tile);
        }
        else if(sprite == mapSprites.grassTile)
        {
            grassTiles.Add(tile);
        }
        else if(sprite == mapSprites.dirtTile)
        {
            dirtTiles.Add(tile);
        }
        else if(sprite == mapSprites.sandTile)
        {
            sandTiles.Add(tile);
        }
        else if(sprite == mapSprites.oceanTile)
        {
            oceanTiles.Add(tile);
        }

        tile.SetTileSprite(sprite);
    }
    private void SwitchGrayScale(float noise, TileData tile)
    {
        if(noise< 0) noise = 0;
        tile.SetTileGrayScale(noise, mapSprites.whiteTile);
    }

    public void SetupPerlinNoiseToStonePath(List<TileData> stonePathTiles)
    {
        foreach(TileData tile in stonePathTiles)
        {
            for(int i = -1; i<=1; i++)
            {
                if(!mapTiles.ContainsKey(new Vector2Int(tile.Position.x + i, tile.Position.y))) continue;

                float normalizedNoise = (noiseMap[tile.Position.x + i, tile.Position.y] - minNoise) / (maxNoise - minNoise);

                SwitchTileSpriteStonePath(normalizedNoise, tile);
                tile.value = normalizedNoise;
                
            }
        } 
    }

    private void SwitchTileSpriteStonePath(float noise, TileData tile)
    {
        if(noise < 0.6f)
        {
            tile.SetTileSprite(mapSprites.stonePathTile);
        }
        else
        {
            tile.SetTileSprite(mapSprites.grassTile);
        }   
    }
}
