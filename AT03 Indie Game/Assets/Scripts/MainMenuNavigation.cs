using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuNavigation : MenuNavigation
{
    [SerializeField] private GameObject infoPanel;

    protected override void Start()
    {
        base.Start();
        infoPanel.SetActive(false);
    }

    public void ToggleInfoPanel()
    {
        infoPanel.SetActive(!infoPanel.activeSelf);
    }

    public void QuitToDesktop()
    {
        Application.Quit();
    }

    public void StartGame()
    {
        SceneManager.LoadScene(1);
    }
}
