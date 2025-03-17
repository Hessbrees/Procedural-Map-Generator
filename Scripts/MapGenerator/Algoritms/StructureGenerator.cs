using UnityEngine;
using System.Collections.Generic;
using Random = System.Random;
using Unity.VisualScripting;
using NUnit.Framework;
using UnityEngine.AI;
using System.Linq;
using System;

public class StructureGenerator
{
    private MapGenerationSettings settings;
    private MapSprites mapSprites;
    private Random random;
    private List<TileData> doorTiles = new List<TileData>();
    private Dictionary<Vector2Int, TileData> mapTiles;
    private List<TileData> dirtTiles = new List<TileData>();
    private List<TileData> grassTiles = new List<TileData>();
    public List<TileData> stonePathTiles = new List<TileData>();
    string resultTerrain;
    public StructureGenerator(MapGenerationSettings settings, MapSprites mapSprites, int seed, Dictionary<Vector2Int, TileData> mapTiles, List<TileData> dirtTiles, List<TileData> grassTiles)
    {
        rules = new Dictionary<char, string>{{ 'F', settings.lSystemRules}};
        axiom = settings.lSystemAxiom;
        iterations = settings.lSystemIterations;
        this.mapTiles = mapTiles;
        this.dirtTiles = dirtTiles;
        this.grassTiles = grassTiles;
        this.settings = settings; 
        this.mapSprites = mapSprites;
        random = new Random(seed);    
        resultTerrain = LSystemGenerate();
    }


    public void GenerateStructures()
    {
        GenerateBSP(mapTiles, 0, 0, settings.mapWidth, settings.mapHeight,0);
        
        InterPolate(resultTerrain, new List<Sprite>{mapSprites.dirtTile}, mapSprites.pathTile, dirtTiles);
        InterPolate(resultTerrain, new List<Sprite>{mapSprites.grassTile}, mapSprites.treeDownTile, grassTiles);
    }
    Dictionary<char, string> rules;
    int iterations;
    string axiom;
    private string LSystemGenerate()
    {
        string current = axiom;
        for (int i = 0; i < iterations; i++)
        {
            string next = "";
            foreach (char c in current)
            {
                next += rules.ContainsKey(c) ? rules[c] : c.ToString();
            }
            current = next;
        }
        return current;
    }
    public class StackItem
    {
        public Vector2Int Position;
        public Vector2Int Direction;
    }
    private class InterpolationItem
    {
        public Vector2Int Position;
        public Vector2Int Direction;
        public bool IsEnding;
    }
    
