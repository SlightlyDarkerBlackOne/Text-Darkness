using UnityEngine;
using System.Collections;
using UnityEngine.Assertions;

public class StorySimulator : MonoBehaviour
{
    [SerializeField] private RoomManager m_roomManager;
    [SerializeField] private ChoiceManager m_choiceManager;
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
        while (m_roomManager.IsTyping)
        {
            yield return null;
        }
        Debug.Log("Starting story simulation...");

        // Patient Room sequence
        yield return SimulateChoice("look bed");
        Assert.IsTrue(m_roomManager.CurrentRoom.title == "Patient Room", "Title should be revealed after looking at bed");
        
        yield return SimulateChoice("read journal");
        yield return SimulateChoice("look mirror");
        yield return SimulateChoice("touch mirror");
        Assert.IsTrue(m_choiceManager.HasItem("journal"), "Journal should be in inventory");
        
        yield return SimulateChoice("search bed");
        Assert.IsTrue(m_choiceManager.HasItem("scalpel"), "Scalpel should be in inventory");
        
        yield return SimulateChoice("enter mirror");

        // Shadow Hallway sequence
        Assert.IsTrue(m_roomManager.CurrentRoom.title == "Shadow Hallway", "Should be in Shadow Hallway");
        yield return SimulateChoice("run away");
        yield return SimulateChoice("follow whispers");
        yield return SimulateChoice("trace symbols");

        // Operating Room sequence
        Assert.IsTrue(m_roomManager.CurrentRoom.title == "Operating Room", "Should be in Operating Room");
        yield return SimulateChoice("inspect tools");
        yield return SimulateChoice("cut restraints");
        yield return SimulateChoice("search cabinet");
        yield return SimulateChoice("4732");

        Assert.IsTrue(m_choiceManager.HasItem("exit_key"), "Exit key should be in inventory");
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
        Assert.IsTrue(m_roomManager.IsInRoom("patient_room"), "Should return to Patient Room after denying truth");

        Debug.Log("Story simulation completed successfully!");
    }

    private IEnumerator SimulateAlternativePaths()
    {
        // Test invalid choices
        yield return SimulateChoice("invalid command");
        Assert.IsFalse(m_choiceManager.LastChoiceWasValid, "Invalid command should not be accepted");

        // Test requirements
        yield return SimulateChoice("enter mirror");
        Assert.IsFalse(m_choiceManager.LastChoiceWasValid, "Should not be able to enter mirror without looking first");

        // Test item requirements
        yield return SimulateChoice("cut restraints");
        Assert.IsFalse(m_choiceManager.LastChoiceWasValid, "Should not be able to cut restraints without scalpel");

        Debug.Log("Alternative paths tested successfully!");
    }

    private IEnumerator SimulateChoice(string choice)
    {
        Debug.Log($"Simulating choice: {choice}");
        m_choiceManager.ProcessChoice(choice);
        
        // Wait for typing to complete
        while (m_roomManager.IsTyping)
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