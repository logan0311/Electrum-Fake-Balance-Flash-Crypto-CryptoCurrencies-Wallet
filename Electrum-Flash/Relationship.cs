using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//relationship represent a character "mode of interaction" towards another, they don't strictly tie to the characters opinions of each other (a character may act friendly to someone they don't like for various reason) they don't value either
//Later on having relationship that are more intensive versions of others might be good, but for now it's easily expressed with conditions
public enum RelationshipType
{
  self,
  aquaintance,
  friend,
  goodFriend,
  ally,
  lover,
  rival,
  enemy,
  parent,
  child,
  sibling,
  liege,
  vassal
}
 