    private List<InterpolationItem> InterPolate(string result,List<Sprite> allowedTiles, Sprite buildingTile,List<TileData> tiles, bool isEnding = false)
    {
        List<InterpolationItem> items = new List<InterpolationItem>();
        if(tiles.Count == 0) return items;

        int randomIndex = random.Next(0, tiles.Count);

        Vector2Int randomPosition = tiles[randomIndex].Position;
        Vector2Int direction = Vector2Int.right;
        Vector2Int currentPosition = randomPosition;
        items.Add(new InterpolationItem{Position = currentPosition, Direction = direction, IsEnding = false});
        Stack<StackItem> stack = new Stack<StackItem>();
        Vector2Int lastPosition = currentPosition;

        foreach(char c in result)
        {
            if(c == 'F')
            {
                if(!mapTiles.ContainsKey(currentPosition))
                {
                    if(isEnding)
                    {
                        currentPosition = WrapPosition(currentPosition);
                    }
                    else
                    {
                        currentPosition = tiles[random.Next(0, tiles.Count)].Position;
                    }
                }
                
                if(!isEnding)
                if(!allowedTiles.Contains(mapTiles[currentPosition].Sprite))
                {
                        currentPosition = tiles[random.Next(0, tiles.Count)].Position;   
                }

                if(mapTiles[currentPosition].IsStructure)
                {
                    currentPosition += direction;
                    continue;
                }

                if(buildingTile == mapSprites.treeDownTile)
                {
                    if(mapTiles[currentPosition].Sprite != mapSprites.treeUpTile)
                    if(mapTiles.ContainsKey(currentPosition+Vector2Int.up))
                    {
                        if(mapTiles[currentPosition+Vector2Int.up].Sprite != mapSprites.treeDownTile && !mapTiles[currentPosition+Vector2Int.up].IsStructure)
                        {
                            mapTiles[currentPosition+Vector2Int.up].SetTileSprite(mapSprites.treeUpTile);
                            mapTiles[currentPosition].SetTileSprite(buildingTile);

                            if(buildingTile == mapSprites.stonePathTile)
                            {
                                stonePathTiles.Add(mapTiles[currentPosition+Vector2Int.up]);
                            }
                        }
                    } 
                }

                else
                {
                    if(isEnding)
                    {
                        if(!mapTiles[currentPosition].IsWater)
                        {
                            mapTiles[currentPosition].SetTileSprite(buildingTile);  

                            if(buildingTile == mapSprites.stonePathTile)
                            {
                                stonePathTiles.Add(mapTiles[currentPosition]);
                            }

                        }
                    }
                    else
                    {
                        mapTiles[currentPosition].SetTileSprite(buildingTile);  

                        if(buildingTile == mapSprites.stonePathTile)
                        {
                            stonePathTiles.Add(mapTiles[currentPosition]);
                        }
                    }


                }

                lastPosition = currentPosition;
                currentPosition += direction;
            }
            else if(c == '+')
            {
                direction = new Vector2Int(direction.y, -direction.x);
            }
            else if(c == '-')
            {
                direction = new Vector2Int(-direction.y, direction.x);
            }
        }
        //items.Add(new InterpolationItem{Position = lastPosition, Direction = direction, IsEnding = true});
        return items;
    }
    private void GenerateBSP(Dictionary<Vector2Int, TileData> mapTiles, int x0, int y0, int xmax, int ymax,int iterations)
    {
        int padding = 4;
        int width = xmax-x0-2*padding;
        int height = ymax-y0-2*padding;

        x0 += padding;
        y0 += padding;
        xmax -= padding;
        ymax -= padding;

        if(iterations == settings.bspIterations || width < settings.townMaximumSize || height < settings.townMaximumSize)
        {
            GenerateCity(mapTiles, x0, y0, xmax, ymax);
            return;
        }

        bool splitHorizontally = random.Next(0, 2) == 0;

        int splitPosition = splitHorizontally ? (ymax - y0)/2 : (xmax - x0)/2;
        
        // Split the grid and recursively generate BSP for each half
        if (splitHorizontally)
        {
            GenerateBSP(mapTiles, x0, y0, xmax, ymax - splitPosition, iterations+1);
            GenerateBSP(mapTiles, x0, y0 + splitPosition, xmax, ymax, iterations+1);
        }
        else
        {
            GenerateBSP(mapTiles, x0, y0, xmax - splitPosition, ymax, iterations+1);
            GenerateBSP(mapTiles, x0 + splitPosition, y0, xmax, ymax, iterations+1);
        } 
    }
    private List<InterpolationItem> CreateCityPath(List<TileData> tiles)

    {
        int randomRules = random.Next(0, 2);

        if(randomRules == 0)
        {
            axiom = "F";
            iterations = 5;
            rules = new Dictionary<char, string>{{ 'F', "F+F-F"}};
        }
        else if(randomRules == 1)
        {
            axiom = "--F";
            iterations = 3;
            rules = new Dictionary<char, string>{{ 'F', "FF+FF-FF"}};
        }

        string result = LSystemGenerate();


        return InterPolate(result, 
        new List<Sprite>{
            mapSprites.grassTile, 
            mapSprites.dirtTile, 
            mapSprites.pathTile, 
            mapSprites.stoneTile,
            mapSprites.grassRockTile,
            mapSprites.grassBushTile,
            mapSprites.grassFlowerTile,
            mapSprites.grassSeedTile,
            mapSprites.stumpTile,
            mapSprites.stoneRockTile,
            mapSprites.stoneCopperTile,
            mapSprites.stoneCoalTile,
            mapSprites.sandTile
            }, 
            mapSprites.stonePathTile,
            tiles,
            true);
    }

