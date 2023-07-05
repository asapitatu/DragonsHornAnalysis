using System;
using Unity.VisualScripting;

public enum RoomLayout
{
    [EnumValueName("C")]
    Cross,
    [EnumValueName("TN")]
    Tee_North,
    [EnumValueName("TE")]
    Tee_East,
    [EnumValueName("TS")]
    Tee_South,
    [EnumValueName("TW")]
    Tee_West,
    [EnumValueName("NE")]
    Corner_NorthEast,
    [EnumValueName("NW")]
    Corner_NorthWest,
    [EnumValueName("SE")]
    Corner_SouthEast,
    [EnumValueName("SW")]
    Corner_SouthWest,
    [EnumValueName("N")]
    DeadEnd_North,
    [EnumValueName("E")]
    DeadEnd_East,
    [EnumValueName("S")]
    DeadEnd_South,
    [EnumValueName("W")]
    DeadEnd_West,
}

public static class RoomLayoutExtensions
{
    public static DirectionSet ExitDirections(this RoomLayout self)
    {
        switch (self)
        {
            case RoomLayout.Cross:
                return new DirectionSet(Direction.North, Direction.East, Direction.South, Direction.West);
            case RoomLayout.Tee_North:
                return new DirectionSet(Direction.North, Direction.East, Direction.West);
            case RoomLayout.Tee_East:
                return new DirectionSet(Direction.North, Direction.East, Direction.South);
            case RoomLayout.Tee_South:
                return new DirectionSet(Direction.East, Direction.South, Direction.West);
            case RoomLayout.Tee_West:
                return new DirectionSet(Direction.North, Direction.South, Direction.West);
            case RoomLayout.Corner_NorthEast:
                return new DirectionSet(Direction.North, Direction.East);
            case RoomLayout.Corner_NorthWest:
                return new DirectionSet(Direction.North, Direction.West);
            case RoomLayout.Corner_SouthEast:
                return new DirectionSet(Direction.South, Direction.East);
            case RoomLayout.Corner_SouthWest:
                return new DirectionSet(Direction.South, Direction.West);
            case RoomLayout.DeadEnd_North:
                return new DirectionSet(Direction.North);
            case RoomLayout.DeadEnd_East:
                return new DirectionSet(Direction.East);
            case RoomLayout.DeadEnd_South:
                return new DirectionSet(Direction.South);
            case RoomLayout.DeadEnd_West:
                return new DirectionSet(Direction.West);
        }
        throw new ArgumentOutOfRangeException();
    }

    public static DirectionSet EntryDirections(this RoomLayout self)
    {
        return ExitDirections(self).Opposites;
    }
}
