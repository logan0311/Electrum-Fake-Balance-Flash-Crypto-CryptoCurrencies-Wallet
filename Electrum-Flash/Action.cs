using RotaryHeart.Lib.SerializableDictionary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.Timeline;

[Serializable]
public class AffinityDictionary : SerializableDictionaryBase<Trait, AnimationCurve> { }


[CreateAssetMenu(fileName ="new Action", menuName = "Electrum/Action")]
public class Action : ScriptableObject
{
    [Tooltip("traits, along with its influence on the attractivity of the action for a character who have these trait")]
    public AffinityDictionary affinitiesRules;
    [Tooltip("Mandatory role the action requires to be taken")]
    public List<RoleBinding> ActorControlledRoles;
    [Tooltip("Extra role of character that the action may involve")]
    public List<RoleBinding> EngineControlledRoles;
    [Tooltip("potential effects of the action")]
    public List<ActionEffect> effects;
    
}
public class ActionInstance
{
    public Action Template;
    public Dictionary<Role, Character> InvolvedCharacters;//I was not sure wether I should use Charater or role as key, code will be faster and clearer this way I think, it will need to be refactored to allow multiple bindings to the same role.
    public float Affinity = 1.0f;
    public float ExpectedImmediateUtility = 0.0f;
    public float ExpectedTotalUtility = 0.0f;//based on the expected utility of future actions after that one.
    public float expectedProbability = 0.0f;
    public ActionInstance(Action template, Dictionary<Role, Character> involvedCharacters)
    {
        Template = template;
        InvolvedCharacters = new Dictionary<Role, Character>(involvedCharacters);
    }
    public ActionInstance(ActionInstance original)
    {
        Template = original.Template;
        InvolvedCharacters = new Dictionary<Role, Character>(original.InvolvedCharacters);
        Affinity = original.Affinity;
    }

    /*Used to adjust the affinity of the instance based on the attractiveness of the bindings the Character has control over.*/
    internal void RunCharacterControlledPreferenceRules()
    {
        foreach (var binding in Template.ActorControlledRoles)
        {
            foreach (var rule in binding.DesirabilityRules)
            {
                Affinity *= rule.getAffinityMod(this);
            }
        }
    }

   

    internal void RunEngineControlledPreferenceRules()
    {
        foreach(var binding in Template.EngineControlledRoles)
        {
            foreach(var rule in binding.DesirabilityRules)
            {
                Affinity *= rule.getAffinityMod(this);
            }
        }
    }

    internal float EvaluateLikelyhoodWeight()
    {
        float likelyhoodWeight = 1.0f;
        foreach(var binding in Template.EngineControlledRoles)
        {
            foreach(var rule in binding.LikelyhoodRules)
            {
                likelyhoodWeight *= rule.getAffinityMod(this);
            }
        }
        return likelyhoodWeight;
    }

    internal WorldModel VirtualRun(WorldModel Model)//this should be run during the Engine controlled bindings attractiveness evaluation, to avoid having to look for these bindings twice.
    {
        var newModel = Model.Copy();
        foreach(var effect in Template.effects)
        {

            bool valid = effect.TryVirtualApply(Model,ref newModel, InvolvedCharacters);
        }
        throw new NotImplementedException();
    }
}



[Serializable]
public class ActionEffect
{
    public List<EffectInstance> effects = new List<EffectInstance>();
    public ConditionContainer conditions = new ConditionContainer();//condition that must be met for the effect to be applied, allow for more nuance with smaller authoring overhead, condition is marked unmet if it refers to an unbound role
    public List<influenceRule> influenceRules = new List<influenceRule>();

