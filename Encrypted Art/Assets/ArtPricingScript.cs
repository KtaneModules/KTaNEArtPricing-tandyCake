using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using KeepCoding;

public class ArtPricingScript : MonoBehaviour {

    public KMBombInfo Bomb;
    public KMAudio Audio;
    public KMBombModule Module;

    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;

    void Awake()
    {
        moduleId = moduleIdCounter++;
        
        /*
        foreach (KMSelectable selectable in keypad) 
        {
            selectable.OnInteract += delegate () { keypadPress(selectable); return false; };
        }
        */
        //button.OnInteract += delegate () { buttonPress(); return false; };

    }

    void Start()
    {

    }

    void Update() 
    {

    }

    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Use <!{0}> to do something.";
    #pragma warning restore 414

    IEnumerator ProcessTwitchCommand (string command)
    {
        throw new BeatingBlanException();
        command = command.Trim().ToUpperInvariant();
        List<string> parameters = command.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        yield return null;
    }

    IEnumerator TwitchHandleForcedSolve ()
    {
        yield return null;
    }
}
