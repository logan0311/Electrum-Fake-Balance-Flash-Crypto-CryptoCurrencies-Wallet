using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEditor;
using System.Runtime.Serialization;
using UnityEditor.AI;
using UnityEditor.Experimental.TerrainAPI;


public enum ConditionType
{
    trait,
    relationship,
    opinion
    //goal
}
[Serializable]
public abstract class NewCondition
{
    [SerializeField]public  ConditionType _type;
    //there will always be a holder in the current conditions, but it might not always be the case so it will be reimplemented in each inherited class.
    internal abstract bool isMet(Dictionary<Role,Character> involvedCharacters, WorldModel worldModel);
    internal abstract bool isCurrentlyMet(Dictionary<Role,Character> involvedCharacters);//check if the condition is "really" met
    internal abstract float getDistance(WorldModel worldModel, Dictionary<Role,Character> involvedCharacters);//negative results indicate the condition is fullfilled;
    internal abstract float getCurrentDistance(Dictionary<Role,Character> involvedCharacters);

    //public abstract void OnGUI(Rect position, TraitCondition condition);
}

[Serializable]
public class TraitCondition : NewCondition
{
    [SerializeField]public Trait _trait;
    [SerializeField]public Role _holder;
    [SerializeField]public float _value;
    [SerializeField]public ValueComparisonOperator _operator;
    [SerializeField]public float tolerance;//for the equal operator only, could have used another layer of inheritance too
    public TraitCondition() : base()
    {
        _type = ConditionType.trait;
    }
    internal override float getCurrentDistance(Dictionary<Role,Character> involvedCharacters)
    {
        Character holderRef = involvedCharacters[_holder];
        if (!involvedCharacters.TryGetValue(_holder, out holderRef))
        {
            Debug.Log("evaluated a condition with unassigned role");
            return 5000.0f;//role wasn't assigned to a character, probably a sign that something is amiss
        }
        float traitValue;
        if (!holderRef.m_traits.TryGetValue(_trait, out traitValue)) return 5000.0f;//character does not have the specified trait, this is a potential expected behavior(we could populate unspecified trait values with default ones
        switch (_operator)
        {
            case ValueComparisonOperator.lessThan:
                return traitValue - _value;
            case ValueComparisonOperator.MoreThan:
                return _value - traitValue;
            case ValueComparisonOperator.Equals:
                return Mathf.Abs(_value - traitValue);
            default:
                Debug.LogError("Invalid comparaison operator used in distance estimation to Trait condition. Trait: " + _operator.ToString());
                throw new NotImplementedException(); //using this exception because we don't need much extra info.
        }
    }

    internal override float getDistance(WorldModel worldModel, Dictionary<Role,Character> involvedCharacters)
    {
        CharModel holderModel;
        float traitValue;
        if (!worldModel.Characters.TryGetValue(involvedCharacters[_holder], out holderModel)) traitValue = 0.5f;
        else if (!holderModel.traits.TryGetValue(_trait, out traitValue)) traitValue = 0.5f;
        switch (_operator)
        {
            case ValueComparisonOperator.lessThan:
                return traitValue - _value;
            case ValueComparisonOperator.MoreThan:
                return _value - traitValue;
            case ValueComparisonOperator.Equals:
                return Mathf.Abs(_value - traitValue);
            default:
                Debug.LogError("Invalid comparaison operator used in distance estimation to Trait condition. Trait: " + _operator.ToString());
                throw new NotImplementedException(); //using this exception because we don't need much extra info.
        }
    }

    internal override bool isCurrentlyMet(Dictionary<Role,Character> involvedCharacters)
    {
        Character holderRef = involvedCharacters[_holder];
        if (!involvedCharacters.TryGetValue(_holder, out holderRef))
        {
            Debug.Log("evaluated a condition with unassigned role");
            return false;//role wasn't assigned to a character, probably a sign that something is amiss
        }
        float traitValue;
        if (!holderRef.m_traits.TryGetValue(_trait, out traitValue)) return false;//character does not have the specified trait, this is a potential expected behavior(we could populate unspecified trait values with default ones
        switch (_operator)
        {
            case ValueComparisonOperator.lessThan:
                return (traitValue < _value);
            case ValueComparisonOperator.MoreThan:
                return traitValue > _value;
            case ValueComparisonOperator.Equals:
                return (traitValue <_value + tolerance) && (traitValue > _value - tolerance);
            default:
                Debug.LogError("Invalid comparaison operator used in distance estimation to Trait condition. Trait: " + _operator.ToString());
                throw new NotImplementedException(); //using this exception because we don't need much extra info.
        }
    }

