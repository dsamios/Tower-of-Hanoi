using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{    
    public TMP_InputField diskNum,diskNum2;
    public Toggle undoButtonToggle,solutionTipToggle;

    // Start is called before the first frame update
    void Start()
    {
        diskNum.text = "5";
        diskNum2.text = "5";
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartGame()
    {
        int disks = int.Parse(diskNum.text);

        if (disks > 0)
        {
            PlayerPrefs.SetInt("disks", disks);
            PlayerPrefsX.SetBool("undoButton", undoButtonToggle.isOn);
            PlayerPrefsX.SetBool("solutionTip", solutionTipToggle.isOn);            

            SceneManager.LoadScene("MainGame");    
        }
    }

    public void Simulate()
    {
        int disks = int.Parse(diskNum2.text);

        if (disks > 0)
        {
            PlayerPrefs.SetInt("disks", disks);
            SceneManager.LoadScene("Simulation");    
        }
    }

    public void QuitGame() 
    {
        // save any game data here
        #if UNITY_EDITOR
            // Application.Quit() does not work in the editor so
            // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }


}