    internal bool TryVirtualApply(WorldModel model,ref WorldModel targetModel, Dictionary<Role, Character> involvedCharacters)
    {
        bool isvalid = true;
        foreach (var condition in conditions.conditions)
        {
            if (!condition.isMet(involvedCharacters, model))
            {
                isvalid = false;
                break;
            }
        }
        if (!isvalid) return false;
        float likelyhood = 1.0f;
        foreach (var influenceRule in influenceRules)
        {
            likelyhood *= influenceRule.getAffinityMod(model, involvedCharacters);
        }
        if(likelyhood < 0.0f)
        {
            Debug.LogError("effect of action was evaluated to negative likelyhood.");//this would be a certain sign that the influence rule evaluation function is broken
            return false;
        }
        //apply effect (modified by likelyhood)
        foreach(var effect in effects)
        {
            effect.VirtualApply(ref targetModel,involvedCharacters, likelyhood);
        }
        return true;
    }
}
[Serializable]
public class RoleBinding
{
    public Role role = Role.witness;
    public bool Mandatory= true;//is this role allowed to be unbound ?
    //to do witness tags, similar to the effect ones, to let character draw conclusion from witnessing actions.
    //to do latter, add possibility to optionally allow several character to be bound to the same role
    [HideInInspector]public Character holder;
    [Tooltip("take care to not reference unbound roles in these, mandatory role up the list and the actor are usually safe to reference")]
    public ConditionContainer conditions;//condition that must be fullfilled for the role to be bound, reference to the role will test the candidate state
    public List<influenceRule> DesirabilityRules;//influenceRules that let the character mesure how desirable a binding is (if the actor controls that bindings
    public List<influenceRule> LikelyhoodRules;//influence rules that determine the likelyhood for a set of bindings not controlled by the actor to be picked, should be empty for an Actor controleld bindings, as they would be the same as the desirability ones

}
public enum Role
{
    none,
    actor,
    target,
    witness,
    assistant,
    hinderer,
    allInvolved
}
public enum ValueChangeOperator
{
    add,
    multiply,
    set
}
public enum ValueComparisonOperator//removing equals might be a good call as floats are rarely equal anyways, a range can be expressed with two conditions (and we could easily have interface to author a range condition)
{
    lessThan,
    MoreThan,
    Equals
}
[Serializable]
public class EffectInstance : ISerializationCallbackReceiver // this is going to turn into a container for the actual effects
{
    public InfoType type;
    [NonSerialized]public Effect effect;
    //control for who percieves the effect directly.
    [HideInInspector]public string effectType;
    [HideInInspector]public string JsonEffect;
    [Tooltip("have all involved character see if this effect is applied.")]
    public bool AllWitnesses;
    [Tooltip("Roles that see the outcome of this effect, others will use likelyhood to alter their worldmodel")]
    public List<Role> Witnesses;

    public void OnAfterDeserialize()
    {
        if(JsonEffect == "" || effectType == null)
        {
            effect = new TraitEffect();
        }
        else
        {
            effect = (Effect)JsonUtility.FromJson(JsonEffect, Type.GetType(effectType));
        }
    }

    public void OnBeforeSerialize()
    {
        effectType = effect.GetType().ToString();
        JsonEffect = JsonUtility.ToJson(effect);
    }

    internal void VirtualApply(ref WorldModel targetModel, Dictionary<Role, Character> involvedCharacters, float likelyhood)
    {
        effect.VirtualApply(ref targetModel, involvedCharacters, likelyhood);
    }
}


[Serializable]
public abstract class Effect//all the inherited subclasses will have public fields, because of how the serializer works
{
    internal abstract void VirtualApply(ref WorldModel targetModel, Dictionary<Role, Character> involvedCharacters, float likelyhood);
    internal abstract void Apply(Dictionary<Role, Character> involvedCharacters);

}

public class TraitEffect : Effect
{
    public Role Holder;
    public Trait trait;
    public float value;
    public ValueChangeOperator Operator;