    internal override bool isMet(Dictionary<Role,Character> involvedCharacters, WorldModel worldModel)
    {
        CharModel holderModel;
        float traitValue;
        if (!worldModel.Characters.TryGetValue(involvedCharacters[_holder], out holderModel)) traitValue = 0.5f;
        else if (!holderModel.traits.TryGetValue(_trait, out traitValue)) traitValue = 0.5f;
        switch (_operator)
        {
            case ValueComparisonOperator.lessThan:
                return traitValue < _value;
            case ValueComparisonOperator.MoreThan:
                return _value < traitValue;
            case ValueComparisonOperator.Equals:
                return Mathf.Abs(_value - traitValue) <  tolerance;
            default:
                Debug.LogError("Invalid comparaison operator used in distance estimation to Trait condition. Trait: " + _operator.ToString());
                throw new NotImplementedException(); //using this exception because we don't need much extra info.
        }
    }
    
}
[Serializable]
public class RelationshipCondition : NewCondition
{
    [SerializeField] public RelationshipType relationship;
    [SerializeField] public Role holder;
    [SerializeField] public Role recipient;
    [SerializeField] public bool relationshipStatus = false;

    public RelationshipCondition() : base()
    {
        _type = ConditionType.relationship;
    }
    internal override float getCurrentDistance(Dictionary<Role, Character> involvedCharacters)
    {
        throw new NotImplementedException();//only way I see to compute this is to find a trigger rule that results in this relationship (which is sort of equivalent to an opinion condition)
    }

    internal override float getDistance(WorldModel worldModel, Dictionary<Role, Character> involvedCharacters)
    {
        throw new NotImplementedException();
    }

    internal override bool isCurrentlyMet(Dictionary<Role, Character> involvedCharacters)
    {
        throw new NotImplementedException();
    }

    internal override bool isMet(Dictionary<Role, Character> involvedCharacters, WorldModel worldModel)
    {
        throw new NotImplementedException();
    }
}

[Serializable]
public abstract class OpinionCondition : NewCondition
{
    [SerializeField]public Role holder;
    [SerializeField]public OpinionType opinionType;
    public OpinionCondition()
    {
        _type = ConditionType.opinion;
    }
    
}
[Serializable]
public class TraitOpinionCondition : OpinionCondition
{
    public Trait trait;
    public Role TraitHolder;
    public float value;
    public ValueComparisonOperator OpinionOperator;
    public float tolerance;//only used for the equal operator

    public TraitOpinionCondition() : base()
    {
        opinionType = OpinionType.trait;
    }
    internal override float getCurrentDistance(Dictionary<Role, Character> involvedCharacters)
    {
        Character OpinionHolderChar;
        if (!involvedCharacters.TryGetValue(holder, out OpinionHolderChar))
        {
            Debug.LogWarning("trying to evaluate a condition about an unbound role.");//could happen on non mandatory roles
            return 5000.0f;
        }
        Character TraitHolderChar;
        CharModel TraitholderModel;
        if(!involvedCharacters.TryGetValue(TraitHolder, out TraitHolderChar))
        {
            Debug.LogWarning("Trying to evaluate a condition about an unbound role.");//could also happen
            return 5000.0f;
        }
        float traitValue;
        if(!OpinionHolderChar.worldModel.Characters.TryGetValue(TraitHolderChar, out TraitholderModel))
        {
            //opinion holder isn't aware of this character. Assume average value
            traitValue = 0.5f;
        }
        else
        {
            if (!TraitholderModel.traits.TryGetValue(trait, out traitValue)) traitValue = 0.5f;//assume average value if no particular opinion is held.
        }
        switch (OpinionOperator)
        {
            case ValueComparisonOperator.lessThan:
                return traitValue - value;
            case ValueComparisonOperator.MoreThan:
                return value - traitValue;
            case ValueComparisonOperator.Equals:
                return Mathf.Abs(value - traitValue);
            default:
                Debug.LogError("invalid operator on traitOpinionCondition: " + OpinionOperator.ToString());
                throw new NotImplementedException();
        }
    }

