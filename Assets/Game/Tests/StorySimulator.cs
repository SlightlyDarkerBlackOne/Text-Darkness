using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;

public class StorySimulator : MonoBehaviour
{
    [SerializeField] private GameStateManager m_gameManager;
    [SerializeField] private bool m_autoStart = false;

    private void Start()
    {
        if (m_autoStart)
        {
            StartCoroutine(SimulateMainPath());
        }
    }
    
    [ContextMenu("Start Simulation")]
    public void StartSimulation()
    {
        StartCoroutine(SimulateMainPath());
    }

    private IEnumerator SimulateMainPath()
    {
        yield return new WaitForSeconds(1f);
        while (m_gameManager.IsTyping)
        {
            yield return null;
        }
        Debug.Log("Starting story simulation...");

        // Patient Room sequence
        yield return SimulateChoice("look bed");
        Assert.IsTrue(m_gameManager.CurrentRoom.title == "Patient Room", "Title should be revealed after looking at bed");
        
        yield return SimulateChoice("look mirror");
        yield return SimulateChoice("touch mirror");
        yield return SimulateChoice("read journal");
        Assert.IsTrue(m_gameManager.HasItem("journal"), "Journal should be in inventory");
        
        yield return SimulateChoice("search bed");
        Assert.IsTrue(m_gameManager.HasItem("scalpel"), "Scalpel should be in inventory");
        
        yield return SimulateChoice("enter mirror");

        // Shadow Hallway sequence
        Assert.IsTrue(m_gameManager.CurrentRoom.title == "Shadow Hallway", "Should be in Shadow Hallway");
        yield return SimulateChoice("follow whispers");
        yield return SimulateChoice("trace symbols");

        // Operating Room sequence
        Assert.IsTrue(m_gameManager.CurrentRoom.title == "Operating Room", "Should be in Operating Room");
        yield return SimulateChoice("inspect tools");
        yield return SimulateChoice("cut restraints");
        yield return SimulateChoice("search cabinet");
        Assert.IsTrue(m_gameManager.HasItem("exit_key"), "Exit key should be in inventory");
        yield return SimulateChoice("leave room");

        // Forgotten Exit sequence
        yield return SimulateChoice("use exit key");
        yield return SimulateChoice("attack doctor");
        yield return SimulateChoice("check desk");
        yield return SimulateChoice("burn file");

        // Final Truth - Good Ending
        yield return SimulateChoice("accept truth");
        Debug.Log("Completed good ending path");

        // Test bad ending
        yield return SimulateChoice("deny truth");
        Assert.IsTrue(m_gameManager.IsInRoom("patient_room"), "Should return to Patient Room after denying truth");

        Debug.Log("Story simulation completed successfully!");
    }

    private IEnumerator SimulateAlternativePaths()
    {
        // Test invalid choices
        yield return SimulateChoice("invalid command");
        Assert.IsFalse(m_gameManager.LastChoiceWasValid, "Invalid command should not be accepted");

        // Test requirements
        yield return SimulateChoice("enter mirror");
        Assert.IsFalse(m_gameManager.LastChoiceWasValid, "Should not be able to enter mirror without looking first");

        // Test item requirements
        yield return SimulateChoice("cut restraints");
        Assert.IsFalse(m_gameManager.LastChoiceWasValid, "Should not be able to cut restraints without scalpel");

        Debug.Log("Alternative paths tested successfully!");
    }

    private IEnumerator SimulateChoice(string choice)
    {
        Debug.Log($"Simulating choice: {choice}");
        m_gameManager.ProcessChoice(choice);
        
        // Wait for typing to complete
        while (m_gameManager.IsTyping)
        {
            yield return null;
        }
    }

    private IEnumerator TestDevCommands()
    {
        yield return SimulateChoice("/choices");
        yield return SimulateChoice("/finished");
        Debug.Log("Dev commands tested successfully!");
    }
} 