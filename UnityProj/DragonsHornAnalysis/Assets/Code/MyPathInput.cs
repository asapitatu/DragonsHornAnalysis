using UnityEngine;
using System.Linq;

public class MyPathInput : ScriptableObject
{
    [System.Serializable]
    public class Step
    {
        public MyRawPathInput.Step RawData;
        public Direction EntryDirection;
        public RoomLayout RoomLayout;
        public bool LongRest;
        public Crystal Crystal;

        public static Step Parse(MyRawPathInput.Step rawStep)
        {
            return new Step
            {
                RawData = rawStep,
                EntryDirection = EnumExtensions.Parse<Direction>(rawStep.Direction),
                RoomLayout = EnumExtensions.Parse<RoomLayout>(rawStep.Room),
                LongRest = !string.IsNullOrWhiteSpace(rawStep.LR),
                Crystal = Crystal.Parse(rawStep.CrystalR, rawStep.CrystalG, rawStep.CrystalB, rawStep.CrystalDestroyed),
            };
        }
    }

    public Step[] Steps;

    public void Parse(MyRawPathInput rawData)
    {
        Steps = rawData.Steps.Select(s => Step.Parse(s)).ToArray();
    }
}
