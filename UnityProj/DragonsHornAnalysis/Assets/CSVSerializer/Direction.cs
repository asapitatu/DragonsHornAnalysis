using System;
using UnityEngine;

public enum Direction
{
    [EnumValueName("N")]
    North,
    [EnumValueName("E")]
    East,
    [EnumValueName("S")]
    South,
    [EnumValueName("W")]
    West,
}

public static class DirectionExtensions
{
    public static Direction Opposite(this Direction direction)
    {

        switch (direction)
        {
            case Direction.North:
                return Direction.South;
            case Direction.East:
                return Direction.West;
            case Direction.South:
                return Direction.North;
            case Direction.West:
                return Direction.East;
        }
        throw new ArgumentOutOfRangeException();
    }

    public static Vector2Int Unit(this Direction direction)
    {
        switch (direction)
        {
            case Direction.North:
                return new Vector2Int(0, 1);
            case Direction.East:
                return new Vector2Int(1, 0);
            case Direction.South:
                return new Vector2Int(0, -1);
            case Direction.West:
                return new Vector2Int(-1, 0);
        }
        throw new ArgumentOutOfRangeException();
    }

    public static Vector2 Rotate(this Direction? direction, Vector2 point)
    {
        switch (direction)
        {
            case Direction.North:
            case null:
                return point;
            case Direction.East:
                return new Vector2(point.y, -point.x);
            case Direction.South:
                return -point;
            case Direction.West:
                return new Vector2(-point.y, point.x);
        }
        throw new ArgumentOutOfRangeException();
    }
}
