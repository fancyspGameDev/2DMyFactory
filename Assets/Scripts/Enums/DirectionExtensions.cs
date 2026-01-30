using UnityEngine;

public static class DirectionExtensions
{
    public static Vector2Int ToVector(this Direction direction)
    {
        switch (direction)
        {
            case Direction.North: return Vector2Int.up;
            case Direction.East: return Vector2Int.right;
            case Direction.South: return Vector2Int.down;
            case Direction.West: return Vector2Int.left;
            default: return Vector2Int.zero;
        }
    }

    public static Direction ToDirection(this Vector2Int vector)
    {
        if (vector == Vector2Int.up) return Direction.North;
        if (vector == Vector2Int.right) return Direction.East;
        if (vector == Vector2Int.down) return Direction.South;
        if (vector == Vector2Int.left) return Direction.West;
        return Direction.North; // Default
    }
}
