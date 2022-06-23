using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    [SerializeField] private Image crosshairImg;
    [SerializeField] private Text objectiveTxt;
    [SerializeField] private string objectiveA;
    [SerializeField] private string objectiveB;

    public static HUD Instance { get; private set; }

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        objectiveTxt.text = objectiveA;
        TargetItem.ObjectiveActivatedEvent += delegate { objectiveTxt.text = objectiveB; };
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) == true)
        {
            Application.Quit();
        }
    }

    public void SetCrosshairColour(Color colour)
    {
        if(crosshairImg.color != colour)
        {
            crosshairImg.color = colour;
        }
    }
}
