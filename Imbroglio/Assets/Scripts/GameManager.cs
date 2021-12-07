using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public TextMeshProUGUI pausedMenu;
    public TextMeshProUGUI mainMenu;
    public TextMeshProUGUI victoryMenu;
    public TextMeshProUGUI failureMenu;
    public TextMeshProUGUI batteryLife;
    public TextMeshProUGUI interactPrompt;

    private Movit player;
    private Mousing lookat;
    private bool state;

    void Start()
    {
        player = GameObject.Find("Player").GetComponent<Movit>();
        lookat = GameObject.Find("Main Camera").GetComponent<Mousing>();
    }

    //Change menu states
    void Update()
    {
        Cursor.visible = player.menuUp;
        if (player.menuUp) {
            mainMenu.gameObject.SetActive(player.mainMenu);
            pausedMenu.gameObject.SetActive(player.paused);
            victoryMenu.gameObject.SetActive(player.victory);
            failureMenu.gameObject.SetActive(player.failure);
            state = player.menuUp;
        } else if (state != player.menuUp) {
            mainMenu.gameObject.SetActive(false);
            pausedMenu.gameObject.SetActive(false);
            victoryMenu.gameObject.SetActive(false);
            failureMenu.gameObject.SetActive(false);
            state = player.menuUp;
        }

        //Display interact when in range
        interactPrompt.gameObject.SetActive(player.interactRange);

        //Display flashlight battery
        batteryLife.text = (lookat.flashBat/lookat.flashMax*100).ToString("F1") + "%";
    }
}