    private List<TileData> GetMiddleTiles(Dictionary<Vector2Int, TileData> mapTiles, int x0, int y0, int xmax, int ymax)
    {
        List<TileData> tiles = new List<TileData>();
        if(mapTiles.ContainsKey(new Vector2Int((x0+xmax)/2, (y0+ymax)/2)))
        {
            tiles.Add(mapTiles[new Vector2Int((x0+xmax)/2, (y0+ymax)/2)]);
        }
        return tiles;
    }
    private void GenerateCity(Dictionary<Vector2Int, TileData> mapTiles, int x0, int y0, int xmax, int ymax)
    {
        List<TileData> tiles = GetMiddleTiles(mapTiles, x0, y0, xmax, ymax);
        List<InterpolationItem> items = CreateCityPath(tiles);


        foreach(InterpolationItem item in items)
        {
            Structure structure = new Structure{
                Width = 10,
                Height = 10,
                Position = new Vector2Int(item.Position.x, item.Position.y)
            };

            Structure newStructure = PlaceStructure(mapTiles, structure);
            if(newStructure == null) continue;
            CreateTownBounds(newStructure);
        }

    }
    public class StructureBounds
    {
        public int Width;
        public int Height;
        public Vector2Int Position;
    }
    private void CreateTownBounds(Structure structure)
    {
        int randomBoundsWidth = random.Next(settings.townMinimumSize, settings.townMaximumSize);
        int randomBoundsHeight = random.Next(settings.townMinimumSize, settings.townMaximumSize);

        int townOffsetX = (randomBoundsWidth-1-structure.Width)/2;
        int townOffsetY = (randomBoundsHeight-1-structure.Height)/2;

        StructureBounds townBounds = new StructureBounds{
            Width = randomBoundsWidth, Height = randomBoundsHeight, 
            Position = new Vector2Int(structure.Position.x - townOffsetX, structure.Position.y - townOffsetY)};

        //Create a bounding box around the structure for new structures

        SetDownStructuresInTownBounds(townBounds, structure);
        SetMiddleStructuresInTownBounds(townBounds, structure);
        SetUpStructuresInTownBounds(townBounds, structure);
    }
    private void SetMiddleStructuresInTownBounds(StructureBounds townBounds, Structure structure)
    {
        bool isLeftStructure = random.Next(0, 2) == 0;
        bool isRightStructure = random.Next(0, 2) == 0;

        if(!isLeftStructure && !isRightStructure) return;

        int padding = 2;
        int minimumSize = 4;
        int x0 = townBounds.Position.x+padding;
        int y0 = structure.Position.y+padding;

        int width = Math.Abs(townBounds.Position.x - structure.Position.x)-1 -2*padding;
        int height = structure.Height-minimumSize;
        int xmax = x0+width-minimumSize-padding;
        int ymax = y0+height-minimumSize-padding;
        if(xmax<x0) return;
        int randomPositionX = random.Next(x0, xmax);
        if(ymax<y0) return;
        int randomPositionY = random.Next(y0, ymax);

        if(isRightStructure)
        {
            Structure newStructure = new Structure
            {
                Width = width,
                Height = height,
                Position = new Vector2Int(randomPositionX, randomPositionY),
                IsDoorTowardsDirection = false
            };

            PlaceStructure(mapTiles, newStructure);
        }

        if(isLeftStructure)
        {
            x0 = structure.Position.x+structure.Width+padding;
            xmax = x0+width- minimumSize;
            randomPositionX = random.Next(x0, xmax);

            Structure newStructure = new Structure
            {
                Width = width,
                Height = height,
                Position = new Vector2Int(randomPositionX, randomPositionY),
                IsDoorTowardsDirection = false
            };
            PlaceStructure(mapTiles, newStructure);
        }

    }

