using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetItem : MonoBehaviour, IInteractable
{
    public delegate void ObjectiveDelegate();

    private static bool active = false;

    public static ObjectiveDelegate ObjectiveActivatedEvent = delegate
    {
        ObjectiveActivatedEvent = delegate { };
    };

    // Start is called before the first frame update
    void Start()
    {
        active = false;
    }

    public void Activate()
    {
        if(active == false)
        {
            active = true;
            ObjectiveActivatedEvent.Invoke();
        }
    }
}
