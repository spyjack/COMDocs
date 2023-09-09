using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructibleProp : PropData
{
    [SerializeField]
    int health = 1;
    [SerializeField]
    int maxHealth = 10;

    [SerializeField]
    Spawntable droptable;

    [SerializeField]
    Transform deathTransform;
    [SerializeField]
    Transform aliveTransform;

    // Start is called before the first frame update
    void Start()
    {
        aliveTransform.gameObject.SetActive(true);
        deathTransform.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Deal damage to the prop and check for destruction.
    /// </summary>
    /// <param name="_damage">The damage to deal to the prop.</param>
    public void TakeDamage(int _damage)
    {
        if(health > 0)
        {
            print("Taking damage to prop!");
            health--;
            CheckForDeath();
        }
    }

    /// <summary>
    /// Check if the object has been destroyed, if so toggle transforms.
    /// </summary>
    public void CheckForDeath()
    {
        if(health <= 0)
        {
            aliveTransform.gameObject.SetActive(false);
            if(deathTransform != null)
            {
                deathTransform.gameObject.SetActive(true);
            }

            if(droptable != null)
            {
                Transform _dropable = droptable.FetchItem();
                if(_dropable != null)
                {
                    Instantiate(_dropable, this.transform.position + new Vector3(0, 0.1f, 0), Quaternion.Euler(90,0,0), this.transform);
                }
            }
        }
    }
}
