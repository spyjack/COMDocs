using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropLoader : MonoBehaviour
{
    private static PropLoader propLoaderInstance;
    public static PropLoader Instance { get { return propLoaderInstance; } }

    //Array of CATEGORIES
    [SerializeField]
    private PropCategoryStruct[] propsArray;
    /// <summary>
    /// Returns the array of Prop Category Structs. Read-Only.
    /// </summary>
    public PropCategoryStruct[] PropsStructArray
    {
        get { return propsArray; }
    }

    private void Awake()
    {
        //Do singleton stuff
        if (propLoaderInstance != null && propLoaderInstance != this)
        {
            Destroy(this.gameObject);
            print("Destroying self");
        }
        else
        {
            propLoaderInstance = this;
        }
        DontDestroyOnLoad(propLoaderInstance);

        //Load Resources
        InitPropsArray();
        LoadAllProps();
    }
    
    /// <summary>
    /// Initializes the Props Array and sets all categories.
    /// </summary>
    private void InitPropsArray()
    {
        int catLength = System.Enum.GetValues(typeof(PropCategories)).Length;
        propsArray = new PropCategoryStruct[catLength];

        for (int i = 0; i < catLength; i++)
        {
            //Converts the enum to an array, then gets the value at I, then casts it into PropCategories.
            propsArray[i].category = (PropCategories)System.Enum.GetValues(typeof(PropCategories)).GetValue(i);
            propsArray[i].props = new List<PropData>();
        }
    }

    /// <summary>
    /// Returns an array of PropDatas loaded from the assigned directory.
    /// </summary>
    /// <param name="dir">The directory to load from. Case sensitive.</param>
    /// <returns></returns>
    List<PropData> LoadPropsFromDirectory(string dir)
    {
        //Create the list that will be added to. Returned at the end.
        List<PropData> _propsList = new List<PropData>();

        //Load all the assets into a generic array.
        Object[] loadedAssets = Resources.LoadAll(dir, typeof(GameObject));

        //Iterate through them all, casted into objects then grab the PropData component.
        foreach (var asset in loadedAssets)
        {
            GameObject _prefab = (GameObject)asset;
            _propsList.Add(_prefab.GetComponent<PropData>());
        }

        return _propsList;
    }

    /// <summary>
    /// Loads and sorts all props stored in the categorized Object/Prop_ resources folders.
    /// </summary>
    void LoadAllProps()
    {
        //The following lists could contain improper props, so we must sort them all after they have been initialized.
        //Unsorted list of all Veggies
        List<PropData> propsVegUnsorted = LoadPropsFromDirectory("Objects/Props_Vegetation");
        FastSortToCategories(propsVegUnsorted);

        //Unsorted list of all Generic props
        List<PropData> propsGenericUnsorted = LoadPropsFromDirectory("Objects/Props_Generic");
        FastSortToCategories(propsGenericUnsorted);

        //Unsorted list of all Objective props
        List<PropData> propsObjUnsorted = LoadPropsFromDirectory("Objects/Props_Objectives");
        FastSortToCategories(propsObjUnsorted);

        //Unsorted list of all prop structures
        List<PropData> propsStructuresUnsorted = LoadPropsFromDirectory("Objects/Props_Structures");
        FastSortToCategories(propsStructuresUnsorted);

    }

    /// <summary>
    /// Sorts a list of PropData using the FastCategorize function.
    /// </summary>
    /// <param name="_propsToSort">List of PropData to be sorted.</param>
    void FastSortToCategories(List<PropData> _propsToSort)
    {
        foreach (PropData prp in _propsToSort)
        {
            FastCategorize(prp);
        }
    }

    /// <summary>
    /// Quickly attempts to add a prop to the corresponding category. This will break if the order of the PropCategories enum is messed up.
    /// </summary>
    /// <param name="_prop">Prop to add to the categorized list</param>
    /// <returns>Returns false if the category doesn't exist.</returns>
    bool FastCategorize(PropData _prop)
    {
        switch (_prop.Category)
        {
            case PropCategories.Tree:
                propsArray[0].props.Add(_prop);
                break;
            case PropCategories.Bush:
                propsArray[1].props.Add(_prop);
                break;
            case PropCategories.Grass:
                propsArray[2].props.Add(_prop);
                break;
            case PropCategories.Container:
                propsArray[3].props.Add(_prop);
                break;
            case PropCategories.Generic:
                propsArray[4].props.Add(_prop);
                break;
            case PropCategories.Objective:
                propsArray[5].props.Add(_prop);
                break;
            default:
                Debug.LogWarning(_prop.gameObject.name + " has an invalid Prop Category! Did you forget to add it to the category switch?");
                return false;
        }
        return true;
    }

    /// <summary>
    /// Grab a random prop from the specified category.
    /// </summary>
    /// <param name="_category">Category to grab from.</param>
    /// <returns>Returns a PropData prefab.</returns>
    public static PropData GrabProp(PropCategories _category)
    {
        PropData _grabbedProp = null;

        //Go through each category until you have the correct one, then grab a random prop.
        foreach (PropCategoryStruct _propCat in Instance.PropsStructArray)
        {
            if(_propCat.category == _category)
            {   //If there are no props in the list, then return null.
                if(_propCat.props.Count > 0)
                {
                    _grabbedProp = _propCat.props[Random.Range(0, _propCat.props.Count)];
                }
                break;
            }
        }
        return _grabbedProp;
    }

    /// <summary>
    /// Grab a random prop from a specified category with a specific filter.
    /// </summary>
    /// <param name="_category">Category to grab from.</param>
    /// <param name="_filter">A specific string filter to check for.</param>
    /// <returns>Returns a PropData prefab.</returns>
    public static PropData GrabProp(PropCategories _category, string _filter)
    {
        PropData _grabbedProp = null;

        //Go through each category until you have the correct one, then grab a random prop.
        foreach (PropCategoryStruct _propCat in Instance.PropsStructArray)
        {
            if (_propCat.category == _category)
            {   //If there are no props in the list, then return null.
                if (_propCat.props.Count > 0)
                {
                    int _startIndex = Random.Range(0, _propCat.props.Count);
                    int _index = _startIndex;
                    do
                    {
                        if (_propCat.props[_index].Filters.Contains(_filter))
                        {
                            _grabbedProp = _propCat.props[_index];
                            break;
                        }

                        _index++;
                        //If index goes outside of the array then loop back.
                        if(_index > _propCat.props.Count-1)
                        {
                            _index = 0;
                        }
                    } while (_index != _startIndex);
                    
                }
                break;
            }
        }
        return _grabbedProp;
    }
}

/// <summary>
/// List of all the base category types a prop can be.
/// </summary>
public enum PropCategories
{
    Tree,
    Bush,
    Grass,
    Container,
    Generic,
    Objective
}

/// <summary>
/// Holds a category for filtering and a list for all PropData stored.
/// </summary>
[System.Serializable]
public struct PropCategoryStruct
{
    //Category definition
    public PropCategories category;
    //List of all stored PropData
    public List<PropData> props;

    public PropCategoryStruct(PropCategories _cat)
    {
        category = _cat;
        props = new List<PropData>();
    }
}