    private void SetUpStructuresInTownBounds(StructureBounds townBounds, Structure structure)
    {
        int cutWidth = townBounds.Width/3;
        int padding = 2;

        int minimumSize = 4;
        int x0 = townBounds.Position.x+padding;
        int y0 = structure.Position.y+structure.Height+padding;

        int width = Math.Abs(townBounds.Position.x - structure.Position.x)-1 -2*padding;
        int height = Math.Abs(townBounds.Position.y - structure.Position.y)-1 -2*padding;
        int xmax = x0+townBounds.Width- padding - minimumSize;
        int ymax = y0+height-padding;
        int randomPositionY = random.Next(y0, ymax);
        int randomPositionX = random.Next(x0, xmax);
        width = minimumSize + xmax - randomPositionX;
        
        int amountOfStructures = random.Next(0, 3);

        if(cutWidth <16)
        {
            amountOfStructures = random.Next(1, 2);
        }


        if(amountOfStructures == 0) return;
        else if(amountOfStructures == 1)
        {
            Structure newStructure = new Structure
            {
                Width = width,
                Height = height,
                Position = new Vector2Int(randomPositionX, randomPositionY),
                IsDoorTowardsDirection = false
            };
            PlaceStructure(mapTiles, newStructure);
        }
        else if(amountOfStructures == 2)
        {
            int minimumWidth = minimumSize + 2*padding;
            int x2 = random.Next(x0 + minimumWidth, xmax - minimumWidth);

            int randomPositionX2 = random.Next(x2, xmax);
            width = minimumSize + xmax - randomPositionX2 - padding;
            randomPositionY = random.Next(y0, ymax);

            Structure newStructure = new Structure
            {
                Width = width,
                Height = height,
                Position = new Vector2Int(randomPositionX2, randomPositionY),
                IsDoorTowardsDirection = false
            };

            randomPositionX2 = random.Next(x0, x2);
            width = minimumSize + x2 - randomPositionX2 - padding;
            randomPositionY = random.Next(y0, ymax);

            Structure newStructure2 = new Structure
            {
                Width = width,
                Height = height,
                Position = new Vector2Int(randomPositionX2, randomPositionY),
                IsDoorTowardsDirection = false
            };

            PlaceStructure(mapTiles, newStructure);
            PlaceStructure(mapTiles, newStructure2);
        }      
        else if(amountOfStructures == 3)
        {
            int minimumWidth = minimumSize + 2*padding;
            int x2 = random.Next(x0 + cutWidth, xmax - cutWidth);
            int x3 = random.Next(x2, xmax -minimumSize);

            randomPositionX = random.Next(x0, x2);
            width = minimumSize + x0 - randomPositionX;
            randomPositionY = random.Next(y0, ymax);


            Structure newStructure = new Structure
            {
                Width = width,
                Height = height,
                Position = new Vector2Int(randomPositionX, randomPositionY),
                IsDoorTowardsDirection = false
            };

            int randomPositionX2 = random.Next(x2, x3);
            width = minimumSize + x2 - randomPositionX2;
            randomPositionY = random.Next(y0, ymax);


            Structure newStructure2 = new Structure
            {
                Width = width,
                Height = height,
                Position = new Vector2Int(randomPositionX2, randomPositionY),
                IsDoorTowardsDirection = false
            };

            int randomPositionX3 = random.Next(x3, xmax);
            width = minimumSize + x3 - randomPositionX3;
            randomPositionY = random.Next(y0, ymax);

            Structure newStructure3 = new Structure
            {
                Width = width,
                Height = height,
                Position = new Vector2Int(randomPositionX3, randomPositionY),
                IsDoorTowardsDirection = false
            };
            PlaceStructure(mapTiles, newStructure);
            PlaceStructure(mapTiles, newStructure2);
            PlaceStructure(mapTiles, newStructure3);
        }      
    }
    private void SetDownStructuresInTownBounds(StructureBounds townBounds, Structure structure)
    {
        int padding = 2;

        int minimumSize = 4;
        int x0 = townBounds.Position.x+padding;
        int y0 = townBounds.Position.y+padding;

        int width = Math.Abs(townBounds.Position.x - structure.Position.x)-1 -2*padding;
        int height = Math.Abs(townBounds.Position.y - structure.Position.y)-1 -2*padding;
        int xmax = x0+townBounds.Width- padding - minimumSize;
        int ymax = y0+height-minimumSize;

        if(ymax<y0) return;
        int randomPositionY = random.Next(y0, ymax);
        if(xmax<x0) return;
        int randomPositionX = random.Next(x0, xmax);
        width = minimumSize + xmax - randomPositionX;
        

        int amountOfStructures = random.Next(1, 2);

        if(amountOfStructures == 0) return;
        else if(amountOfStructures == 1)
        {
            Structure newStructure = new Structure
            {
                Width = width,
                Height = height,
                Position = new Vector2Int(randomPositionX, randomPositionY),
                IsDoorTowardsDirection = false
            };
            PlaceStructure(mapTiles, newStructure);
        }
        else if(amountOfStructures == 2)
        {
            int minimumWidth = minimumSize + 2*padding;
            int x2 = random.Next(x0 + minimumWidth, xmax - minimumWidth);

            int randomPositionX2 = random.Next(x2, xmax);
            width = minimumSize + xmax - randomPositionX2 - padding;
            randomPositionY = random.Next(y0, ymax);

            Structure newStructure = new Structure
            {
                Width = width,
                Height = height,
                Position = new Vector2Int(randomPositionX2, randomPositionY),
                IsDoorTowardsDirection = false
            };

            randomPositionX2 = random.Next(x0, x2);
            width = minimumSize + x2 - randomPositionX2 - padding;
            randomPositionY = random.Next(y0, ymax);

            Structure newStructure2 = new Structure
            {
                Width = width,
                Height = height,
                Position = new Vector2Int(randomPositionX2, randomPositionY),
                IsDoorTowardsDirection = false
            };

            PlaceStructure(mapTiles, newStructure);
            PlaceStructure(mapTiles, newStructure2);
        }      


    }
    private Structure PlaceStructure(Dictionary<Vector2Int, TileData> mapTiles, Structure structure)
    {
        if(structure.Width < 4) return null;
        if(structure.Height < 4) return null;

        int structureWidth = random.Next(4, structure.Width);
        int structureHeight = random.Next(4, structure.Height);

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
            if(mapTiles.ContainsKey(new Vector2Int(structure.Position.x + i, structure.Position.y +randomDoorPositionY-1)))
                boundingBoxTiles.Add(mapTiles[new Vector2Int(structure.Position.x + i, structure.Position.y +randomDoorPositionY-1)]);



            if(mapTiles.ContainsKey(new Vector2Int(structure.Position.x + i, structure.Position.y + structureHeight+randomDoorPositionY)))
                boundingBoxTiles.Add(mapTiles[new Vector2Int(structure.Position.x + i, structure.Position.y + structureHeight+randomDoorPositionY)]);

        }