    internal override float getDistance(WorldModel worldModel, Dictionary<Role, Character> involvedCharacters)
    {
        Character holderChar;//holder of the opinion
        if (!involvedCharacters.TryGetValue(holder, out holderChar))
        {
            Debug.LogWarning("Trying to evaluate an opinion of an unbound role");
            return 5000.0f;//the role is unassigned, couldn't find the opinion holder, not sure if it should result in arbitrary distance or 0
        }
        CharModel charModel;
        float traitValue;
        if (!worldModel.Characters.TryGetValue(holderChar, out charModel)) traitValue = 0.5f;//the character doesn't have a model of the other character
        else
        {
            Character TraitHolderChar;//subject of the opinion
            if (!involvedCharacters.TryGetValue(TraitHolder, out TraitHolderChar))
            {
                traitValue = 0.5f;//Opinion targeted an unbound Role, possibly a sign that something is wrong.
                Debug.LogWarning("Trying to access a character's opinion of an unbound role");
            }
            OpinionModel opinion;
            if (!charModel.opinions.TryGetValue(TraitHolderChar, out opinion)) traitValue = 0.5f;//the opinion holder has no opinions about the target.
            else if (!opinion.traits.TryGetValue(trait, out traitValue)) traitValue = 0.5f;//the opinion holder doesn't have an opinion on this specific trait
            
        }
        switch (OpinionOperator)
        {
            case ValueComparisonOperator.lessThan:
                return traitValue - value;
            case ValueComparisonOperator.MoreThan:
                return value - traitValue;
            case ValueComparisonOperator.Equals:
                return Mathf.Abs(traitValue - value);
            default:
                Debug.LogError("Unimplemented opinion operator :" + OpinionOperator.ToString());
                throw new NotImplementedException();
        }
    }

    internal override bool isCurrentlyMet(Dictionary<Role, Character> involvedCharacters)
    {
        Character OpinionHolderChar;
        if (!involvedCharacters.TryGetValue(holder, out OpinionHolderChar))
        {
            Debug.LogWarning("trying to evaluate a condition about an unbound role.");//could happen on non mandatory roles
            return false;
        }
        Character TraitHolderChar;
        CharModel TraitholderModel;
        if (!involvedCharacters.TryGetValue(TraitHolder, out TraitHolderChar))
        {
            Debug.LogWarning("Trying to evaluate a condition about an unbound role.");//could also happen
            return false;
        }
        float traitValue;
        if (!OpinionHolderChar.worldModel.Characters.TryGetValue(TraitHolderChar, out TraitholderModel))
        {
            //opinion holder isn't aware of this character. Assume average value
            traitValue = 0.5f;
        }
        else
        {
            if (!TraitholderModel.traits.TryGetValue(trait, out traitValue)) traitValue = 0.5f;//assume average value if no particular opinion is held.
        }
        switch (OpinionOperator)
        {
            case ValueComparisonOperator.lessThan:
                return traitValue < value;
            case ValueComparisonOperator.MoreThan:
                return value > traitValue;
            case ValueComparisonOperator.Equals:
                return traitValue < value+tolerance && traitValue > value - tolerance;
            default:
                Debug.LogError("invalid operator on traitOpinionCondition: " + OpinionOperator.ToString());
                throw new NotImplementedException();
        }
    }

    internal override bool isMet(Dictionary<Role, Character> involvedCharacters, WorldModel worldModel)
    {
        Character holderChar;//holder of the opinion
        if (!involvedCharacters.TryGetValue(holder, out holderChar))
        {
            Debug.LogWarning("Trying to evaluate an opinion of an unbound role");
            return false;//the role is unassigned, couldn't find the opinion holder, not sure if it should result in arbitrary distance or 0
        }
        CharModel charModel;
        float traitValue;
        if (!worldModel.Characters.TryGetValue(holderChar, out charModel)) traitValue = 0.5f;//the character doesn't have a model of the other character, assume average
        else
        {
            Character TraitHolderChar;//subject of the opinion
            if (!involvedCharacters.TryGetValue(TraitHolder, out TraitHolderChar))
            {
                traitValue = 0.5f;//Opinion targeted an unbound Role, possibly a sign that something is wrong.
                Debug.LogWarning("Trying to access a character's opinion of an unbound role");
            }
            OpinionModel opinion;
            if (!charModel.opinions.TryGetValue(TraitHolderChar, out opinion)) traitValue = 0.5f;//the opinion holder has no opinions about the target.
            else if (!opinion.traits.TryGetValue(trait, out traitValue)) traitValue = 0.5f;//the opinion holder doesn't have an opinion on this specific trait

        }
        switch (OpinionOperator)
        {
            case ValueComparisonOperator.lessThan:
                return traitValue < value;
            case ValueComparisonOperator.MoreThan:
                return value < traitValue;
            case ValueComparisonOperator.Equals:
                return traitValue < value + tolerance && traitValue > value - tolerance;
            default:
                Debug.LogError("Unimplemented opinion operator :" + OpinionOperator.ToString());
                throw new NotImplementedException();
        }
    }
}
[Serializable]
public class RelationshipOpinionCondition : OpinionCondition //similar to the relationship condition, it's currently impossible to evaluate distances to relationship without some kind a rule triggering them
{
    [SerializeField] public RelationshipType relationship;
    [SerializeField] public Role RelationshipHolder;
    [SerializeField] public Role RelationShipRecipient;
    [SerializeField] public bool RelationshipStatus;

    public RelationshipOpinionCondition()
    {
        opinionType = OpinionType.relationship;
    }
    internal override float getCurrentDistance(Dictionary<Role, Character> involvedCharacters)
    {
        throw new NotImplementedException();
    }

