using UnityEngine;

public class MyRawPathInput
{

    [System.Serializable]
    public class Step
    {
        public string Direction;
        public string Room;
        public string LR;
        public string CrystalR;
        public string CrystalG;
        public string CrystalB;
        public string CrystalDestroyed;
    }

    public Step[] Steps;

    public MyRawPathInput(string csvText)
    {
        Steps = CSVSerializer.Deserialize<Step>(csvText);
    }
}
