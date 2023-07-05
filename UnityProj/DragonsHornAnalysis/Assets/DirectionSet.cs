using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public struct DirectionSet: IEnumerable<Direction>
{
    public DirectionSet(IEnumerable<Direction> directions)
    {
        Bits = 0;
        foreach (Direction direction in directions)
        {
            Bits |= 1 << (int)direction;
        }
    }
    public DirectionSet(params Direction[] directions)
    {
        Bits = 0;
        foreach (Direction direction in directions)
        {
            Bits |= 1 << (int)direction;
        }
    }

    private int Bits { get; }

    public bool Contains(Direction direction)
    {
        return (Bits & (1 << (int)direction)) != 0;
    }

    public bool Contains(DirectionSet directions)
    {
        return (Bits & directions.Bits) == directions.Bits;
    }

    public IEnumerator<Direction> GetEnumerator()
    {
        foreach (Direction direction in Enum.GetValues(typeof(Direction)))
        {
            if (Contains(direction))
                yield return direction;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public DirectionSet Opposites => new DirectionSet(this.Select(DirectionExtensions.Opposite));
}
