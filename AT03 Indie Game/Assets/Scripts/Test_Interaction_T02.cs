using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test_Interaction_T02 : MonoBehaviour, IInteractable
{
    public Test_Interaction interaction;

    private void Start()
    {
        interaction.interactionEvent += TestMethodThree;
    }

    public void Activate()
    {
        Debug.Log("the interaction is currently turned on: " + interaction.ExampleBool);
        interaction.Activate();
    }

    private void TestMethodThree()
    {
        Debug.Log("TestMethodThree has been executed");
    } 
}
