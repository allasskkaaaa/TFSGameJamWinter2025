using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomGenerator : MonoBehaviour
{

    // --------------------
    // RULES
    // --------------------
    // 1. Start scene is consistent
    // 2. Second-last scene only connects to boss
    // 3. Random connections between middle scenes
    // 4. Last scene is boss
    // 5. Graph persists (no reset)
    // 6. Backtracking allowed *** IMPORTANT AS IT DOESNT WORK RNN 
    // 7. Boss can be key-locked later 

    // NEXT ISSUE TO TACKLE: how do I put the sprite of the door in the right place based on direction? Like what if no door :( 
    public static RoomGenerator Instance { get; private set; }

    public List<string> scenes;

    // Room -> (Direction -> Connected Room)
    private Dictionary<string, Dictionary<Direction, string>> graph =
        new Dictionary<string, Dictionary<Direction, string>>();

    private static bool graphGenerated = false;

    public enum Direction
    {
        North,
        South,
        East,
        West
    }

    // Making sure this DOOESNT reset on scene load, needs to persist over all scenes
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (!graphGenerated)
        {
            GenerateGraph();
            graphGenerated = true;
        }
    }

    // Generate the room graph based on the rules
    void GenerateGraph()
    {
        // Error checking in case I FORGET TO PUT THE ROOMS!!!!!!! (Reminder to add them into the build profile)
        if (scenes == null || scenes.Count < 3)
        {
            Debug.LogError("Need at least 3 scenes.");
            return;
        }

        // Initialize the empty graph
        foreach (var scene in scenes)
            graph[scene] = new Dictionary<Direction, string>();

        // Making sure the start and boss rooms are CONSISTENT
        string start = scenes[0];
        string boss = scenes[^1];
        string preBoss = scenes[^2];

        List<string> middleScenes = scenes.GetRange(1, scenes.Count - 3);
        Shuffle(middleScenes);

        // Startng connections from START room will always be 1-2 
        int startConnections = Mathf.Min(2, middleScenes.Count);
        for (int i = 0; i < startConnections; i++)
            ConnectAuto(start, middleScenes[i]);

        //Random connections between MIDDLE rooms
        foreach (var scene in middleScenes)
        {
            int connections = UnityEngine.Random.Range(1, 3);

            for (int i = 0; i < connections; i++)
            {
                string other = middleScenes[UnityEngine.Random.Range(0, middleScenes.Count)];

                if (other == scene) continue;
                if (graph[scene].ContainsValue(other)) continue;

                ConnectAuto(scene, other);
            }
        }

        // Pre-boss connects to boss AND at least one middle room (the previous) 
        ConnectAuto(preBoss, boss);

        if (middleScenes.Count > 0)
            ConnectAuto(preBoss, middleScenes[UnityEngine.Random.Range(0, middleScenes.Count)]);

        Debug.Log("Room graph generated.");
        PrintGraph();
    }

    // Connecting two rooms with opposite directions (dear lord pls work)
    void ConnectAuto(string a, string b)
    {
        if (a == b) return;
        if (graph[a].Count >= 4 || graph[b].Count >= 4) return;

        Direction dirA = GetFreeDirection(a);
        Direction dirB = Opposite(dirA);

        graph[a][dirA] = b;
        graph[b][dirB] = a;
    }

    // Get a random free direction in the given scene 
    Direction GetFreeDirection(string scene)
    {
        List<Direction> free = new List<Direction>();

        foreach (Direction d in Enum.GetValues(typeof(Direction)))
            if (!graph[scene].ContainsKey(d))
                free.Add(d);

        return free[UnityEngine.Random.Range(0, free.Count)];
    }

    Direction Opposite(Direction d)
    {
        return d switch
        {
            Direction.North => Direction.South,
            Direction.South => Direction.North,
            Direction.East => Direction.West,
            Direction.West => Direction.East,
            _ => Direction.North
        };
    }

    void Shuffle(List<string> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int j = UnityEngine.Random.Range(i, list.Count);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    public void Go(Direction dir)
    {
        string current = SceneManager.GetActiveScene().name;

        if (!graph.ContainsKey(current))
        {
            Debug.LogError($"Scene {current} not in graph.");
            return;
        }

        if (!graph[current].ContainsKey(dir))
        {
            Debug.Log("No door in that direction.");
            return;
        }

        SceneManager.LoadScene(graph[current][dir]);
    }


    // USE THIS TO MAKE THE DOOR VISIBLE!!
    public bool HasDoor(Direction dir)
    {
        string current = SceneManager.GetActiveScene().name;
        return graph.ContainsKey(current) && graph[current].ContainsKey(dir);
    }

    void PrintGraph()
    {
        Debug.Log("===== ROOM GRAPH =====");

        foreach (var room in graph)
        {
            string line = $"{room.Key} -> ";

            foreach (var connection in room.Value)
            {
                line += $"[{connection.Key}: {connection.Value}] ";
            }

            Debug.Log(line);
        }

        Debug.Log("======================");
    }

}
