using RotaryHeart.Lib.SerializableDictionary;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//default reference for the serialized dictionary need to be setup in advance

[Serializable]
public class TraitValueDictionary : SerializableDictionaryBase<Trait, float> { }
[Serializable]
public class RelationshipArray//this class only serve to go around a quirk of unity serialization interaction with the serialized dictionaries
{
    public List<RelationshipType> relationships = new List<RelationshipType>();
}
[Serializable]
public class RelationShipDictionary : SerializableDictionaryBase<Character, RelationshipArray> { }

[CreateAssetMenu(fileName = "new character", menuName = "Electrum/Character")]
public class Character : ScriptableObject
{
    //trait values must be between 0f and 1f
    public TraitValueDictionary m_traits;
    public RelationShipDictionary m_relationships;
    public List<Goal> m_goals =new List<Goal>();
    public WorldModel worldModel;


    public ActionInstance ChooseAction()
    {
        var finalActions = new List<ActionInstance>();
        foreach(var action in Electrum.actionSet.actions)
        {
            float BaseAffinity = 1.0f;
            //evaluate the affinity score of this action, discard all potential instances if below affinity treshold (defined in the engine settings object)
            foreach(var affinityRule in action.affinitiesRules)
            {
                if (!m_traits.ContainsKey(affinityRule.Key))continue;
                BaseAffinity *= affinityRule.Value.Evaluate(m_traits[affinityRule.Key]);//get the affinity modifier associated with this trait value.
            }
            if (BaseAffinity < Electrum.affinityTreshold)
            {
                Debug.Log("Action " + action.name + " discarded with affinity of: " + BaseAffinity);
                continue;
            }
            //get candidate actionInstance with the different possible role bindings
            List<ActionInstance> ActionCandidates = FindControlledBindings(action);
            if (ActionCandidates == null) continue;//the action has been aborted;
            for (int i = 0; i < ActionCandidates.Count; )
            {
                ActionInstance instance = ActionCandidates[i];
                if (!EstimateBindingsQuality(instance, worldModel))//evaluate utility
                {
                    Debug.Log("instance removed with affinity " + instance.Affinity);
                    ActionCandidates.Remove(instance);//bindings are expected to be too bad to consider taking the action 

                }
                else
                {
                    
                    i++;
                }
            }
            finalActions.AddRange(ActionCandidates);
        }
        
        finalActions.Sort((x, y) => x.ExpectedImmediateUtility.CompareTo(y.ExpectedImmediateUtility));
        foreach (var instance in finalActions)
        {
            string description = instance.Template.name + ": \n ";
            foreach(var character in instance.InvolvedCharacters)
            {
                description += character.Value.name + " as " + character.Key.ToString() + "\n";
            }
            Debug.Log(description);
        }
        if (finalActions.Count == 0)
        {
            var doNothingCharacters = new Dictionary<Role, Character>();
            doNothingCharacters.Add(Role.actor, this);
            var doNothingAction = new ActionInstance(Electrum.DoNothingAction,doNothingCharacters );
            Debug.Log(name + " decided to take no action.");
            return doNothingAction;
        }
        return finalActions[0];//probably want a bit more impredictablility here. But it's better for testing purposes.
    }
    private bool EstimateBindingsQuality(ActionInstance instance, WorldModel context)//calculate expected affinity from the instance, as well as expected utility, recursion would happen here to evaluate future actions
    {
        var OpenCandidateList = new List<CharModel>(context.Characters.Values);
        for (int i = 0; i < OpenCandidateList.Count;)
        {
            if (instance.InvolvedCharacters.ContainsValue(OpenCandidateList[i].Character)) OpenCandidateList.RemoveAt(i);
            else i++;
        }
        instance.RunCharacterControlledPreferenceRules();
        var possibleBindingsinstances = RecursiveBindings(instance, OpenCandidateList, instance.Template.EngineControlledRoles);
        SetBindingsProbability(possibleBindingsinstances);

        instance.Affinity = EvaluateBindingsAffinity(possibleBindingsinstances);
        if (instance.Affinity < Electrum.affinityTreshold) return false;//the action will be discarded, as it is too unattractive
                                                                        //utility estimation
        instance.ExpectedImmediateUtility = EvaluateBindingsUtility(context, possibleBindingsinstances);
        return true;
    }
    private void SetBindingsProbability(List<ActionInstance> possibleBindingsinstances)
    {
        float likelyhoodWeightCumulated = 0.0f;
        var likelyhoodWeights = new List<float>();
        for (int i = 0; i < possibleBindingsinstances.Count; i++)
        {
            likelyhoodWeights.Add(possibleBindingsinstances[i].EvaluateLikelyhoodWeight());
            likelyhoodWeightCumulated += likelyhoodWeights[i];
            possibleBindingsinstances[i].RunEngineControlledPreferenceRules();
        }

        for (int i = 0; i < possibleBindingsinstances.Count; i++)
        {
            possibleBindingsinstances[i].expectedProbability = (likelyhoodWeights[i] / likelyhoodWeightCumulated);

        }
    }
    private float EvaluateBindingsUtility(WorldModel context, List<ActionInstance> possibleBindingsinstances)
    {
        float total = 0.0f;
        for (int i = 0; i < possibleBindingsinstances.Count; i++)
        {
            ActionInstance binding = possibleBindingsinstances[i];
            var newModel = binding.VirtualRun(context);
            float Utility = 0.0f;
            foreach(var goal in m_goals)
            {
                Utility += goal.getProgress(context, newModel)*goal.importance;
            }
            total += Utility;
        }
        return total;
    }
    private float EvaluateBindingsAffinity(List<ActionInstance> possibleBindingsinstances)
    {
        float Attractiveness = 1.0f;
        for (int i = 0; i < possibleBindingsinstances.Count; i++)
        {
            ActionInstance binding = possibleBindingsinstances[i];
            Attractiveness *= Mathf.Pow(binding.Affinity, binding.expectedProbability);
        }
        return Attractiveness;
    }
    private List<ActionInstance> FindControlledBindings(Action action)//Looks for all valid bindings that are under the character control, Proably a major memory hog on some "domains" and also slow
    {
        var unboundInstance = new ActionInstance(action,new Dictionary<Role, Character>());
        unboundInstance.InvolvedCharacters.Add(Role.actor, this);//binds the acting character
        var OpenCandidateList = new List<CharModel>(worldModel.Characters.Values);
        OpenCandidateList.Remove(worldModel.Characters[this]);
        //This is one of the point where we could do manual memory management, this is going to make a LOT of garbage
        return RecursiveBindings(unboundInstance ,OpenCandidateList, action.ActorControlledRoles);
    }
    internal List<ActionInstance> RecursiveBindings(ActionInstance instanceBase, List<CharModel> openCandidateList, List<RoleBinding> RoleSet, int depth=0)//the ActionInstance passed at the base of the recursion should only have the actor role bound
    {
        List<ActionInstance> boundInstances = new List<ActionInstance>();
        if (RoleSet.Count == 0 || depth == RoleSet.Count) return boundInstances;
        foreach(var character in openCandidateList)
        {
            ActionInstance RecursionInstance = new ActionInstance(instanceBase);
            RecursionInstance.InvolvedCharacters.Add(RoleSet[depth].role, character.Character);
            bool isValidCandidate = true;
            foreach(var condition in RoleSet[depth].conditions.conditions)
            {
                if(!condition.isMet(RecursionInstance.InvolvedCharacters,worldModel))//character did not meet a condition for this role
                {
                    isValidCandidate = false;
                    break;
                }
            }
            if (isValidCandidate)
            {
                var recursionOpenlist = new List<CharModel>(openCandidateList);
                recursionOpenlist.Remove(character);
                if (depth == RoleSet.Count - 1)//this is a completely bound instance, add it to the list to be returned
                {
                    //RecursionInstance.RunControlledPreferenceRules();
                    boundInstances.Add(RecursionInstance);
                }
                var additionalBindings = RecursiveBindings(RecursionInstance, recursionOpenlist, RoleSet, depth + 1);
                if (additionalBindings == null) return null;//action has been aborted
                boundInstances.AddRange(additionalBindings);//this will contain finished bindings only
            }
        }
        if (boundInstances.Count > Electrum.MaxBindingCandidatesAbort)
        {
            Debug.LogError("Error : " + name + " found more possible bindings combinations than the " + Electrum.MaxBindingCandidatesAbort + " limit on the " + instanceBase.Template.name + " action. Action discarded.");
            
            return null;

        }
        if (!instanceBase.Template.ActorControlledRoles[depth].Mandatory && depth != instanceBase.Template.ActorControlledRoles.Count -1)//creates an extra branch with current depth's role unassigned if it is not mandatory
        {
            var additionalBindings = RecursiveBindings(instanceBase, openCandidateList, RoleSet, depth + 1);
            if (additionalBindings == null) return null;//action is aborted
            boundInstances.AddRange(additionalBindings);
        }
        return boundInstances;//should contain all completely bound possible instances of the action. If empty it means no set of characters fullfilled all conditions
    }
    internal void ConstructownModel()//should only be called once per character, as it keeps references to the real values;
    {
        var model = new CharModel();
        model.Character = this;
        model.goals = m_goals;
        model.Relationships = m_relationships;
        model.traits = m_traits;
        if (worldModel.Characters.ContainsKey(this)) worldModel.Characters[this] = model;
        else worldModel.Characters.Add(this, model);
    }
}