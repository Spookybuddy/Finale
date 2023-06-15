using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject[] menus;
    public Slider[] sliders;
    public TextMeshProUGUI itemDisplay;
    public TextMeshProUGUI markersPlaced;
    public TextMeshProUGUI interactPrompt;
    public GameObject[] batteryLevels;
    private float battery;
    private int display;
    private bool canPlaySound;
    public AudioClip[] noises;
    private AudioSource playerAudio;
    private Movit player;

    //Settings save Data
    public float Volume;
    public float Sounds;
    public float Musics;
    public float Difficulty;
    public float Brightness;

    void Start()
    {
        player = GameObject.FindWithTag("Player").GetComponent<Movit>();
        playerAudio = GetComponent<AudioSource>();
        StartCoroutine(soundDelay(2));

        //Read in saved settings variables
        Volume = GetSaveData("V");
        Sounds = GetSaveData("S");
        Musics = GetSaveData("M");
        Difficulty = GetSaveData("D");
        Brightness = GetSaveData("B");
    }

    //Change menu states
    void Update()
    {
        //Display name of first item in player inventory
        if (player.inventory.Count > 0 && !player.menuUp) {
            itemDisplay.text = player.inventory[0].gameObject.name;
        } else {
            itemDisplay.text = "---";
        }

        //Display the number of markers placed
        markersPlaced.text = (player.markers).ToString("F0") + "/10";

        //Display interact when in range
        interactPrompt.gameObject.SetActive(player.interactRange);

        //Display flashlight battery as a battery icon
        battery = (player.flashBat / player.flashMax * 100);
        if (battery > 75) {
            display = 0;
        } else if (battery > 50) {
            display = 1;
        } else if (battery > 25) {
            display = 2;
        } else {
            display = 3;
        }

        //If the number matches the value display, set it as active and hide the others
        for (int b = 0; b < batteryLevels.Length; b++) {
            batteryLevels[b].gameObject.SetActive(b == display);
        }

        //Play a sound randomly
        if (canPlaySound) {
            Ambiance();
            canPlaySound = false;
        }
    }

    //Switch to given menu
    public void MenuState(int state)
    {
        Cursor.lockState = CursorLockMode.None;
        for (int i = 0; i < menus.Length; i++) menus[i].SetActive(false);
        menus[state].SetActive(true);
        if (state == 0) RecordData();
    }

    //Remove menus
    public void MenuDown()
    {
        for (int i = 0; i < menus.Length; i++) menus[i].SetActive(false);
        player.menuUp = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    //Quit game
    public void MenuQuit() { Application.Quit(); }

    //Reload the game to generate a new cave system
    public void MenuReload()
    {
        MenuState(0);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    //Read in slider changes
    public void MenuSlider()
    {
        Volume = sliders[0].value;
        Sounds = sliders[1].value;
        Musics = sliders[2].value;
        Difficulty = sliders[3].value;
        Brightness = sliders[4].value;
    }

    //Record all settings data
    private void RecordData()
    {
        SaveData("V", Volume);
        SaveData("S", Sounds);
        SaveData("M", Musics);
        SaveData("D", Difficulty);
        SaveData("B", Brightness);
    }

    //Save Data
    private void SaveData(string Key, float Data) { PlayerPrefs.SetFloat(Key, Data); }

    //Return the saved float data, 1 otherwise
    private float GetSaveData(string Key)
    {
        if (PlayerPrefs.HasKey(Key)) {
            return PlayerPrefs.GetFloat(Key);
        } else {
            PlayerPrefs.SetFloat(Key, 1);
            return 1;
        }
    }

    private void Ambiance()
    {
        //play one of the longest ones when main menu is up
        int rando = 0;
        float strength = Random.Range(0.2f, 0.4f);
        rando = Random.Range(0, noises.Length);
        playerAudio.PlayOneShot(noises[rando], strength);

        //Sounds are stored shortest to longest (2s - 63s) so delay is based on the index
        soundDelay((rando * rando + 2));
    }

    IEnumerator soundDelay(int delay)
    {
        yield return new WaitForSeconds(delay);
        canPlaySound = true;
    }
}