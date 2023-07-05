using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

public class MyPathMeshGenerator : MonoBehaviour
{
    public MyPathInput m_PathInput;
    public MyPathSettings m_PathSettings;

    public float m_HallWidth = 0.2f;
    public float m_HallArrowLength = 0.05f;
    public float m_CrystalWidth = 0.05f;
    public float m_CrystalHeight = 0.1f;
    public float m_CenterWidth = 0.2f;
    public Color m_DisappearingHallColor = Color.black;
    public Color m_HallColor = Color.grey;
    public Color m_ExitColor = Color.magenta;
    public Color m_CenterColor = Color.white;
    public Color m_ConflictColor = Color.red;

    public Vector3 m_GridScale = Vector3.one;
    public float m_Padding = 0.1f;

    public float m_YOffsetPerStep = 0.01f;
    public float m_YOffsetPerReturn = 0.1f;
    public float m_YOffsetPerConflict = 1.0f;

    public bool m_AnimateSteps = false;
    public float m_StepDuration = 0.2f;
    public float m_AnimatedTime = 0.0f;
    private int m_StepsRendered = -1;

    private MyPath Path { get; set; }

    public MeshFilter m_MeshFilter;

    private List<Vector3> Vertices { get; } = new List<Vector3>();
    private List<Color> Colors { get; } = new List<Color>();
    private List<int> Triangles { get; } = new List<int>();

    private struct Vertex
    {
        public Vertex(Vector3 position, Color color)
        {
            Position = position;
            Color = color;
        }

        public Vector3 Position;
        public Color Color;
    }

    private void DrawTriangle(Vertex a, Vertex b, Vertex c)
    {
        int indexA = Vertices.Count;
        int indexB = indexA + 1;
        int indexC = indexB + 1;
        Vertices.Add(a.Position);
        Vertices.Add(b.Position);
        Vertices.Add(c.Position);
        Colors.Add(a.Color);
        Colors.Add(b.Color);
        Colors.Add(c.Color);
        Triangles.Add(indexA);
        Triangles.Add(indexB);
        Triangles.Add(indexC);
    }

    private void DrawQuad(Vertex a, Vertex b, Vertex c, Vertex d)
    {
        int indexA = Vertices.Count;
        int indexB = indexA + 1;
        int indexC = indexB + 1;
        int indexD = indexC + 1;
        Vertices.Add(a.Position);
        Vertices.Add(b.Position);
        Vertices.Add(c.Position);
        Vertices.Add(d.Position);
        Colors.Add(a.Color);
        Colors.Add(b.Color);
        Colors.Add(c.Color);
        Colors.Add(d.Color);
        Triangles.Add(indexA);
        Triangles.Add(indexB);
        Triangles.Add(indexC);
        Triangles.Add(indexC);
        Triangles.Add(indexD);
        Triangles.Add(indexA);
    }

    private void DrawLine(Vertex from, Vertex to, float width)
    {
        Vector3 direction = Vector3.Normalize(to.Position - from.Position);
        Vector3 right = Vector3.Cross(direction, Vector3.up);

        Vertex bl = new Vertex(from.Position - 0.5f * width * right, from.Color);
        Vertex br = new Vertex(from.Position + 0.5f * width * right, from.Color);
        Vertex tl = new Vertex(to.Position - 0.5f * width * right, to.Color);
        Vertex tr = new Vertex(to.Position + 0.5f * width * right, to.Color);

        DrawQuad(bl, tl, tr, br);
    }

    private void BuildMesh()
    {
        var mesh = new Mesh();
        mesh.SetVertices(Vertices.ToArray());
        mesh.SetTriangles(Triangles.ToArray(), 0);
        mesh.SetColors(Colors.ToArray());
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        m_MeshFilter.sharedMesh = mesh;
    }

    private void GenerateMesh(int steps, bool force = false)
    {
        if (!force && steps == m_StepsRendered)
            return;

        Vertices.Clear();
        Triangles.Clear();
        Colors.Clear();

        float y = 0.0f;
        for (int i = 0; i < steps; ++i)
        {
            DrawStep(Path.Steps[i], ref y);
        }

        BuildMesh();

        m_StepsRendered = steps;
    }

    private Vector3 RoomRelativePosition(MyPath.Step step, Vector2 relativePosition, Direction? direction = null, float y = 0.0f)
    {
        relativePosition = direction.Rotate(relativePosition);
        Vector2 gridPosition = new Vector2(step.GridPosition.x, step.GridPosition.y);
        gridPosition += relativePosition;
        return new Vector3(gridPosition.x * m_GridScale.x, y * m_GridScale.y, gridPosition.y * m_GridScale.z);
    }

