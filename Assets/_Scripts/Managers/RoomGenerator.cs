using System;
using System.Collections.Generic;
using UnityEngine;

public class RoomGenerator : MonoBehaviour
{
    public List<string> scenes;
    Dictionary<string, List<string>> graph = new Dictionary<string, List<string>>();

    // RULES: 
    // 1. Start scene is consistent
    // 2. Second last scene only connects to boss scene
    // 3. Random connections between middle scenes
    // 4. Last scene is boss scene
    // 5. Rooms do not reset 
    // 6. Players can backtrack/explore, but cannot skip to boss fight 
    // 7. Boss scene requires keys 

    void Awake()
    {
        GenerateGraph();
    }

    void GenerateGraph()
    {
        foreach (var scene in scenes)
            graph[scene] = new List<string>();

        string start = scenes[0];
        string boss = scenes[scenes.Count - 1];
        string preBoss = scenes[scenes.Count - 2];

        // RULE 1: Start scene is consistent
        // Connect start to 2 random non-special scenes

        List<string> middleScenes = scenes.GetRange(1, scenes.Count - 3);

        Shuffle(middleScenes);

        for (int i = 0; i < 2 && i < middleScenes.Count; i++)
            Connect(start, middleScenes[i]);

        // RULE 2: ensuring second last room ONLY connects to boss
        Connect(preBoss, boss);

        // we'll randomize connections between middle scenes
        foreach (var scene in middleScenes)
        {
            int connections = UnityEngine.Random.Range(1, 3);
            for (int i = 0; i < connections; i++)
            {
                string other = middleScenes[
                    UnityEngine.Random.Range(0, middleScenes.Count)];
                if (scene == other) continue;
                Connect(scene, other);
            }
        }
    }

    void Connect(string a, string b)
    {
        if (!graph[a].Contains(b))
            graph[a].Add(b);

        if (!graph[b].Contains(a))
            graph[b].Add(a);
    }

    void Shuffle(List<string> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int j = UnityEngine.Random.Range(i, list.Count);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    // To reference the new room: 

    // string nextScene = graph[currentScene] [Random.Range(0, graph[currentScene].Count)];
    // SceneManager.LoadScene(nextScene);
}