    internal override float getDistance(WorldModel worldModel, Dictionary<Role, Character> involvedCharacters)
    {
        throw new NotImplementedException();
    }

    internal override bool isCurrentlyMet(Dictionary<Role, Character> involvedCharacters)
    {
        Character opinionHolderChar;
        if (!involvedCharacters.TryGetValue(holder, out opinionHolderChar))
        {
            Debug.LogWarning("trying to evaluate a relationshipOpinion condition about an unbound role");
            return false;
        }
        Character OpinionRelationshipHolder;
        if(!involvedCharacters.TryGetValue(RelationshipHolder, out OpinionRelationshipHolder))
        {
            Debug.LogWarning("Unbound role in relationship opinion condition : " + RelationshipHolder.ToString());
            return false;
        }
        Character OpinionRelationshipRecipient;
        if(!involvedCharacters.TryGetValue(RelationShipRecipient, out OpinionRelationshipRecipient))
        {
            Debug.LogWarning("Unbound role in relationship opinion condition : " + RelationShipRecipient.ToString());
            return false;
        }
        CharModel RelationshipHolderModel;
        if(!opinionHolderChar.worldModel.Characters.TryGetValue(OpinionRelationshipHolder,out RelationshipHolderModel)) return false;
        RelationshipArray relationshipArray;
        if (!RelationshipHolderModel.Relationships.TryGetValue(OpinionRelationshipRecipient, out relationshipArray)) return false;
        bool Status = relationshipArray.relationships.Contains(relationship);
        /*Having relationship be boolean status turned out to be a bad idea I think. I'll refactor it if it seems like it will save a lot of time(It likely will)*/
        return RelationshipStatus == Status;
    }

    internal override bool isMet(Dictionary<Role, Character> involvedCharacters, WorldModel worldModel)
    {
        Character OpinionHolderChar;
        CharModel OpinionHolderModel;
        if (!involvedCharacters.TryGetValue(holder, out OpinionHolderChar)) return false;//probably a sign that something is wrong
        if (!worldModel.Characters.TryGetValue(OpinionHolderChar, out OpinionHolderModel)) return false;
        Character RelationHolderChar;
        OpinionModel opinion;
        if (!involvedCharacters.TryGetValue(RelationshipHolder, out RelationHolderChar)) return false; //unbound role, might be bad
        if (!OpinionHolderModel.opinions.TryGetValue(RelationHolderChar, out opinion)) return false; //no opinion found
        RelationshipArray relationArray;
        if (!opinion.relationships.TryGetValue(RelationHolderChar, out relationArray)) return false;// no known relationship opinion, again, assuming absence of relationship as the default;
        return relationArray.relationships.Contains(relationship) == RelationshipStatus;
    }
}
//[Serializable]
//public class GoalCondition : NewCondition // this may need to be excluded for now, there are no good short hand to represent goals, probably too much work when you can infer goals from other informations most of the time
//{
//    [SerializeField] public Goal goalModel;
//    [SerializeField] public Role holder;
//    internal override float getCurrentDistance(Dictionary<Role, Character> involvedCharacters)
//    {
//        throw new NotImplementedException();
//    }
//
//    internal override float getDistance(WorldModel worldModel, Dictionary<Role, Character> involvedCharacters)
//    {
//        throw new NotImplementedException();
//    }
//
//    internal override bool isCurrentlyMet(Dictionary<Role, Character> involvedCharacters)
//    {
//        throw new NotImplementedException();
//    }
//
//    internal override bool isMet(Dictionary<Role, Character> involvedCharacters, WorldModel worldModel)
//    {
//        throw new NotImplementedException();
//    }
//}

[Serializable]
public class ConditionContainer : ISerializationCallbackReceiver
{
    [NonSerialized]public List<NewCondition> conditions = new List<NewCondition>();//hopefully the serialisation of this doesn't interfere(it did)
    [HideInInspector]public List<string> conditionData = new List<string>();
    [HideInInspector]public List<string> conditionTypes = new List<string>();
    

    public void OnAfterDeserialize()
    {
        conditions.Clear();//just to be sure there is no junk left in the list.
        for(int i = 0; i < conditionData.Count; i++)
        {
            conditions.Add((NewCondition)JsonUtility.FromJson(conditionData[i], Type.GetType(conditionTypes[i])));
        }
        
    }

    public void OnBeforeSerialize()
    {
        //could add some more error checking here to avoid loosing data if something goes wrong
        conditionData = new List<string>();
        conditionTypes = new List<string>();
        foreach(var condition in conditions)
        {
            conditionTypes.Add(condition.GetType().ToString());
            conditionData.Add(JsonUtility.ToJson(condition, false));
        }
        
    }
}