        for(int j = -1+randomDoorPositionY; j < structureHeight+randomDoorPositionY; j++)
        {
            if(mapTiles.ContainsKey(new Vector2Int(structure.Position.x +randomDoorPositionX-1, structure.Position.y + j+randomDoorPositionY)))
                boundingBoxTiles.Add(mapTiles[new Vector2Int(structure.Position.x +randomDoorPositionX-1, structure.Position.y + j+randomDoorPositionY)]);



            if(mapTiles.ContainsKey(new Vector2Int(structure.Position.x+structureWidth +randomDoorPositionX, structure.Position.y+ j+randomDoorPositionY)))
                boundingBoxTiles.Add(mapTiles[new Vector2Int(structure.Position.x + structureWidth + randomDoorPositionX, structure.Position.y + j+randomDoorPositionY)]);

        }

        // Place the structure
        for (int i = randomDoorPositionX; i < structureWidth + randomDoorPositionX; i++)
        {
            for (int j = randomDoorPositionY; j < structureHeight + randomDoorPositionY; j++)
            {
                Vector2Int position = new Vector2Int(structure.Position.x + i, structure.Position.y + j);


                if (!mapTiles.ContainsKey(position)) continue;

                if(mapTiles[position].IsWater|| mapTiles[position].IsStructure && !mapTiles[position].IsStonePath) return null;
                
                TileData tile = mapTiles[position];
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
                    horizontalWallTiles[i].SetTileSprite(mapSprites.floorTile);
                }

                else
                {
                    horizontalWallTiles[i].SetTileSprite(mapSprites.wallTile);
                }
        }

