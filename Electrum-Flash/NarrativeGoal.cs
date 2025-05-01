using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

//[CreateAssetMenu(fileName = "new NarrativeGoal", menuName = "Electrum/NarrativeGoal")]
public abstract class NarrativeGoal : ScriptableObject
{
    public enum Type
    {
        relationship,
        worldState,
        Trait,
        Action,
        opinion
    }
    Type type;//probably better off using conditions ?
    public abstract bool IsMet(WorldModel worldModel);
    public abstract int distanceToGoal(WorldModel worldModel);
}
[CreateAssetMenu(fileName = "new RelationshipGoal",menuName = "Electrum/Narrative Goal/Relationship")]
public class RelationshipNGoal : NarrativeGoal
{
    Type type = Type.relationship;
    [Header("Characters involved")]
    public Character character1;
    public Character character2;
    public override bool IsMet(WorldModel worldModel)
    {
        throw new System.NotImplementedException();
    }
    public override int distanceToGoal(WorldModel worldModel)
    {
        throw new System.NotImplementedException();
    }
   
}