    internal override void Apply(Dictionary<Role, Character> involvedCharacters)
    {
        Character HolderChar;
        if (!involvedCharacters.TryGetValue(Holder, out HolderChar)) return;//unbound role
        float previousValue;
        if(!HolderChar.m_traits.TryGetValue(trait,out previousValue))
        {
            //the character doesn't have the trait, it defaults to 0.5
            HolderChar.m_traits.Add(trait, 0.5f);
            previousValue = 0.5f;
        }
        switch (Operator)
        {
            case ValueChangeOperator.add:
                HolderChar.m_traits[trait] = Mathf.Clamp01(previousValue + value);
                break;
            case ValueChangeOperator.multiply:
                HolderChar.m_traits[trait] = Mathf.Clamp01(previousValue * value);
                break;
            case ValueChangeOperator.set:
                HolderChar.m_traits[trait] = Mathf.Clamp01(value);
                break;
            default:
                Debug.LogError("Invalid operator " + Operator.ToString() + " in action effect ");
                throw new NotImplementedException();
        }
    }

    internal override void VirtualApply(ref WorldModel targetModel, Dictionary<Role, Character> involvedCharacters, float likelyhood)
    {
        Character HolderCharacter;
        if (!involvedCharacters.TryGetValue(Holder, out HolderCharacter)) return;//unbound role, might happen sometimes
        CharModel HolderModel;
        if(!targetModel.Characters.TryGetValue(HolderCharacter,out HolderModel))
        {
            targetModel.Characters.Add(HolderCharacter, new CharModel(HolderCharacter));
            HolderModel = targetModel.Characters[HolderCharacter];
        }
        float previousValue;
        if(!HolderModel.traits.TryGetValue(trait,out previousValue))
        {
            HolderModel.traits.Add(trait, 0.5f);
            previousValue = 0.5f;
        }
        switch (Operator)
        {
            case ValueChangeOperator.add:
                HolderModel.traits[trait] = Mathf.Clamp01(previousValue + value*likelyhood);
                break;
            case ValueChangeOperator.set:
                HolderModel.traits[trait] = Mathf.Clamp01(Mathf.Lerp(previousValue,value,likelyhood));//this one doesn't work with this simple likelyhood strategy, and this fix will likely lead to some weird edge cases
                break;
            case ValueChangeOperator.multiply:
                HolderModel.traits[trait] = Mathf.Clamp01(previousValue *  Mathf.Pow(value,likelyhood));
                break;
            default:
                break;
        }
    }
}

[Serializable]
public class RelationshipEffect : Effect
{
    public Role Holder;
    public Role Recipient;
    public RelationshipType relationship;
    public bool status;

    internal override void Apply(Dictionary<Role, Character> involvedCharacters)
    {
        Character HolderCharacter;
        if (!involvedCharacters.TryGetValue(Holder, out HolderCharacter)) return;//unbound role
        Character RecipientCharacter;
        if (!involvedCharacters.TryGetValue(Recipient, out RecipientCharacter)) return;
        RelationshipArray relationships;
        if(!HolderCharacter.m_relationships.TryGetValue(RecipientCharacter,out relationships))
        {
            relationships = new RelationshipArray();
            HolderCharacter.m_relationships.Add(RecipientCharacter, relationships);
        }
        if (status && !relationships.relationships.Contains(relationship)) relationships.relationships.Add(relationship);
        else if (!status && relationships.relationships.Contains(relationship)) relationships.relationships.Remove(relationship);
    }