    private void DrawRelativeQuad(MyPath.Step step, Vector2 min, Vector2 max, Color color, Direction? direction = null, float y = 0.0f)
    {
        Vector3 a = RoomRelativePosition(step, min, direction, y);
        Vector3 b = RoomRelativePosition(step, new Vector2(min.x, max.y), direction, y);
        Vector3 c = RoomRelativePosition(step, max, direction, y);
        Vector3 d = RoomRelativePosition(step, new Vector2(max.x, min.y), direction, y);

        DrawQuad(new Vertex(a, color), new Vertex(b, color), new Vertex(c, color), new Vertex(d, color));
    }

    private void DrawRelativeTriangle(MyPath.Step step, Vector2 a, Vector2 b, Vector2 c, Color color, Direction? direction = null, float y = 0.0f)
    {
        Vector3 a3 = RoomRelativePosition(step, a, direction, y);
        Vector3 b3 = RoomRelativePosition(step, b, direction, y);
        Vector3 c3 = RoomRelativePosition(step, c, direction, y);

        DrawTriangle(new Vertex(a3, color), new Vertex(b3, color), new Vertex(c3, color));
    }

    private void DrawStep(MyPath.Step step, ref float y)
    {
        if (step.IsLayoutConflict)
        {
            y += m_YOffsetPerConflict;
        }
        else if (step.IsRepeatPosition)
        {
            y += m_YOffsetPerReturn;
        }
        else
        {
            y += m_YOffsetPerStep;
        }

        DrawCenter(step, y);
        DrawCrystal(step, y);
        DrawHalls(step, y);
    }

    private void DrawCenter(MyPath.Step step, float y)
    {
        Color color = step.IsLayoutConflict ? m_ConflictColor : m_CenterColor;
        DrawRelativeQuad(step, -0.5f * m_CenterWidth * Vector2.one, 0.5f * m_CenterWidth * Vector2.one, color, null, y);
    }

    private void DrawCrystal(MyPath.Step step, float y)
    {
        if (!step.Crystal.Exists)
            return;

        DrawRelativeQuad(step, -0.5f * m_CrystalWidth * Vector2.one, 0.5f * m_CrystalWidth * Vector2.one, step.Crystal.Color, null, y + m_CrystalHeight);
    }

    private void DrawHalls(MyPath.Step step, float y)
    {
        DrawHall(step, Direction.North, y);
        DrawHall(step, Direction.East, y);
        DrawHall(step, Direction.South, y);
        DrawHall(step, Direction.West, y);
    }

    private void DrawHall(MyPath.Step step, Direction direction, float y)
    {
        bool isExitDirection = step.ExitDirection == direction;
        bool isEntryDirection = !isExitDirection && step.EntryDirection == direction.Opposite();

        bool exists = step.RoomLayout.ExitDirections().Contains(direction);

        if (!exists && !isEntryDirection)
            return;

        bool isDisappearing = (isEntryDirection && !exists);


        Color color = isExitDirection ? m_ExitColor : (isDisappearing ? m_DisappearingHallColor : (step.IsLayoutConflict ? m_ConflictColor : m_HallColor));

        float arrowLength = isExitDirection ? m_HallArrowLength : 0.0f;

        Vector2 baseMin = new Vector2(-0.5f * m_HallWidth, 0.5f * m_CenterWidth);
        Vector2 baseMax = new Vector2(0.5f * m_HallWidth, 0.5f - arrowLength - m_Padding);
        if (isDisappearing)
        {
            baseMax.y = 0.5f * m_CenterWidth + 0.1f;
        }
        else if (step.NextStep?.EnteredThroughWall ?? false)
        {
            baseMax.y += 0.5f * m_CenterWidth;
        }

        Vector2 arrowA = new Vector2(baseMin.x, baseMax.y);
        Vector2 arrowB = new Vector2(0.0f, 0.5f - m_Padding);
        Vector2 arrowC = new Vector2(baseMax.x, baseMax.y);

        DrawRelativeQuad(step, baseMin, baseMax, color, direction, y);
        if (arrowLength > 0.0f)
        {
            DrawRelativeTriangle(step, arrowA, arrowB, arrowC, color, direction, y);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (m_MeshFilter == null)
            m_MeshFilter = GetComponent<MeshFilter>();

        if (m_MeshFilter == null)
            return;

        if (m_PathInput == null)
            return;

        Path = new MyPath(m_PathInput, m_PathSettings);
    }

    // Update is called once per frame
    void Update()
    {
        int totalSteps = Path.Steps.Count;
        int stepsToRender = totalSteps;
        if (m_AnimateSteps)
        {
            m_AnimatedTime += Time.deltaTime;
            stepsToRender = Mathf.Clamp((int)(m_AnimatedTime / m_StepDuration), 0, totalSteps);
        } 
        else
        {
            m_AnimatedTime = 0;
        }
        GenerateMesh(stepsToRender);
    }
}
