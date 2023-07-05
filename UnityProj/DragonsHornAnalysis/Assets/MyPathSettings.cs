using System;

[Serializable]
public class MyPathSettings
{
    public bool m_SeparateConflictSets;
    public int m_SeparateConflictSetsMargin = 1;
    public bool m_AlignFixedPoints;

    public RoomLayout[] m_FixedPointLayouts = new RoomLayout[0];
}
