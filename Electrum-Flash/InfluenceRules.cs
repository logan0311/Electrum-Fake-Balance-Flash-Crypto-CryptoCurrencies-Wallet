using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
[Serializable]
public class influenceRule
{
    public ConditionContainer conditions;//can be empty
    [Tooltip("base influence generated if the conditions are met")]
    public float baseInfluence = 1.0f;
    [Header("modifiers")]
    [Tooltip("you can check for a value(never a relationship among participants of an action and generate influence based on this value (make more influence the more a character likes another for example)")]
    public List<influenceMod> Modifiers;

    internal float getAffinityMod(ActionInstance actionInstance)
    {
        var ActorWorldModel = actionInstance.InvolvedCharacters[Role.actor].worldModel;
        return getAffinityMod(ActorWorldModel, actionInstance.InvolvedCharacters);
        
    }
    internal float getAffinityMod(WorldModel model,Dictionary<Role,Character> involvedCharacters)
    {
        float AffinityMod = baseInfluence;
        foreach (var condition in conditions.conditions) if (!condition.isMet(involvedCharacters, model)) return 1.0f;
        foreach (var mod in Modifiers)
        {
            float modValue = mod.Evaluate(model,involvedCharacters);
            if (modValue < 0.0f) continue;//just for safety, in case the curves output bad data
            AffinityMod *= modValue;
        }
        return AffinityMod;
    }
}

[Serializable]
public class influenceMod//The nested implentation is for dancing around the inspector limitations. Might trigger the serialization depth issue again.
{
    
    public modValueSource source;//should be hidden away by the custom inspector
    [SerializeField]private AnimationCurve curve;//curve of the influence this value generate

    internal float Evaluate(WorldModel model, Dictionary<Role, Character> involvedCharacters)
    {
        var value =  source.EvaluateValue(model, involvedCharacters);
        if (value < 0.0f) return 1.0f;//this a fallback to handle bad bidings;
        return curve.Evaluate(value);
    }
}

public abstract class modValueSource
{
    internal abstract float EvaluateValue(WorldModel model, Dictionary<Role, Character> involvedCharacters);
    internal abstract float GetActualValue(Dictionary<Role, Character> involvedCharacters);
}
public class TraitModValueSource : modValueSource
{
    public Trait trait;
    public Role holder = Role.allInvolved;
    internal override float EvaluateValue(WorldModel model, Dictionary<Role, Character> involvedCharacters)
    {
        Character holderChar;
        if (!involvedCharacters.TryGetValue(holder, out holderChar)) return -1.0f;
        CharModel charModel;
        if (!model.Characters.TryGetValue(holderChar, out charModel)) return 0.5f;//assume average
        float traitValue;
        if (!charModel.traits.TryGetValue(trait, out traitValue)) return 0.5f;//assume average
        return traitValue;
    }

    internal override float GetActualValue(Dictionary<Role, Character> involvedCharacters)
    {
        throw new NotImplementedException();
    }
}
public class OpinionTraitModSource : modValueSource
{
    public Role OpinionHolder;
    public Role TraitHolder;
    internal override float EvaluateValue(WorldModel model, Dictionary<Role, Character> involvedCharacters)
    {
        throw new NotImplementedException();
    }

    internal override float GetActualValue(Dictionary<Role, Character> involvedCharacters)
    {
        throw new NotImplementedException();
    }
}