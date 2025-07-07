using UnityEngine;

/// <summary>
/// Example script demonstrating how to use the refactored enum-based PuzzleSystem
/// </summary>
public class PuzzleSystemExample : MonoBehaviour
{    
    [SerializeField] private PuzzleSystem m_puzzleSystem;
    
    [Header("Example Puzzle IDs")]
    [SerializeField] private string m_doorPuzzleId = "door_puzzle";
    [SerializeField] private string m_safePuzzleId = "safe_puzzle";
    [SerializeField] private string m_computerPuzzleId = "computer_puzzle";

    private void Start()
    {
        // Initialize puzzles
        m_puzzleSystem.InitializePuzzle(m_doorPuzzleId);
        m_puzzleSystem.InitializePuzzle(m_safePuzzleId);
        m_puzzleSystem.InitializePuzzle(m_computerPuzzleId);
        
        // Set some puzzles to locked state
        m_puzzleSystem.UpdatePuzzleState(m_safePuzzleId, PuzzleStateType.Locked);
        m_puzzleSystem.UpdatePuzzleState(m_computerPuzzleId, PuzzleStateType.Locked);
    }

    [ContextMenu("Unlock Safe Puzzle")]
    public void UnlockSafePuzzle()
    {
        m_puzzleSystem.UpdatePuzzleState(m_safePuzzleId, PuzzleStateType.Unlocked);
        Debug.Log($"Safe puzzle state: {m_puzzleSystem.GetPuzzleState(m_safePuzzleId)}");
    }

    [ContextMenu("Start Safe Puzzle")]
    public void StartSafePuzzle()
    {
        m_puzzleSystem.UpdatePuzzleState(m_safePuzzleId, PuzzleStateType.InProgress);
        Debug.Log($"Safe puzzle is now in progress: {m_puzzleSystem.IsPuzzleInProgress(m_safePuzzleId)}");
    }

    [ContextMenu("Complete Safe Puzzle")]
    public void CompleteSafePuzzle()
    {
        m_puzzleSystem.UpdatePuzzleState(m_safePuzzleId, PuzzleStateType.Completed);
        Debug.Log($"Safe puzzle completed: {m_puzzleSystem.IsPuzzleComplete(m_safePuzzleId)}");
    }

    [ContextMenu("Fail Safe Puzzle")]
    public void FailSafePuzzle()
    {
        m_puzzleSystem.UpdatePuzzleState(m_safePuzzleId, PuzzleStateType.Failed);
        Debug.Log($"Safe puzzle failed: {m_puzzleSystem.IsPuzzleComplete(m_safePuzzleId)}");
    }

    [ContextMenu("Reset All Puzzles")]
    public void ResetAllPuzzles()
    {
        m_puzzleSystem.ResetPuzzle(m_doorPuzzleId);
        m_puzzleSystem.ResetPuzzle(m_safePuzzleId);
        m_puzzleSystem.ResetPuzzle(m_computerPuzzleId);
        Debug.Log("All puzzles reset to initial state");
    }

    [ContextMenu("Check Puzzle States")]
    public void CheckPuzzleStates()
    {
        Debug.Log($"Door puzzle state: {m_puzzleSystem.GetPuzzleState(m_doorPuzzleId)}");
        Debug.Log($"Safe puzzle state: {m_puzzleSystem.GetPuzzleState(m_safePuzzleId)}");
        Debug.Log($"Computer puzzle state: {m_puzzleSystem.GetPuzzleState(m_computerPuzzleId)}");
        
        Debug.Log($"Safe puzzle locked: {m_puzzleSystem.IsPuzzleLocked(m_safePuzzleId)}");
        Debug.Log($"Safe puzzle in progress: {m_puzzleSystem.IsPuzzleInProgress(m_safePuzzleId)}");
        Debug.Log($"Safe puzzle complete: {m_puzzleSystem.IsPuzzleComplete(m_safePuzzleId)}");
    }
} 