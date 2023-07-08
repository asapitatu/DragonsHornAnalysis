using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;

public class MyPath
{

    public class Step
    {
        public MyPathInput.Step Input { get; set; }

        public Step PreviousStep { get; set; }

        public Step NextStep { get; set; }

        public Direction EntryDirection => Input.EntryDirection;

        public Direction? ExitDirection => NextStep?.EntryDirection;

        public RoomLayout RoomLayout => Input.RoomLayout;

        public Crystal Crystal => Input.Crystal;

        public int Index { get; set; }

        public Vector2Int RawGridPosition { get; set; }

        public Vector2Int GridAdjustment { get; set; }

        public Vector2Int GridPosition => RawGridPosition + GridAdjustment;

        public bool IsRepeatPosition { get; set; }

        public bool IsLayoutConflict => Index > 0 && LayoutConflictSet.Steps[0] == this;

        public ConflictSet LayoutConflictSet { get; set; }

        public bool EnteredThroughWall { get; set; }
    }

    public class ConflictSet
    {
        public ConflictSet(Step firstStep)
        {
            Steps.Add(firstStep);
        }

        public List<Step> Steps = new List<Step>();

        public RectInt Bounds()
        {
            return Steps.Bounds();
        }
    }

    public class RunShape : ICloneable
    {
        public RunShape(RoomLayout firstLayout)
        {
            Layouts = new List<RoomLayout> { firstLayout };
            Directions = new List<Direction>();
        }

        private RunShape(RunShape other)
        {
            Layouts = new List<RoomLayout>(other.Layouts);
            Directions = new List<Direction>(other.Directions);
        }

        public void Add(Direction dir, RoomLayout layout)
        {
            if (Layouts.Count >= 2)
            {
                Direction dir1 = Directions[Directions.Count - 1];
                RoomLayout layout2 = Layouts[Layouts.Count - 2];
                if (layout2 == layout && dir1 == dir)
                {
                    Directions.RemoveAt(Directions.Count - 1);
                    Layouts.RemoveAt(Layouts.Count - 1);
                    return;
                }
            }
            else
            {
                Directions.Add(dir);
                Layouts.Add(layout);
            }
        }

        private int OverlapSize(RunShape other, int i, int j)
        {
            if (Layouts[i] != other.Layouts[j])
                return 0;

            int max = Mathf.Max(Directions.Count - i, other.Directions.Count - j);
            if (Layouts[i] != other.Layouts[j])
        }

        public (int, int)? FindLargestOverlap(RunShape other)
        {
            for (int i = 0; i < Directions.Count; ++i)
            {
                for (int j = 0; j < other.Directions.Count; ++j)
                {

                }
            }
        }

        public List<RoomLayout> Layouts { get; private set; }

        public List<Direction> Directions { get; private set; }

        object ICloneable.Clone()
        {
            return Clone();
        }

        public RunShape Clone()
        {
            return new RunShape(this);
        }
    }

    public class Run
    {
        public Run(MyPath path, int startIndex, int count)
        {
            Path = path;
            StartIndex = startIndex;

            Count = count;

            for (int i = 1; i < count; ++i)
            {
                Direction dir = next.ExitDirection.Value;
                next = next.NextStep;
                Shape.Add(dir, next.RoomLayout);
            }
        }

        public MyPath Path { get; private set; }

        public int StartIndex { get; private set; }

        public int Count { get; private set; }

        public Step this[int offset]
        {
            get 
            { 
                if (!(offset >= 0 && offset < Count))
                    throw new IndexOutOfRangeException();
                return Path.Steps[StartIndex + offset]; 
            }
        }

        public Step Start => this[0];

        public Step End => this[Count - 1];

        public RunShape Shape { get; }

        public IEnumerable<Step> Steps
        {
            get
            {
                Step next = Start;
                yield return next;
                for (int i = 1; i < Count; ++i)
                {
                    next = next.NextStep;
                    yield return next;
                }
            }
        }

        public bool Extend(int num = 1)
        {
            if (StartIndex + Count + num >= Path.Steps.Count)
                return false;

            for (int i = 0; i < num; ++i)
            {
                Direction dir = End.ExitDirection.Value;
                ++Count;
                RoomLayout layout = End.RoomLayout;
                Shape.Add(dir, layout);
            }
            return true;
        }

