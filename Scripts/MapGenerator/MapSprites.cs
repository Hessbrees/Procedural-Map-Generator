using UnityEngine;

[CreateAssetMenu(fileName = "MapSprites", menuName = "Map/Map Sprites")]
public class MapSprites : ScriptableObject
{
    [Header("Water Tiles")]
    public Sprite waterTile;
    public Sprite oceanTile;

    [Header("Ground Tiles")]
    public Sprite grassTile;
    public Sprite sandTile;
    public Sprite stoneTile;
    public Sprite dirtTile;
    
    [Header("White Tiles")]
    public Sprite whiteTile;
    [Header("Dungeon Tiles")]
    public Sprite dungeonWallTile;
    public Sprite dungeonFloorTile;
    public Sprite dungeonVerticalDoorTile;
    public Sprite dungeonHorizontalDoorTile;
    public Sprite dungeonColumnTile;
    
    [Header("Structure Tiles")]
    public Sprite wallTile;      
    public Sprite floorTile;     
    public Sprite verticalDoorTile;      
    public Sprite horizontalDoorTile;
    public Sprite columnTile;
    public Sprite stonePathTile;
    public Sprite ladderTile;

    public Sprite stoneWallTile;

    [Header("Other")]
    public Sprite pathTile;

    [Header("Environment")]
    public Sprite stumpTile;
    public Sprite treeDownTile;
    public Sprite treeUpTile;
    public Sprite grassSeedTile;
    public Sprite grassBushTile;
    public Sprite grassFlowerTile;
    public Sprite grassRockTile;
    public Sprite stoneRockTile;
    public Sprite stoneCopperTile;
    public Sprite stoneCoalTile;

} 