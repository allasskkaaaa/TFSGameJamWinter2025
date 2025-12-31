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
    private static Dictionary<string, Dictionary<Direction, string>> graph = new Dictionary<string, Dictionary<Direction, string>>();
    private static Dictionary<string, bool> roomHasKey = new Dictionary<string, bool>(); // Tracking which rooms have keys
    public static Direction lastExitDirection;


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

        roomHasKey.Clear();
        foreach (string room in middleScenes)
            roomHasKey[room] = false; // false = key exists, not collected

        // Randomly pick keys
        int keysToPlace = Mathf.Min(3, middleScenes.Count);
        List<string> shuffled = new List<string>(middleScenes);
        Shuffle(shuffled);

        for (int i = 0; i < keysToPlace; i++)
        {
            // Mark room as having a key
            roomHasKey[shuffled[i]] = false; // still false = uncollected
        }

        // Startng connections from START room will always be 1-2  NVM JUST ONE
        // START room: EXACTLY ONE door, SOUTH only
        if (middleScenes.Count > 0)
        {
            string firstRoom = middleScenes[0];
            ConnectStartSouth(start, firstRoom);

            // Ensuring first middle room has at least one other connection
            if (middleScenes.Count > 1)
            {
                // picking a random other middle room that's not the first
                string extraRoom = middleScenes[UnityEngine.Random.Range(1, middleScenes.Count)];
                ConnectAuto(firstRoom, extraRoom);
            }
        }


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

        // PRE-BOSS -> BOSS (forced directions)
        // Boss ONLY has South
        ConnectBossSouth(boss, preBoss);

        // Pre-boss connects to at least one middle room
        if (middleScenes.Count > 0)
            ConnectAuto(preBoss, middleScenes[UnityEngine.Random.Range(0, middleScenes.Count)]);
        EnsureBidirectionalGraph();

        Debug.Log("Room graph generated.");
        PrintGraph();
    }

    // Connecting two rooms with opposite directions (dear lord pls work)
    void ConnectAuto(string a, string b)
    {
        if (a == scenes[0] || b == scenes[0]) return;
        if (a == scenes[^1] || b == scenes[^1]) return;
        if (a == b) return;

        // If either room already has 4 doors, pick a random direction to overwrite
        Direction dirA, dirB;

        // Find possible directions
        List<Direction> possibleDirs = new List<Direction>();
        foreach (Direction dir in Enum.GetValues(typeof(Direction)))
        {
            Direction opp = Opposite(dir);
            if (!graph[a].ContainsKey(dir) && !graph[b].ContainsKey(opp))
                possibleDirs.Add(dir);
        }

        if (possibleDirs.Count > 0)
        {
            // Normal case: free direction exists
            dirA = possibleDirs[UnityEngine.Random.Range(0, possibleDirs.Count)];
            dirB = Opposite(dirA);
        }
        else
        {
            // force a connection even if one direction is already used
            // Pick a random direction on 'a', overwrite if needed
            dirA = (Direction)UnityEngine.Random.Range(0, 4);
            dirB = Opposite(dirA);

            // removing any existing connections in that direction to overwrite
            if (graph[a].ContainsKey(dirA))
            {
                string oldRoom = graph[a][dirA];
                graph[oldRoom].Remove(Opposite(dirA));
            }
            if (graph[b].ContainsKey(dirB))
            {
                string oldRoom = graph[b][dirB];
                graph[oldRoom].Remove(Opposite(dirB));
            }
        }

        // Connect both ways
        graph[a][dirA] = b;
        graph[b][dirB] = a;
    }


    void ConnectStartSouth(string start, string target)
    {
        // Force: start -> South -> target
        graph[start][Direction.South] = target;
        graph[target][Direction.North] = start;
    }

    void ConnectBossSouth(string boss, string fromRoom)
    {
        // Force: fromRoom -> North -> boss
        // Force: boss -> South -> fromRoom
        graph[boss][Direction.South] = fromRoom;
        graph[fromRoom][Direction.North] = boss;
    }

    void EnsureBidirectionalGraph()
    {
        foreach (var room in graph.Keys)
        {
            foreach (var kvp in graph[room])
            {
                Direction dir = kvp.Key;
                string target = kvp.Value;

                Direction opp = Opposite(dir);

                // If the target room doesn’t have a back connection, add it
                if (!graph[target].ContainsKey(opp))
                {
                    graph[target][opp] = room;
                }
            }
        }
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

        string targetRoom = graph[current][dir];

        // Checking if tje room is the boss room
        string bossRoom = scenes[^1]; // last scene will ALWAYS be boss room
        if (targetRoom == bossRoom)
        {
            if (GameManager.Instance.Keys < 3)
            {
                Debug.Log("You need 3 keys to enter the boss room!");
                return; 
            }
        }

        lastExitDirection = Opposite(dir);
        SceneManager.LoadScene(targetRoom);
    }


    // USE THIS TO MAKE THE DOOR VISIBLE!!
    public bool HasDoor(Direction dir)
    {
        string current = SceneManager.GetActiveScene().name;
        string start = scenes[0];

        if (current == start && dir == Direction.South)
            return true;

        return graph.ContainsKey(current) && graph[current].ContainsKey(dir);
    }

    public bool RoomHasKeyBeenCollected(string room)
    {
        if (!roomHasKey.ContainsKey(room)) return false;
        return roomHasKey[room];
    }

    public void MarkKeyCollected(string room)
    {
        roomHasKey[room] = true;
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
