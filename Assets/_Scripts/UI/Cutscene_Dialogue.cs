using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Cutscene_Dialogue : MonoBehaviour
{
    [SerializeField] private float timeBetweenBubbles = 4f;
    [SerializeField] private List<GameObject> dialogueBubbles;
    [SerializeField] private string sceneAfterCutscene;

    private void Start()
    {
        foreach (GameObject dialogue in dialogueBubbles)
        {
            dialogue.SetActive(false);
        }

        StartCoroutine(PlayCutscene());
    }

    private IEnumerator PlayCutscene()
    {
        foreach (GameObject dialogue in dialogueBubbles)
        {
            yield return StartCoroutine(ShowBubble(dialogue));
        }

        SceneManager.LoadScene(sceneAfterCutscene);
    }

    private IEnumerator ShowBubble(GameObject dialogueBubble)
    {
        dialogueBubble.SetActive(true);

        float timer = 0f;
        bool skipped = false;

        while (timer < timeBetweenBubbles && !skipped)
        {
            if (Input.GetMouseButtonDown(0))
            {
                skipped = true;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        dialogueBubble.SetActive(false);
    }

}