        for(int i = 0; i < verticalWallTiles.Count; i++)
        {
            if(verticalWallTiles.Count != 0)
                if(verticalWallTiles[i].IsStructure && !verticalWallTiles[i].IsStonePath)
                {
                    verticalWallTiles[i].SetTileSprite(mapSprites.floorTile);
                }
                else
                {
                    verticalWallTiles[i].SetTileSprite(mapSprites.wallTile);
                }
        }

        for(int i = 0; i < cornerTiles.Count; i++)
        {
            if(cornerTiles.Count != 0)
                if(cornerTiles[i].IsStructure && !cornerTiles[i].IsStonePath)
                {
                    cornerTiles[i].SetTileSprite(mapSprites.floorTile);
                }
                else
                {
                    cornerTiles[i].SetTileSprite(mapSprites.wallTile);
                }
        }

        foreach(TileData tile in structureTiles)
        {
            tile.SetTileSprite(mapSprites.floorTile);
        }

        int numberOfColumns = 0;
        if(structureTiles.Count > 4) numberOfColumns = 1;
        else if(structureTiles.Count > 10) numberOfColumns = 2;
        else if(structureTiles.Count > 20) numberOfColumns = 3;

        for(int i = 0; i < numberOfColumns; i++)
        {
            int randomIndex = random.Next(0, structureTiles.Count);
            if(structureTiles.Count != 0)
                structureTiles[randomIndex].SetTileSprite(mapSprites.columnTile);
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
            Position = new Vector2Int(structure.Position.x + randomDoorPositionX, structure.Position.y + randomDoorPositionY)};
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
    private void SetPathTiles(List<TileData> boundingBoxTiles)
    {
        foreach(TileData tile in boundingBoxTiles)
        {
            if(tile.IsWater) continue;

            if(tile.IsStructure) continue;

            tile.SetTileSprite(mapSprites.stonePathTile);
        }
    }
    private void SetDoorTiles(Vector2Int doorPosition, bool isHorizontalDoor)
    {
        if(mapTiles.ContainsKey(doorPosition))
        {
            if(mapTiles[doorPosition].IsWater) return;


        if(isHorizontalDoor)
        {
            mapTiles[doorPosition].SetTileSprite(mapSprites.horizontalDoorTile);
        }
        else
        {
            mapTiles[doorPosition].SetTileSprite(mapSprites.verticalDoorTile);
        }
        }
    }
    public class BoundingBoxWalls
    {
        public List<Vector2Int> Left;
        public List<Vector2Int> Right;
        public List<Vector2Int> Up;
        public List<Vector2Int> Down;
    }

    private Vector2Int WrapPosition(Vector2Int currentPosition)
    {
        int x = currentPosition.x;
        int y = currentPosition.y;

        if (x <= 0 && y <= 0)
        {
            return new Vector2Int(settings.mapWidth - 1, settings.mapHeight - 1);
        }
        else if (x <= 0 && y >= settings.mapHeight - 1)
        {
            return new Vector2Int(settings.mapWidth - 1, 0);
        }
        else if (x >= settings.mapWidth - 1 && y <= 0)
        {
            return new Vector2Int(0, settings.mapHeight - 1);
        }
        else if (x >= settings.mapWidth - 1 && y >= settings.mapHeight - 1)
        {
            return new Vector2Int(0, 0);
        }
        else if (x <= 0)
        {
            return new Vector2Int(settings.mapWidth - 1, y);
        }
        else if (x >= settings.mapWidth - 1)
        {
            return new Vector2Int(0, y);
        }
        else if (y <= 0)
        {
            return new Vector2Int(x, settings.mapHeight - 1);
        }
        else if (y >= settings.mapHeight - 1)
        {
            return new Vector2Int(x, 0);
        }

        return currentPosition;
    }
} 