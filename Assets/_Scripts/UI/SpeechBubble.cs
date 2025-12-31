using System.Threading;
using UnityEngine;

public class SpeechBubble : MonoBehaviour
{
    [SerializeField] private GameObject dialogueOBJ;
    [SerializeField] private float appearanceTimer = 5f; //How long the speech bubble will last

    private bool startTimer = false;
    private float timer;

    //Activate the speech bubble when player enters range
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            //Only sets the dialogue as active if it isn't already
            if (!startTimer)
            {
                dialogueOBJ.SetActive(true);
                startTimer = true; ;
            }
            
        }
    }

    private void Start()
    {
        timer = appearanceTimer;
    }
    private void Update()
    {
        //once triggered, the timer counts down. When the timer reaches 0, it restarts
        if (startTimer)
        {
            if (timer > 0)
            {
                timer -= Time.deltaTime;
            }
            else
            {
                timer = appearanceTimer;
                dialogueOBJ.SetActive(false);
                startTimer = false;
            }
        }
    }
}
