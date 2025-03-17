using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;
using UnityEngine.UIElements;
using Random = System.Random;

public class DungeonGenerator
{
    private MapGenerationSettings settings;
    private MapSprites mapSprites;
    private Random random;
    private Dictionary<Vector2Int, TileData> dungeonTiles;
    private List<Structure> structures = new List<Structure>();
    private MapGenerator mapGenerator;
    int amountOfStructures = 0;
    public DungeonGenerator(MapGenerationSettings settings, MapSprites mapSprites, int seed, Dictionary<Vector2Int, TileData> dungeonTiles, MapGenerator mapGenerator)
    {
        this.settings = settings;
        this.mapSprites = mapSprites;
        this.random = new Random(seed);
        this.dungeonTiles = dungeonTiles;
        this.mapGenerator = mapGenerator;
    }

    public void GenerateDungeon(int x0, int y0, int xmax, int ymax)
    {
        amountOfStructures = 0;
        GenerateBSP(x0, y0, xmax, ymax, 0);

        GeneratePathBetweenConnectionPoints();
    }

    private void GeneratePathBetweenConnectionPoints()
    {
        foreach(Structure structure in structures)
        {
            List<Structure> nearestStructures = structures.OrderBy(s => Vector2Int.Distance(structure.Position, s.Position)).ToList();
            nearestStructures.Remove(structure);
            
            if(nearestStructures.Count == 0) continue;
            Structure nearestStructure = nearestStructures[random.Next(0, nearestStructures.Count)];

            structure.IsConnected = true;

            Vector2Int currentPosition = structure.DoorPosition;

            Vector2Int targetPosition = nearestStructure.DoorPosition;

            int maxIterations = 10000;
            int iterations = 0;

            while(currentPosition != targetPosition && iterations < maxIterations)
            {
                var tile = mapGenerator.GenerateDungeonTile(currentPosition.x, currentPosition.y); 
                
                if(!dungeonTiles.ContainsKey(tile.Position))
                {
                    dungeonTiles[tile.Position] = tile;
                    tile.SetTileSprite(mapSprites.stonePathTile);
                }

                bool isXIncrement = random.Next(0, 2) == 0;
                
                if(isXIncrement)
                {
                    if(currentPosition.x == targetPosition.x) isXIncrement = false;
                }
                else
                {
                    if(currentPosition.y == targetPosition.y) isXIncrement = true;
                }

                if(isXIncrement && currentPosition.x != targetPosition.x)
                {
                    if(currentPosition.x < targetPosition.x) currentPosition.x++;
                    else if(currentPosition.x > targetPosition.x) currentPosition.x--;
                }
                else if(!isXIncrement && currentPosition.y != targetPosition.y)
                {
                    if(currentPosition.y < targetPosition.y) currentPosition.y++;
                    else if(currentPosition.y > targetPosition.y) currentPosition.y--;
                }

                iterations++;
                
            }
           
        }

    }
    private void GenerateBSP(int x0, int y0, int xmax, int ymax, int iteration)
    {
        if(amountOfStructures == settings.dungeonMaxRooms) return;

        int padding = 0;
        int width = xmax-x0-2*padding;
        int height = ymax-y0-2*padding;

        x0 += padding;
        y0 += padding;
        xmax -= padding;
        ymax -= padding;

        bool splitHorizontally = random.Next(0, 2) == 0;

        int splitSizeX = (xmax - x0)/2;
        int splitSizeY = (ymax - y0)/2;

        int splitPosition = splitHorizontally ? splitSizeX : splitSizeY;

        if(iteration > 30) return;

        if(iteration == 5)
        if(amountOfStructures <= settings.dungeonMaxRooms)
        {
            int randomStructurePosX = x0 + 1;
            int randomStructurePosY = y0 + 1;

            Structure structure = new Structure{
                Width = width,
                Height = height,
                Position = new Vector2Int(randomStructurePosX, randomStructurePosY),
                Direction = Vector2Int.up,
                IsDoorOnPath = false,
                IsDoorTowardsDirection = false,
                BSPLeftCorner = new Vector2Int(x0-padding, y0-padding),
                BSPRightCorner = new Vector2Int(xmax+padding, ymax+padding)
            };

            var newStructure = PlaceStructure(structure);

            if(newStructure != null)
            {
                amountOfStructures++;
                structures.Add(newStructure);
            }
            return;
        }    

        // Split the grid and recursively generate BSP for each half
        if (splitHorizontally)
        {
            if(ymax - splitPosition < y0) return;
            GenerateBSP(x0, y0, xmax, ymax - splitPosition, iteration+1);
            GenerateBSP(x0, ymax - splitPosition, xmax, ymax, iteration+1);
        }
        else
        {
            if(xmax - splitPosition < x0) return;
            GenerateBSP(x0, y0, xmax - splitPosition, ymax, iteration+1);
            GenerateBSP(xmax - splitPosition, y0, xmax, ymax, iteration+1);
        } 
    }