        public bool ExtendToMinShapeLength(int minShapeLength)
        {
            while (Shape.Layouts.Count < minShapeLength)
            {
                if (!Extend(minShapeLength - Shape.Layouts.Count))
                    return false;
            }
            return true;
        }
    }

    public class ConsistentRun
    {
        public ConsistentRun(Run run)
        {
            Runs.Add(new OffsetRun(0, run));
            RunShape shape = run.Shape.Clone();
        }

        public struct OffsetRun
        {
            public OffsetRun(int offset, Run run)
            {
                Offset = offset;
                Run = run;
            }

            public int Offset;
            public Run Run;
        }

        public List<OffsetRun> Runs { get; } = new List<OffsetRun>();

        public RunShape Shape { get; private set; }

        public IEnumerable<Step> AllSteps => Runs.SelectMany(or => or.Run.Steps);

        internal static bool ContainAnySameSteps(ConsistentRun a, ConsistentRun b)
        {
            return a.AllSteps.Intersect(b.AllSteps).Any();
        }

        public bool TryMerge(ConsistentRun other, int minOverlap)
        {
            if (Shape.OverlapSize(other.Shape) < minOverlap)
                return false;
        }
    }

    public MyPath(MyPathInput input, MyPathSettings settings)
    {
        Input = input;
        Settings = settings;

        foreach (var inputStep in input.Steps)
        {
            AddNext(inputStep);
        }

        if (settings.m_OrganizeConsistentRuns)
        {
            CalculateConsistentRuns();
        }

        if (settings.m_AlignFixedPoints)
        {
            AlignFixedPoints();
        }

        if (settings.m_SeparateConflictSets)
        {
            SeparateConflictSets();
        }

        if (settings.m_OrganizeConsistentRuns)
        {
            OrganizeConsistentRuns();
        }
    }

    public MyPathInput Input { get; }

    public MyPathSettings Settings { get; }

    public List<Step> Steps { get; } = new List<Step>();

    public List<ConflictSet> ConflictSets { get; } = new List<ConflictSet>();

    public List<ConsistentRun> ConsistentRuns { get; private set; } = new List<ConsistentRun>();

    private void AddNext(MyPathInput.Step input)
    {
        int index = Steps.Count;
        Step previous = Steps.Count > 0 ? Steps[Steps.Count - 1] : null;

        bool enteredThroughWall = input.RoomLayout.EntryDirections().Contains(input.EntryDirection);
        Vector2Int gridPosition = previous == null ? Vector2Int.zero : previous.RawGridPosition + input.EntryDirection.Unit();

        int lastConflict = Steps.Where(s => s.IsLayoutConflict).LastOrDefault()?.Index ?? 0;
        Step[] stepsInSamePosition = Steps.Where(s => s.Index >= lastConflict && s.RawGridPosition == gridPosition).ToArray();
        bool isRepeatPosition = stepsInSamePosition.Length > 0;
        bool isLayoutConflict = isRepeatPosition && stepsInSamePosition[stepsInSamePosition.Length - 1].RoomLayout != input.RoomLayout;

        Step step = new Step
        {
            Input = input,
            Index = index,
            IsRepeatPosition = isRepeatPosition,
            EnteredThroughWall = enteredThroughWall,
            RawGridPosition = gridPosition,
            PreviousStep = previous,
        };

        ConflictSet conflictSet;
        if (isLayoutConflict || index == 0)
        {
            conflictSet = new ConflictSet(step);
            ConflictSets.Add(conflictSet);
        }
        else
        {
            conflictSet = ConflictSets[ConflictSets.Count - 1];
            conflictSet.Steps.Add(step);
        }
        step.LayoutConflictSet = conflictSet;
        if (previous != null)
            previous.NextStep = step;
        Steps.Add(step);
    }

    private bool IsFixedPoint(Step a, Step b)
    {
        return (a.RoomLayout == b.RoomLayout) && Settings.m_FixedPointLayouts.Contains(b.RoomLayout);
    }

