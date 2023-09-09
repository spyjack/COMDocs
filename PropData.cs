using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropData : MonoBehaviour
{
    [SerializeField]
    private PropCategories propCat;
    /// <summary>
    /// Returns the Prop Category of the object. Read-Only.
    /// </summary>
    public PropCategories Category
    { get { return propCat; } }

    [SerializeField]
    private string propFilters = "";
    /// <summary>
    /// Returns the Prop Filters as a string. Read-Only.
    /// </summary>
    public string Filters
    { get { return propFilters; } }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
