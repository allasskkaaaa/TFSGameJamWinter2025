using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
 
public class CanvasManager : MonoBehaviour
{
    [SerializeField] AudioManager asm;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip pauseSound;
    [SerializeField] AudioClip gameMusic;
    [SerializeField] AudioClip titleMusic;
    [SerializeField] AudioClip buttonSound;

    [SerializeField] AudioMixer audioMixer;

    [Header("Buttons")]
    [SerializeField] Button startButton;
    [SerializeField] Button settingsButton;
    [SerializeField] Button backButton;
    [SerializeField] Button quitButton;
    [SerializeField] Button returnToMenuButton;
    [SerializeField] Button backToPauseMenu;
    [SerializeField] Button resumeGame;

    [Header("Menus")]
    [SerializeField] GameObject mainMenu;
    [SerializeField] GameObject pauseMenu;
    [SerializeField] GameObject settingsMenu;

    [Header("Text")]
    [SerializeField] TMP_Text scoreText;

    [Header("Health Bar")]
    [SerializeField] private Image healthBarImage;

    [Header("Sliders")]
    [SerializeField] Slider masterSlider;
    [SerializeField] Slider musicSlider;
    [SerializeField] Slider sfxSlider;

    [SerializeField] private Button[] menuButtons;
    [SerializeField] int currentButtonIndex = 0;

    void Start()
    {
        asm = GetComponent<AudioManager>();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreValueChanged.AddListener(UpdateScoreText);
            GameManager.Instance.OnHealthValueChanged.AddListener(UpdateHealthBar);
        }

        if (masterSlider != null)
        {
            masterSlider.onValueChanged.AddListener((value) => OnSliderValueChanged(value, "MasterVol"));
        }

        if (musicSlider != null)
        {
            musicSlider.onValueChanged.AddListener((value) => OnSliderValueChanged(value, "MusicVol"));
        }

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.AddListener((value) => OnSliderValueChanged(value, "SFXVol"));
        }

        InitializeButton(startButton, StartGame);
        InitializeButton(settingsButton, ShowSettingsMenu);
        InitializeButton(backButton, ShowMainMenu);
        InitializeButton(backToPauseMenu, UnpauseGame);
        InitializeButton(quitButton, Quit);
        InitializeButton(resumeGame, UnpauseGame);
        InitializeButton(returnToMenuButton, LoadTitle);
    }

    void InitializeButton(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button != null)
        {
            button.onClick.AddListener(action);
            EventTrigger buttonTrigger = button.gameObject.AddComponent<EventTrigger>();
            AddPointerEnterEvent(buttonTrigger, PlayButtonSound);
        }
    }

    void LoadTitle()
    {
        SceneManager.LoadScene("MainMenu");
    }
    void UnpauseGame()
    {
        Time.timeScale = 1.0f;
        GameManager.Instance.SwitchState(GameManager.GameState.Playing);
        pauseMenu.SetActive(false);
        if (settingsMenu) settingsMenu.SetActive(false);
    }

    void UpdateScoreText(int value)
    {
        scoreText.text = value.ToString();
    } 

    void UpdateHealthBar(float currentHealth)
    {
        if (healthBarImage != null && GameManager.Instance != null)
        {
            float fill = currentHealth / 100; 
            healthBarImage.fillAmount = Mathf.Clamp01(fill); 
        }
    }

    void Update()
    {
        if (!pauseMenu) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            pauseMenu.SetActive(!pauseMenu.activeSelf);
            if (settingsMenu) settingsMenu.SetActive(false);


            if (pauseMenu.activeSelf)
            {
                Time.timeScale = 0f;
                GameManager.Instance.SwitchState(GameManager.GameState.Paused);
                asm.PlayOneShot(pauseSound, false);
                pauseMenu.SetActive(true);
            }

            else
            {
                UnpauseGame();
            }

        }
    }
    void ShowSettingsMenu()
    {
        if (mainMenu) mainMenu.SetActive(false);
        if (pauseMenu) pauseMenu.SetActive(false);
        settingsMenu.SetActive(true);
        if (masterSlider != null)
        {
            float value;
            audioMixer.GetFloat("MasterVol", out value);
            masterSlider.value = value + 80;
        }
        else
        {
            Debug.LogError("Audio Mixer is not assigned.");
        }

        if (musicSlider != null)
        {
            float value;
            audioMixer.GetFloat("MusicVol", out value);
            musicSlider.value = value + 80;
        }

        if (sfxSlider != null)
        {
            float value;
            audioMixer.GetFloat("SFXVol", out value);
            sfxSlider.value = value + 80;
        }
    }

    void ShowMainMenu()
    {
        settingsMenu.SetActive(false);
        mainMenu.SetActive(true);
    }
    void StartGame()
    {
        SceneManager.LoadScene("Room_NarrowSmall_S"); ;
        Time.timeScale = 1.0f;
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.clip = gameMusic;
            audioSource.Play();
        }
        else
        {
            Debug.LogError("Audio Source is not assigned.");
        }
    }

    void OnSliderValueChanged(float value, string volume)
    {
        audioMixer.SetFloat(volume, value - 80);
    }

    private void AddPointerEnterEvent(EventTrigger trigger, UnityEngine.Events.UnityAction action)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerEnter;
        entry.callback.AddListener((eventData) => action.Invoke());
        trigger.triggers.Add(entry);
    }

    void PlayButtonSound()
    {
        asm.PlayOneShot(buttonSound, false);
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreValueChanged.RemoveListener(UpdateScoreText);
            GameManager.Instance.OnHealthValueChanged.RemoveListener(UpdateHealthBar);
        }
    }

    void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit(); 
#endif
    }
}