    private Step FindFixedPointBefore(Step step)
    {
        Step earlierStep = step.PreviousStep;
        while (earlierStep != null)
        {
            if (IsFixedPoint(earlierStep, step))
                return earlierStep;
            earlierStep = earlierStep.PreviousStep;
        }
        return null;
    }

    private void AlignFixedPoints()
    {
        foreach (Step step in Steps)
        {
            Step fixedPoint = FindFixedPointBefore(step);
            if (fixedPoint != null)
            {
                AlignSteps(fixedPoint, step);
            }
        }
    }

    private void AlignSteps(Step a, Step b)
    {
        if (a == null || b == null || !(a.Index < b.Index))
            throw new ArgumentException();

        Vector2Int offset = a.GridPosition - b.GridPosition;
        OffsetStep(b, offset);
    }

    private void OffsetStep(Step step, Vector2Int offset)
    {
        while (step != null)
        {
            step.GridAdjustment += offset;
            step = step.NextStep;
        }
    }

    private void SetStepPosition(Step step, Vector2Int newPosition)
    {
        OffsetStep(step, newPosition - step.GridPosition);
    }

    private void SeparateConflictSets()
    {
        RectInt bounds = ConflictSets[0].Bounds();

        for (int i = 1; i < ConflictSets.Count; ++i)
        {
            ConflictSet set = ConflictSets[i];
            RectInt setBounds = set.Bounds();
            Vector2Int offset = new Vector2Int(bounds.xMax - setBounds.xMin + Settings.m_SeparateConflictSetsMargin + 1, -(setBounds.yMin + setBounds.yMax) / 2);
            OffsetStep(set.Steps[0], offset);
            setBounds = set.Bounds();
            bounds.min = Vector2Int.Min(setBounds.min, bounds.min);
            bounds.max = Vector2Int.Max(setBounds.max, bounds.max);
        }

    }

    private void CalculateConsistentRuns()
    {
        // Create a run of the minimum length starting at each step
        for (int i = 0; i < Steps.Count - Settings.m_MinConsistentRunLength; ++i)
        {
            Run run = new Run(Steps[i], Settings.m_MinConsistentRunLength);
            run.ExtendToMinShapeLength(Settings.m_MinConsistentRunLength);
            ConsistentRuns.Add(new ConsistentRun(sequence));
        }

        while (true)
        {
            if (!TryMergeRuns())
            {
                return;
            }
        }
    }

    private bool TryMergeRuns()
    {
        int runsBefore = ConsistentRuns.Count;

        List<ConsistentRun> runsToProcess = ConsistentRuns.ToList();

        List<ConsistentRun> runsAfter = new List<ConsistentRun>();
        while (runsToProcess.Count > 0)
        {
            ConsistentRun candidate = runsToProcess[0];
            runsToProcess.RemoveAt(0);

            TryMergeRuns(candidate, runsToProcess);
            runsAfter.Add(candidate);
        }

        ConsistentRuns = runsAfter;
        return runsAfter.Count < runsBefore;
    }

    private void TryMergeRuns(ConsistentRun into, List<ConsistentRun> candidates)
    {
        for (int i = 0; i < candidates.Count; )
        {
            if (TryMergeRuns(into, candidates[i]))
                candidates.RemoveAt(i);
            else
                ++i;
        }
    }

    private bool TryMergeRuns(ConsistentRun a, ConsistentRun b)
    {
        if (ConsistentRun.ContainAnySameSteps(a, b))
        {
            return true;
        }
    }

    private void OrganizeConsistentRuns()
    {

    }
}

public static class MyPathExtensions
{
    public static RectInt Bounds(this IEnumerable<MyPath.Step> steps)
    {
        return Bounds(steps.Select(s => s.GridPosition));
    }

    public static RectInt Bounds(this IEnumerable<Vector2Int> points)
    {
        var min = new Vector2Int(int.MaxValue, int.MaxValue);
        var max = new Vector2Int(int.MinValue, int.MinValue);
        foreach (Vector2Int point in points)
        {
            min = Vector2Int.Min(min, point);
            max = Vector2Int.Max(max, point);
        }

        return new RectInt(min, max - min);
    }
}
