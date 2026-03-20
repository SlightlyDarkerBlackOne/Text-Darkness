using UnityEngine;
using System.Collections.Generic;

public class PuzzleSystem : MonoBehaviour, IPuzzleSystem
{
    private Dictionary<string, PuzzleState> m_puzzleStates = new Dictionary<string, PuzzleState>();

    private class PuzzleState
    {
        public PuzzleStateType CurrentState { get; set; }
        public bool IsComplete { get; set; }
    }

    public void InitializePuzzle(string _puzzleId)
    {
        if (!m_puzzleStates.ContainsKey(_puzzleId))
        {
            m_puzzleStates[_puzzleId] = new PuzzleState
            {
                CurrentState = PuzzleStateType.Initial,
                IsComplete = false
            };
        }
    }

    public void UpdatePuzzleState(string _puzzleId, PuzzleStateType _state)
    {
        if (m_puzzleStates.TryGetValue(_puzzleId, out var puzzleState))
        {
            puzzleState.CurrentState = _state;
            
            // Auto-complete logic based on state
            if (_state == PuzzleStateType.Completed)
            {
                puzzleState.IsComplete = true;
            }
            else if (_state == PuzzleStateType.Failed)
            {
                puzzleState.IsComplete = false;
            }
        }
    }

    public bool IsPuzzleComplete(string _puzzleId)
    {
        return m_puzzleStates.TryGetValue(_puzzleId, out var puzzleState) && puzzleState.IsComplete;
    }

    public void ResetPuzzle(string _puzzleId)
    {
        if (m_puzzleStates.TryGetValue(_puzzleId, out var puzzleState))
        {
            puzzleState.CurrentState = PuzzleStateType.Initial;
            puzzleState.IsComplete = false;
        }
    }

    // Additional helper methods for better state management
    public PuzzleStateType GetPuzzleState(string _puzzleId)
    {
        return m_puzzleStates.TryGetValue(_puzzleId, out var puzzleState) 
            ? puzzleState.CurrentState 
            : PuzzleStateType.Initial;
    }

    public bool IsPuzzleInProgress(string _puzzleId)
    {
        return GetPuzzleState(_puzzleId) == PuzzleStateType.InProgress;
    }

    public bool IsPuzzleLocked(string _puzzleId)
    {
        return GetPuzzleState(_puzzleId) == PuzzleStateType.Locked;
    }
} 