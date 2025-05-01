using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Electrum : MonoBehaviour
{
    public ElectrumSettings settings;
    
    public static List<Character> cast;
    public static List<NarrativeGoal> beats;

    //static members
    public static Electrum singleton;
    public static ActionSet actionSet;
    public static float affinityTreshold;
    public static Action DoNothingAction;

    public static int MaxBindingCandidatesWarning { get; internal set; }
    public static int MaxBindingCandidatesAbort { get; internal set; }

    private void Awake()
    {
        if (settings == null) Debug.LogError("please assign a setting object to Electrum before initializing it.");
        if (actionSet == null) actionSet = settings.actionSet;
        if (actionSet == null || actionSet.actions.Count == 0) Debug.LogWarning("Action set has no action or is not assigned");
        if (singleton == null)
        {
            singleton = this;
            affinityTreshold = settings.affinityTreshold;
            MaxBindingCandidatesWarning = settings.MaxBindingsCandidateWarning;
            MaxBindingCandidatesAbort = settings.MaxBindingsCandidatesAbort;
            DoNothingAction = settings.DoNothingAction;
            cast = settings.cast;
        }
        else
        {
            Debug.LogError("Two instances of Electrum currently instancied");
            Destroy(this);
        }
        foreach (var character in cast) character.ConstructownModel();
    }
    
}



public enum InfoType//may later add the goal and opinion types
{
    relationship,
    trait,
    opinion
}
public enum OpinionType
{
    trait,
    relationship
    //goal
}




