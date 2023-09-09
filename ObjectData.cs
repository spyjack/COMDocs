using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectData : MonoBehaviour
{
    [SerializeField]
    ObjectType objectToReplace = ObjectType.tree;
    public ObjectType Object
    {
        get{return objectToReplace;}
    }
}
