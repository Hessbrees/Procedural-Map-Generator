using UnityEngine;

public class TileData : MonoBehaviour
{
    public Vector2Int Position {get; private set;}
    public bool IsEmpty {get; private set;} = true;
    public bool IsWater {get; private set;}
    public bool IsStructure {get; private set;}
    public bool IsStonePath{get; private set;}
    public float value;
    public Sprite Sprite {get; private set;}
    public char LSystemChar;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private MapSprites mapSprites;
    public void SetTileSprite(Sprite sprite)
    {
        spriteRenderer.sprite = sprite;
        spriteRenderer.color = new Color(1, 1, 1, 1);
        IsEmpty = false;
        if(sprite == mapSprites.waterTile || sprite == mapSprites.oceanTile)
            IsWater = true;
        if(sprite == mapSprites.wallTile || 
        sprite == mapSprites.verticalDoorTile || 
        sprite == mapSprites.horizontalDoorTile || 
        sprite == mapSprites.columnTile || 
        sprite == mapSprites.floorTile ||
        sprite == mapSprites.verticalDoorTile ||
        sprite == mapSprites.horizontalDoorTile ||
        sprite == mapSprites.stonePathTile ||
        sprite == mapSprites.stoneWallTile)
            IsStructure = true;
        if(sprite == mapSprites.stonePathTile)

            IsStonePath = true;

        Sprite = sprite;
    }
    public void SetTileGrayScale(float value, Sprite sprite)
    {
        spriteRenderer.sprite = sprite;
        spriteRenderer.color = new Color(value, value, value, 1);
        IsEmpty = false;
        Sprite = sprite;
    }

    public void SetTilePosition(int x, int y)
    {
        transform.position = new Vector3(x * MapGeneratorConsts.TILE_SIZE, y * MapGeneratorConsts.TILE_SIZE, 0);
        Position = new Vector2Int(x, y);
    }
} 