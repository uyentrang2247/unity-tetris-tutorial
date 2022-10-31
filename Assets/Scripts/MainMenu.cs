using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour {

    // this is the button panels
    public GameObject[] buttons;
    public GameObject highScorePanel;
    public Text highscoreText;
    private int buttonSelected;
    private int numberOfButtons;

    public Music music;

    void Awake() {
        numberOfButtons = buttons.Length;
        buttonSelected = 0;
        SelectButton(0);
        music = FindObjectOfType<Music>();
    }

    private void Start()
    {
        if (Highscore.highscore > 0)
        {
            highscoreText.text = Highscore.Get();
            highScorePanel.SetActive(true);
        }
        music.playOpenMusic();
    }

    public void NewGame() {
		SceneManager.LoadScene(1);
	}

    public void Exit() {
        Application.Quit();
    }

    public void OpenControls()
    {
        SceneManager.LoadScene(2);
    }

    void openSelected() {
        if (buttonSelected == 0)
        {
            NewGame();
        }
        else if (buttonSelected == 1)
        {
            OpenControls();
        }
        else if (buttonSelected == 2)
        {
            Exit();
        }
    }

    public void SelectButton(int buttonIndex)
    {
        if (buttonIndex > buttons.Length) return;

        for (int i = 0; i < buttons.Length; i++)
        {
            if (i == buttonIndex) buttons[i].SetActive(true);
            else buttons[i].SetActive(false);
        }

        buttonSelected = buttonIndex;
    }

    void changePanel(int direction) {
        buttons[buttonSelected].SetActive(false);
        //Debug.LogFormat("Old selected: {0}", buttonSelected);
        buttonSelected = Utils.Mod(buttonSelected + direction,  numberOfButtons);
        //Debug.LogFormat("New selected: {0}", buttonSelected);
        buttons[buttonSelected].SetActive(true);
    }

    public void Update() {
        // arrow browsing menu
        if (Input.GetKeyDown(KeyCode.UpArrow)) {
            changePanel(-1); // up
        } else if (Input.GetKeyDown(KeyCode.DownArrow)) {
            changePanel(1); // down
        } else if (Input.GetKeyDown(KeyCode.Return)) {
            openSelected(); // open selected button
        } else if (Input.GetKeyDown(KeyCode.Escape)) {
            Application.Quit(); // quit
        }
    }

}
