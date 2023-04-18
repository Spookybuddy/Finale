using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public TextMeshProUGUI pausedMenu;
    public TextMeshProUGUI quitConfirm;
    public TextMeshProUGUI mainMenu;
    public TextMeshProUGUI storyMenu;
    public TextMeshProUGUI victoryMenu;
    public TextMeshProUGUI failureMenu;
    public TextMeshProUGUI itemDisplay;
    public TextMeshProUGUI markersPlaced;
    public TextMeshProUGUI interactPrompt;
    public TextMeshProUGUI talkToNpc;
    public TextMeshProUGUI npcDeposit;
    public TextMeshProUGUI driveAway;

    public GameObject[] batteryLevels;
    private float battery;
    private int display;

    private bool canPlaySound;
    public AudioClip[] noises;
    private AudioSource playerAudio;

    private Movit player;
    private Mousing lookat;
    private bool state;

    void Start()
    {
        player = GameObject.Find("Player").GetComponent<Movit>();
        lookat = GameObject.Find("Main Camera").GetComponent<Mousing>();
        playerAudio = GetComponent<AudioSource>();
        StartCoroutine(soundDelay(2));
    }

    //Change menu states
    void Update()
    {
        Cursor.visible = player.menuUp;
        if (player.menuUp) {
            mainMenu.gameObject.SetActive(player.mainMenu);
            storyMenu.gameObject.SetActive(player.storyMenu);
            pausedMenu.gameObject.SetActive(player.paused);
            quitConfirm.gameObject.SetActive(player.confirm);
            victoryMenu.gameObject.SetActive(player.victory);
            failureMenu.gameObject.SetActive(player.failure);
            state = player.menuUp;
        } else if (state != player.menuUp) {
            mainMenu.gameObject.SetActive(false);
            storyMenu.gameObject.SetActive(false);
            pausedMenu.gameObject.SetActive(false);
            quitConfirm.gameObject.SetActive(false);
            victoryMenu.gameObject.SetActive(false);
            failureMenu.gameObject.SetActive(false);
            state = player.menuUp;
        }

        //Display name of first item in player inventory
        if (player.inventory.Count > 0 && !player.mainMenu) {
            itemDisplay.text = player.inventory[0].gameObject.name;
        } else {
            itemDisplay.text = "---";
        }

        //Display the number of markers placed
        markersPlaced.text = (player.markers).ToString("F0") + "/10";

        //Display interact when in range
        interactPrompt.gameObject.SetActive(player.interactRange);
        talkToNpc.gameObject.SetActive(player.talkingRange);

        driveAway.gameObject.SetActive((player.byCar && player.numSaved > 1));
        npcDeposit.gameObject.SetActive((player.byCar && player.numSaved < 2));

        //Display flashlight battery as a battery icon
        battery = (lookat.flashBat/lookat.flashMax*100);
        if (battery > 70) {
            display = 0;
        } else if (battery > 40) {
            display = 1;
        } else if (battery > 10) {
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
            ambiance();
            canPlaySound = false;
        }
    }

    private void ambiance()
    {
        //play one of the longest ones when main menu is up
        int rando = 0;
        float strength = Random.Range(0.2f, 0.4f);
        if (player.mainMenu) {
            rando = Random.Range(8, 9);
        } else {
            rando = Random.Range(0, noises.Length);
        }
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