    internal override void VirtualApply(ref WorldModel targetModel, Dictionary<Role, Character> involvedCharacters, float likelyhood)
    {
        Character HolderChar;
        Character RecipientChar;
        if (!involvedCharacters.TryGetValue(Holder, out HolderChar)) return;//unbound role
        if (!involvedCharacters.TryGetValue(Recipient, out RecipientChar)) return;//unbound role
        CharModel HolderModel;
        if(!targetModel.Characters.TryGetValue(HolderChar, out HolderModel))//no previous model of the holder
        {
            HolderModel = new CharModel(HolderChar);//assuming no relationship
        }
        RelationshipArray relationshipModel;
        if(!HolderModel.Relationships.TryGetValue(RecipientChar,out relationshipModel))//no known relationship with the recipient
        {
            relationshipModel = new RelationshipArray();
            HolderModel.Relationships.Add(RecipientChar, relationshipModel);

        }
        //There are only two cases I can think of at the moment
        if (status && !relationshipModel.relationships.Contains(relationship)) relationshipModel.relationships.Add(relationship);
        else if (!status && relationshipModel.relationships.Contains(relationship)) relationshipModel.relationships.Remove(relationship);
        
    }
}
[Serializable]
public abstract class OpinionEffect : Effect
{
    public Role OpinionHolder;
    public OpinionType opinionType;

}
[Serializable]
public class TraitOpinionEffect : OpinionEffect
{
    public Role TraitHolder;
    public Trait trait;
    public float traitValue;
    public ValueChangeOperator Operator;

    public TraitOpinionEffect() : base()
    {
        opinionType = OpinionType.trait;
    }

    internal override void Apply(Dictionary<Role, Character> involvedCharacters)
    {
        Character OpinionHolderCharacter;
        if (!involvedCharacters.TryGetValue(OpinionHolder, out OpinionHolderCharacter)) return;
        Character TraitHolderCharacter;
        if (!involvedCharacters.TryGetValue(TraitHolder, out TraitHolderCharacter)) return;
        CharModel TraitHolderModel;
        if(!OpinionHolderCharacter.worldModel.Characters.TryGetValue(TraitHolderCharacter,out TraitHolderModel))
        {
            TraitHolderModel = new CharModel(TraitHolderCharacter);
            OpinionHolderCharacter.worldModel.Characters.Add(TraitHolderCharacter, TraitHolderModel);
        }
        float previousValue;
        if(!TraitHolderModel.traits.TryGetValue(trait,out previousValue))
        {
            previousValue = 0.5f;//assume average
            TraitHolderModel.traits.Add(trait, previousValue);
        }
        switch (Operator)
        {
            case ValueChangeOperator.add:
                TraitHolderModel.traits[trait] = Mathf.Clamp01(previousValue + traitValue);
                break;
            case ValueChangeOperator.multiply:
                TraitHolderModel.traits[trait] = Mathf.Clamp01(previousValue * traitValue);
                break;
            case ValueChangeOperator.set:
                TraitHolderModel.traits[trait] = Mathf.Clamp01(traitValue);
                break;
            default:
                Debug.LogError("invalid Value change operator : " + Operator.ToString());
                break;
        }
    }

    internal override void VirtualApply(ref WorldModel targetModel, Dictionary<Role, Character> involvedCharacters, float likelyhood)
    {
        Character OpinionHolderChar;
        if (!involvedCharacters.TryGetValue(OpinionHolder, out OpinionHolderChar)) return;//unbound role
        Character TraitHolderChar;
        if (!involvedCharacters.TryGetValue(TraitHolder, out TraitHolderChar)) return;//unbound role
        CharModel OpinionHolderModel;
        if(!targetModel.Characters.TryGetValue(OpinionHolderChar,out OpinionHolderModel))//no previous model
        {
            OpinionHolderModel = new CharModel(OpinionHolderChar);
            targetModel.Characters.Add(OpinionHolderChar,OpinionHolderModel);
        }
        OpinionModel opinions;
        if(!OpinionHolderModel.opinions.TryGetValue(TraitHolderChar,out opinions))//no modeled opinions about traitHolder
        {
            opinions = new OpinionModel();
            OpinionHolderModel.opinions.Add(TraitHolderChar, opinions);
        }
        float previousValue;
        if (!opinions.traits.TryGetValue(trait,out previousValue))
        {
            previousValue = 0.5f;
            opinions.traits.Add(trait,previousValue);
        }
        switch (Operator)
        {
            case ValueChangeOperator.add:
                opinions.traits[trait] = Mathf.Clamp01(previousValue + traitValue);
                break;
            case ValueChangeOperator.multiply:
                opinions.traits[trait] = Mathf.Clamp01(previousValue * traitValue);
                break;
            case ValueChangeOperator.set:
                opinions.traits[trait] = Mathf.Clamp01(traitValue);
                break;
            default:
                break;
        }
    }
}
[Serializable]
public class RelationshipOpinionEffect : OpinionEffect
{
    public Role RelationshipHolder;
    public Role RelationshipRecipient;
    public RelationshipType relationship;
    public bool RelationshipStatus;

