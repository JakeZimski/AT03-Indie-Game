using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class MenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public delegate void MenuButtonAction();

    [Tooltip("The default colour of the bottom.")]
    [SerializeField] private Color defaultColour;
    [Tooltip("the colour of the button when selected.")]
    [SerializeField] private Color selectedColour;
    [Tooltip("the colour of the button when the mouse is over it.")]
    [SerializeField] private Color highlightedColour;
    [SerializeField] private UnityEvent onActivate;

    private bool mouseOver = false;
    private Image image;
    private MenuNavigation instance;

    public event MenuButtonAction ActivateEvent = delegate { };
    public event MenuButtonAction SelectEvent = delegate { };

    private void Awake()
    {
        TryGetComponent(out image);
        transform.parent.TryGetComponent(out instance);
        image.color = defaultColour;
        Debug.Log("Button Selected");
    }

    private void Start()
    {
        image.color = defaultColour;
    }

    private void Update()
    {
        if(mouseOver = true && Input.GetButtonDown("Fire1") == true)
        {
            //if selected for the menu is this button
            if(instance.SelectedButton == this)
            {
                Activate();
            }
            else
            {
                Select();
            }
        }
    }

    /// <summary>
    /// use this method to invoke the selection event for the button
    /// </summary>
    public void Select()
    {
        SelectEvent.Invoke();
    }

    ///<summary> 
    ///use this method to invoke the activation event for the button
    ///</summary> 
    public void Activate()
    {
        ActivateEvent.Invoke();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        mouseOver = true;
        if(instance.SelectedButton != this)
        {
            image.color = highlightedColour;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        mouseOver = false;
        if (image.color == highlightedColour && this != instance.SelectedButton)
        {
            image.color = defaultColour;
        }
    }

    private void OnActivate()
    {
        onActivate.Invoke();
    }

    private void OnSelect()
    {
        if(instance.SelectedButton != null)
        {
            instance.SelectedButton.image.color = instance.SelectedButton.defaultColour;
        }
        instance.SelectedButton = this;
        image.color = selectedColour;
    }

    private void OnEnable()
    {
        ActivateEvent += OnActivate;
        SelectEvent += OnSelect;
    }

    private void OnDisable()
    {
        ActivateEvent -= OnActivate;
        SelectEvent -= OnSelect;
    }
}
