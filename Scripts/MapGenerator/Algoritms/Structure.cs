using UnityEngine;

public class Structure
{
    public int Width;

    public int Height;
    public Vector2Int Position;
    public Vector2Int Direction;

    public Vector2Int DoorPosition;
    public bool IsDoorTowardsDirection;
    public bool IsDoorOnPath;

    public Vector2Int BSPLeftCorner;
    public Vector2Int BSPRightCorner;
    public Vector2Int BSPConnectionPoint;

    public bool IsHorizontalDoor;
    public bool IsConnected;


    public override string ToString()
    {
        return "Width: " + Width + " Height: " + Height + " Position: " + Position + " Direction: " + Direction + " IsDoorTowardsDirection: " + IsDoorTowardsDirection + " IsDoorOnPath: " + IsDoorOnPath;
    }
}
