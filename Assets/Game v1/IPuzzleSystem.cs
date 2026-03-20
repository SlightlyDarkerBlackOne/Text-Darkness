using UnityEngine;

public interface IPuzzleSystem
{
    void InitializePuzzle(string _puzzleId);
    void UpdatePuzzleState(string _puzzleId, PuzzleStateType _state);
    bool IsPuzzleComplete(string _puzzleId);
    void ResetPuzzle(string _puzzleId);
} 