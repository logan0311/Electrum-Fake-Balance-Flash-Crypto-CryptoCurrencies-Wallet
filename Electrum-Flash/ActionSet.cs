using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="new Action set", menuName = "Electrum/Action Set")]
public class ActionSet : ScriptableObject
{
    public List<Action> actions = new List<Action>();

}
