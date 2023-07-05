using UnityEngine;
using UnityEngine.Assertions;

[System.Serializable]
public class Crystal
{
    public bool Exists = false;
    public Color Color = Color.white;
    public bool Destroyed = false;

    public static Crystal Parse(string r, string g, string b, string destroyed)
    {
        if (string.IsNullOrEmpty(r))
        {
            Assert.IsTrue(string.IsNullOrEmpty(g));
            Assert.IsTrue(string.IsNullOrEmpty(b));
            Assert.IsTrue(string.IsNullOrEmpty(destroyed));
            return new Crystal();
        }

        float rf = int.Parse(r) / 255f;
        float gf = int.Parse(g) / 255f;
        float bf = int.Parse(b) / 255f;
        return new Crystal
        {
            Exists = true,
            Color = new Color(rf, gf, bf),
            Destroyed = !string.IsNullOrWhiteSpace(destroyed),
        };
    }
}