    public RelationshipOpinionEffect() : base()
    {
        opinionType = OpinionType.relationship;
    } 

    internal override void Apply(Dictionary<Role, Character> involvedCharacters)
    {
        Character OpinionHolderCharacter;
        if (!involvedCharacters.TryGetValue(OpinionHolder, out OpinionHolderCharacter)) return;
        Character RelationshipHolderChar;
        if (!involvedCharacters.TryGetValue(RelationshipHolder, out RelationshipHolderChar)) return;
        Character RelationshipRecipientChar;
        if (!involvedCharacters.TryGetValue(RelationshipRecipient,out RelationshipRecipientChar)) return;
        CharModel RelationshipHolderModel;
        if(!OpinionHolderCharacter.worldModel.Characters.TryGetValue(RelationshipHolderChar,out RelationshipHolderModel))
        {
            RelationshipHolderModel = new CharModel(RelationshipHolderChar);
            OpinionHolderCharacter.worldModel.Characters.Add(RelationshipHolderChar, RelationshipHolderModel);
        }
        RelationshipArray relationshipArray;
        if (!RelationshipHolderModel.Relationships.TryGetValue(RelationshipRecipientChar, out relationshipArray))
        {
            relationshipArray = new RelationshipArray();
            RelationshipHolderModel.Relationships.Add(RelationshipRecipientChar, relationshipArray);
        }
        if (RelationshipStatus && !relationshipArray.relationships.Contains(relationship)) relationshipArray.relationships.Add(relationship);
        else if (!RelationshipStatus && relationshipArray.relationships.Contains((relationship))) relationshipArray.relationships.Remove(relationship);
    }

    internal override void VirtualApply(ref WorldModel targetModel, Dictionary<Role, Character> involvedCharacters, float likelyhood)
    {
        Character OpinionHolderChar;
        if (!involvedCharacters.TryGetValue(OpinionHolder, out OpinionHolderChar)) return;//unbound role
        Character RelationshipHolderChar;
        if (!involvedCharacters.TryGetValue(RelationshipHolder, out RelationshipHolderChar)) return;
        Character RelationshipRecipientChar;
        if (!involvedCharacters.TryGetValue(RelationshipRecipient, out RelationshipRecipientChar)) return;
        CharModel OpinionHolderModel;
        if(!targetModel.Characters.TryGetValue(OpinionHolderChar,out OpinionHolderModel))//no previous model
        {
            OpinionHolderModel = new CharModel(OpinionHolderChar);
            targetModel.Characters.Add(OpinionHolderChar, OpinionHolderModel);
        }
        OpinionModel opinions;
        if(!OpinionHolderModel.opinions.TryGetValue(RelationshipHolderChar,out opinions))
        {
            opinions = new OpinionModel();
            OpinionHolderModel.opinions.Add(RelationshipHolderChar, opinions);
        }
        RelationshipArray relationships;
        if(!opinions.relationships.TryGetValue(RelationshipRecipientChar,out relationships))//no previous relationship modeled for the recipient
        {
            relationships = new RelationshipArray();
        }
        if (RelationshipStatus && !relationships.relationships.Contains(relationship)) relationships.relationships.Add(relationship);
        else if (!RelationshipStatus && relationships.relationships.Contains(relationship)) relationships.relationships.Remove(relationship);
    }
}
