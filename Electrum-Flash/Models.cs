using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using RotaryHeart.Lib.SerializableDictionary;

[Serializable]
public class CharModelDictionary : SerializableDictionaryBase<Character, CharModel> { }
[Serializable]
public class WorldModel
{
    //public readonly List<CharModel> Characters;
    public CharModelDictionary Characters;
    private WorldModel(CharModelDictionary characters)//this constructor will also copy every character model
    {
        Characters = new CharModelDictionary();//this should work because charmodel is a value type
        foreach (var character in characters.Keys)
        {
            Characters.Add(character, characters[character].copy());
        }
    }
    //utility to navigate and log info about this list or get a specific collection of information
    public WorldModel Copy()
    {
        var result = new WorldModel(Characters);
        return result;
    }

}
[Serializable]
public class CharModel//model that the character have of each other
{
    /*These models are much simpler than what we initially intended (information source, and trustworthiness is not being tracked), but it will do for Ensemble level of performance*/
    public Character Character;
    public TraitValueDictionary traits = new TraitValueDictionary();

    public List<Goal> goals = new List<Goal>();
    public RelationShipDictionary Relationships = new RelationShipDictionary();
    public OpinionDictionary opinions = new OpinionDictionary();

    internal CharModel copy()
    {
        var result = new CharModel();
        result.Character = Character;
        result.traits = new TraitValueDictionary();
        result.traits.CopyFrom(traits);
        result.goals = new List<Goal>(goals);
        result.Relationships = new RelationShipDictionary();
        result.Relationships.CopyFrom(Relationships);
        result.opinions = new OpinionDictionary();
        result.opinions.CopyFrom(opinions);
        return result;
    }
    public CharModel() { }
    public CharModel(Character character)
    {
        Character = character;
        
    }
}

[Serializable]
public class OpinionModel
{
    public TraitValueDictionary traits = new TraitValueDictionary();
    public List<Goal> goals = new List<Goal>();//This one will probably be updated through Action effects.
    public RelationShipDictionary relationships = new RelationShipDictionary();
} 
[Serializable]
public class OpinionDictionary : SerializableDictionaryBase<Character, OpinionModel> { }