    private Structure PlaceStructure(Structure structure)
    {
        if(structure.Width < MapGeneratorConsts.STRUCTURE_MINIMUM_SIZE) return null;
        if(structure.Height < MapGeneratorConsts.STRUCTURE_MINIMUM_SIZE) return null;

        int structureWidth = random.Next(MapGeneratorConsts.STRUCTURE_MINIMUM_SIZE, structure.Width >= MapGeneratorConsts.STRUCTURE_MAXIMUM_SIZE ? MapGeneratorConsts.STRUCTURE_MAXIMUM_SIZE : structure.Width);
        int structureHeight = random.Next(MapGeneratorConsts.STRUCTURE_MINIMUM_SIZE, structure.Height >= MapGeneratorConsts.STRUCTURE_MAXIMUM_SIZE ? MapGeneratorConsts.STRUCTURE_MAXIMUM_SIZE : structure.Height);

        List<TileData> structureTiles = new List<TileData>();
        List<TileData> horizontalWallTiles = new List<TileData>();
        List<TileData> verticalWallTiles = new List<TileData>();
        List<TileData> cornerTiles = new List<TileData>();
        List<TileData> boundingBoxTiles = new List<TileData>();

        int randomDoorPositionX = 0;
        int randomDoorPositionY = 0;
        bool isHorizontalDoor = false;

        if(structure.IsDoorOnPath)
        {
            DetermineDoorPosition(structure.Direction, structureWidth, structureHeight, structure.IsDoorTowardsDirection, out randomDoorPositionX, out randomDoorPositionY, out isHorizontalDoor);
        }

        for(int i = randomDoorPositionX-1; i < structureWidth+randomDoorPositionX+1; i++)
        {
            if(dungeonTiles.ContainsKey(new Vector2Int(structure.Position.x + i, structure.Position.y +randomDoorPositionY-1)))
                boundingBoxTiles.Add(dungeonTiles[new Vector2Int(structure.Position.x + i, structure.Position.y +randomDoorPositionY-1)]);
            else
                boundingBoxTiles.Add(mapGenerator.GenerateDungeonTile(structure.Position.x + i, structure.Position.y +randomDoorPositionY-1));

            if(dungeonTiles.ContainsKey(new Vector2Int(structure.Position.x + i, structure.Position.y + structureHeight+randomDoorPositionY)))
                boundingBoxTiles.Add(dungeonTiles[new Vector2Int(structure.Position.x + i, structure.Position.y + structureHeight+randomDoorPositionY)]);
            else
                boundingBoxTiles.Add(mapGenerator.GenerateDungeonTile(structure.Position.x + i, structure.Position.y + structureHeight+randomDoorPositionY));

        }

        for(int j = -1+randomDoorPositionY; j < structureHeight+randomDoorPositionY; j++)
        {
            if(dungeonTiles.ContainsKey(new Vector2Int(structure.Position.x +randomDoorPositionX-1, structure.Position.y + j+randomDoorPositionY)))
                boundingBoxTiles.Add(dungeonTiles[new Vector2Int(structure.Position.x +randomDoorPositionX-1, structure.Position.y + j+randomDoorPositionY)]);
            else
                boundingBoxTiles.Add(mapGenerator.GenerateDungeonTile(structure.Position.x +randomDoorPositionX-1, structure.Position.y + j+randomDoorPositionY));

            if(dungeonTiles.ContainsKey(new Vector2Int(structure.Position.x+structureWidth +randomDoorPositionX, structure.Position.y+ j+randomDoorPositionY)))
                boundingBoxTiles.Add(dungeonTiles[new Vector2Int(structure.Position.x + structureWidth + randomDoorPositionX, structure.Position.y + j+randomDoorPositionY)]);
            else
                boundingBoxTiles.Add(mapGenerator.GenerateDungeonTile(structure.Position.x +structureWidth +randomDoorPositionX, structure.Position.y+ j+randomDoorPositionY));

        }

        // Place the structure
        for (int i = randomDoorPositionX; i < structureWidth + randomDoorPositionX; i++)
        {
            for (int j = randomDoorPositionY; j < structureHeight + randomDoorPositionY; j++)
            {
                Vector2Int position = new Vector2Int(structure.Position.x + i, structure.Position.y + j);
                
                TileData tile = mapGenerator.GenerateDungeonTile(position.x, position.y);
                dungeonTiles[tile.Position] = tile;
                
                bool isHorizontalWall = i == randomDoorPositionX || i == structureWidth + randomDoorPositionX - 1;
                bool isVerticalWall = j == randomDoorPositionY || j == structureHeight + randomDoorPositionY - 1;
                bool isCorner = (i == randomDoorPositionX && j == randomDoorPositionY) || 
                (i == randomDoorPositionX && j == structureHeight + randomDoorPositionY - 1) || 
                (i == structureWidth + randomDoorPositionX - 1 && j == randomDoorPositionY) || 
                (i == structureWidth + randomDoorPositionX - 1 && j == structureHeight + randomDoorPositionY - 1);

                if(tile.IsWater) continue;

                if(isCorner) cornerTiles.Add(tile);
                else if(isHorizontalWall) horizontalWallTiles.Add(tile);
                else if(isVerticalWall) verticalWallTiles.Add(tile);
                else structureTiles.Add(tile);
            }
        }

        for(int i = 0; i < horizontalWallTiles.Count; i++)
        {
            if(horizontalWallTiles.Count != 0)
                if(horizontalWallTiles[i].IsStructure && !horizontalWallTiles[i].IsStonePath)
                {
                    horizontalWallTiles[i].SetTileSprite(mapSprites.dungeonFloorTile);
                    dungeonTiles[horizontalWallTiles[i].Position] = horizontalWallTiles[i];
                }

                else
                {
                    horizontalWallTiles[i].SetTileSprite(mapSprites.dungeonWallTile);
                    dungeonTiles[horizontalWallTiles[i].Position] = horizontalWallTiles[i];
                }
        }

        for(int i = 0; i < verticalWallTiles.Count; i++)
        {
            if(verticalWallTiles.Count != 0)
                if(verticalWallTiles[i].IsStructure && !verticalWallTiles[i].IsStonePath)
                {
                    verticalWallTiles[i].SetTileSprite(mapSprites.dungeonFloorTile);
                    dungeonTiles[verticalWallTiles[i].Position] = verticalWallTiles[i];
                }
                else
                {
                    verticalWallTiles[i].SetTileSprite(mapSprites.dungeonWallTile);
                    dungeonTiles[verticalWallTiles[i].Position] = verticalWallTiles[i];
                }
        }

        for(int i = 0; i < cornerTiles.Count; i++)
        {
            if(cornerTiles.Count != 0)
                if(cornerTiles[i].IsStructure && !cornerTiles[i].IsStonePath)
                {
                    cornerTiles[i].SetTileSprite(mapSprites.dungeonFloorTile);
                }
                else
                {
                    cornerTiles[i].SetTileSprite(mapSprites.dungeonWallTile);
                }
        }

        foreach(TileData tile in structureTiles)
        {
            dungeonTiles[tile.Position] = tile;
            tile.SetTileSprite(mapSprites.dungeonFloorTile);
        }

        int numberOfColumns = 0;
        if(structureTiles.Count > 4) numberOfColumns = 1;
        else if(structureTiles.Count > 10) numberOfColumns = 2;
        else if(structureTiles.Count > 20) numberOfColumns = 3;

        for(int i = 0; i < numberOfColumns; i++)
        {
            int randomIndex = random.Next(0, structureTiles.Count);
            if(structureTiles.Count != 0)
            {
                structureTiles[randomIndex].SetTileSprite(mapSprites.dungeonColumnTile);
                dungeonTiles[structureTiles[randomIndex].Position] = structureTiles[randomIndex];
            }
        }

        Vector2Int doorPosition = new Vector2Int(structure.Position.x - randomDoorPositionX, structure.Position.y - randomDoorPositionY);
        
        if(structure.IsDoorOnPath)
        {
            SetDoorTiles(doorPosition, isHorizontalDoor);
        }
        else
        {
            if(isHorizontalDoor)
            {
                if(horizontalWallTiles.Count != 0)
                doorPosition = horizontalWallTiles[random.Next(0, horizontalWallTiles.Count)].Position;
            }
            else
            {
                if(verticalWallTiles.Count != 0)
                doorPosition = verticalWallTiles[random.Next(0, verticalWallTiles.Count)].Position;
            }

            SetDoorTiles(doorPosition, isHorizontalDoor);
        }

        SetPathTiles(boundingBoxTiles);

        return new Structure{
            Width = structureWidth, 
            Height = structureHeight, 
            Position = new Vector2Int(structure.Position.x + randomDoorPositionX, structure.Position.y + randomDoorPositionY),
            DoorPosition = doorPosition,
            IsHorizontalDoor = isHorizontalDoor,
            BSPLeftCorner = structure.BSPLeftCorner,
            BSPRightCorner = structure.BSPRightCorner
        };
    }
    private void SetDoorTiles(Vector2Int doorPosition, bool isHorizontalDoor)
    {
        TileData tile = mapGenerator.GenerateDungeonTile(doorPosition.x, doorPosition.y);
        dungeonTiles[doorPosition] = tile;

        if(isHorizontalDoor)
        {
            tile.SetTileSprite(mapSprites.dungeonHorizontalDoorTile);
        }
        else
        {
            tile.SetTileSprite(mapSprites.dungeonVerticalDoorTile);
        }        
    }
    private void SetPathTiles(List<TileData> boundingBoxTiles)
    {
        foreach(TileData tile in boundingBoxTiles)
        {
            if(tile.IsWater) continue;

            if(tile.IsStructure) continue;

            tile.SetTileSprite(mapSprites.stonePathTile);
        }
    }
    private void DetermineDoorPosition(Vector2Int direction, int structureWidth, int structureHeight, bool isEnding, out int randomDoorPositionX, out int randomDoorPositionY, out bool isHorizontalDoor)
    {
        randomDoorPositionX = 0;
        randomDoorPositionY = 0;
        isHorizontalDoor = false;

        if(!isEnding)
        {
            if(direction == Vector2Int.up)
            {
                randomDoorPositionY = -random.Next(1, 3);
                isHorizontalDoor = true;
            }
            else if(direction == Vector2Int.down)
            {
                randomDoorPositionX = -structureWidth + 1;
                randomDoorPositionY = -random.Next(1, 3);
                isHorizontalDoor = true;
            }
            else if(direction == Vector2Int.right)
            {
                randomDoorPositionX = -random.Next(1, 3);
                isHorizontalDoor = false;
            }
            else if(direction == Vector2Int.left)
            {
                randomDoorPositionY = -structureHeight + 1;
                randomDoorPositionX = -random.Next(1, 3);
                isHorizontalDoor = false;
            }
        }
        else
        {

            if(direction == Vector2Int.down)
            {
                randomDoorPositionY = -random.Next(1, 3);
                isHorizontalDoor = true;
            }
            else if(direction == Vector2Int.up)
            {
                randomDoorPositionX = -structureWidth + 1;
                randomDoorPositionY = -random.Next(1, 3);
                isHorizontalDoor = true;
            }
            else if(direction == Vector2Int.left)
            {
                randomDoorPositionX = -random.Next(1, 3);
                isHorizontalDoor = false;
            }
            else if(direction == Vector2Int.right)
            {
                randomDoorPositionY = -structureHeight + 1;
                randomDoorPositionX = -random.Next(1, 3);
                isHorizontalDoor = false;
            }
        }
    }
}
