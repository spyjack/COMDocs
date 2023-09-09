using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelExitArea : MonoBehaviour
{
    public ExitAreaType exitType;
    [SerializeField] float exitTime;
    int playersInExitArea = 0;
    public GenerationDefinition genDef = null;
    public LevelDifficultyData difDef = null;
    float exitTimer;
    // Start is called before the first frame update
    void Start()
    {
        exitTimer = exitTime;
        UIManager.UIInstance.TravelText.text = "";
    }

    // Update is called once per frame
    void Update()
    {
        if(exitTimer <= 0 && playersInExitArea >= PlayerManager.Main.PlayerCount && PlayerManager.Main.PlayerCount > 0)
        {
            if(ProgressionManager.ProgressionInstance.HasWon()) //Player has won the game so we do the fade out
            {
                LevelGenerator.Generator.transitioner.EndGameTransition();
                
            }else if(exitType == ExitAreaType.encounter)
            {
                AssignDefinitions();

                LevelGenerator.Generator.transitioner.StartTransition(genDef, difDef);

                genDef = null;
                difDef = null;

            }else if(exitType == ExitAreaType.splitPath)
            {

                LevelGenerator.Generator.transitioner.StartTransition(new Vector3(-84,1,50), 2);
                if (ProgressionManager.ProgressionInstance.CurrentProgress.x == 0)
                {
                    FindObjectOfType<Portcullis>().SetPortcullisAnimator();
                }

            }
            
            playersInExitArea = 0;
        }else if(playersInExitArea > 0)
        {
            if(playersInExitArea >= PlayerManager.Main.PlayerCount)
            {
                exitTimer -= Time.deltaTime;
                if(exitTimer > exitTime * 0.75f)
                {
                    UIManager.UIInstance.TravelText.text = playersInExitArea + "/" + PlayerManager.Main.PlayerCount + "Players At Exit";
                }else
                {
                    UIManager.UIInstance.TravelText.text = "Exiting in... " + Mathf.Round(exitTimer);
                }
            }else
            {
                UIManager.UIInstance.TravelText.text = playersInExitArea + "/" + PlayerManager.Main.PlayerCount + "Players At Exit";
            }
        }
    }

    void AssignDefinitions()
    {
        if(genDef == null)
        {
            genDef = ProgressionManager.ProgressionInstance.GetGenerationDefinition();
        }

        if(difDef == null)
        {
            difDef = ProgressionManager.ProgressionInstance.GetLevelDifficultyData();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            if(ObjectiveManager.CheckForLevelCleared() && other.GetComponent<PlayerScript>().isAlive)
            {
                exitTimer = exitTime;
                playersInExitArea++;
                UIManager.UIInstance.TravelText.text = "0/1 Players At Exit";
                other.GetComponent<PlayerScript>().DisableObjectivePointer();
            }else
            {
                UIManager.UIInstance.TravelText.text = "You must kill all enemies before you leave...";
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            if(ObjectiveManager.CheckForLevelCleared() && other.GetComponent<PlayerScript>().isAlive)
            {
                exitTimer = exitTime;
                playersInExitArea--;
                playersInExitArea = Mathf.Max(0, playersInExitArea);
            }
            UIManager.UIInstance.TravelText.text = "";
        }
    }
}

public enum ExitAreaType
{
    encounter,
    camp,
    splitPath
}
