using System;
using System.Collections.Generic;
using System.Linq;
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
        public ConflictSet(MyPath path, Step firstStep)
        {
            Path = path;
            Steps.Add(firstStep);
        }

        public MyPath Path;
        public List<Step> Steps = new List<Step>();

        public RectInt Bounds()
        {
            return Steps.Bounds();
        }
    }

    [System.Serializable]
    public class LayoutAnalysis
    {

    }

    public MyPath(MyPathInput input, MyPathSettings settings)
    {
        Input = input;
        Settings = settings;

        foreach (var inputStep in input.Steps)
        {
            AddNext(inputStep);
        }

        if (settings.m_AlignFixedPoints)
        {
            AlignFixedPoints();
        }

        if (settings.m_SeparateConflictSets)
        {
            SeparateConflictSets();
        }
    }

    public MyPathInput Input { get; }

    public MyPathSettings Settings { get; }

    public List<Step> Steps { get; } = new List<Step>();

    public List<ConflictSet> ConflictSets { get; } = new List<ConflictSet>();

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
            conflictSet = new ConflictSet(this, step);
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
