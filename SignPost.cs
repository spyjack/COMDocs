using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SignPost : MonoBehaviour
{
    [SerializeField] Animator signAnimator;
    [SerializeField] int leanRange;
    [SerializeField] LevelExitArea[] exitAreas = new LevelExitArea[2];
    [SerializeField] TextMeshProUGUI levelAreaLabel;
    [SerializeField] TextMeshProUGUI levelReinforcementTimeLabel;
    [Header("Left Path UI")]
    [SerializeField] TextMeshProUGUI levelNameLabel_L;
    [SerializeField] TextMeshProUGUI levelSignLabel_L;
    [SerializeField] TextMeshProUGUI levelEnemyCountLabel_L;
    [SerializeField] TextMeshProUGUI levelEnemyAggressionLabel_L;
    [SerializeField] TextMeshProUGUI levelTravelTimeLabel_L;
    [Header("Right Path UI")]
    [SerializeField] TextMeshProUGUI levelNameLabel_R;
    [SerializeField] TextMeshProUGUI levelSignLabel_R;
    [SerializeField] TextMeshProUGUI levelEnemyCountLabel_R;
    [SerializeField] TextMeshProUGUI levelEnemyAggressionLabel_R;
    [SerializeField] TextMeshProUGUI levelTravelTimeLabel_R;
    bool reassignNeeded = true;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(GetAveragePlayerPosition().x > transform.position.x + leanRange)
        {
            signAnimator.SetInteger("Direction", 1);
        }else if(GetAveragePlayerPosition().x < transform.position.x - leanRange)
        {
            signAnimator.SetInteger("Direction", -1);
        }else
        {
            signAnimator.SetInteger("Direction", 0);
        }

        if(PlayerManager.Main.PlayerCount > 0 && Vector3.Distance(PlayerManager.Main.GetPlayer(0).transform.position, this.transform.position) > 20f)
        {
            reassignNeeded = true;
        }else if(reassignNeeded)
        {
            reassignNeeded = false;
            AssignExitAreas();
            SetLabels();
        }
    }

    public void AssignExitAreas()
    {
        ProgressionManager.ProgressionInstance.GetGenerationDefinition(out GenerationDefinition _GA, out GenerationDefinition _GB);
        exitAreas[0].genDef = _GA;
        exitAreas[1].genDef = _GB;

        ProgressionManager.ProgressionInstance.GetLevelDifficultyData(out LevelDifficultyData _DA, out LevelDifficultyData _DB);
        exitAreas[0].difDef = _DA;
        exitAreas[1].difDef = _DB;
    }

    Vector3 GetAveragePlayerPosition()
    {
        Vector3 averagePosition = Vector3.zero;
        for (int i = 0; i < PlayerManager.Main.PlayerCount; i++)
        {
            averagePosition += PlayerManager.Main.GetPlayer(i).transform.position;
        }
        //print(averagePosition/PlayerManager.Main.PlayerCount);
        return averagePosition / PlayerManager.Main.PlayerCount;
    }

    void SetLabels()
    {
        levelAreaLabel.text = ProgressionManager.ProgressionInstance.WorldTimeline[ProgressionManager.ProgressionInstance.CurrentProgress.y].areaName;

        levelReinforcementTimeLabel.text = Mathf.Max(0, ProgressionManager.ProgressionInstance.ReinforcementTime).ToString() + " Hours";

        levelSignLabel_L.text = exitAreas[0].genDef.Theme;
        levelSignLabel_R.text = exitAreas[1].genDef.Theme;

        levelNameLabel_L.text = ProgressionManager.ProgressionInstance.WorldTimeline[ProgressionManager.ProgressionInstance.CurrentProgress.y].areaTimeline[ProgressionManager.ProgressionInstance.CurrentProgress.x].sectionName + " " + exitAreas[0].genDef.Theme;
        levelNameLabel_R.text = ProgressionManager.ProgressionInstance.WorldTimeline[ProgressionManager.ProgressionInstance.CurrentProgress.y].areaTimeline[ProgressionManager.ProgressionInstance.CurrentProgress.x].sectionName + " " + exitAreas[1].genDef.Theme;

        SetEnemyCountText(exitAreas[0].difDef.GetEnemyCount(), levelEnemyCountLabel_L);
        SetEnemyCountText(exitAreas[1].difDef.GetEnemyCount(), levelEnemyCountLabel_R);

        SetEnemyAggressionText(exitAreas[0].difDef.enemyAgression, levelEnemyAggressionLabel_L);
        SetEnemyAggressionText(exitAreas[1].difDef.enemyAgression, levelEnemyAggressionLabel_R);

        levelTravelTimeLabel_L.text = exitAreas[0].difDef.travelTime.ToString() + " Hours.";
        levelTravelTimeLabel_R.text = exitAreas[1].difDef.travelTime.ToString() + " Hours.";
    }

    public void UpdateReinforcementLabels()
    {
        levelReinforcementTimeLabel.text = Mathf.Max(0, ProgressionManager.ProgressionInstance.ReinforcementTime).ToString() + " Hours";
    }

    void SetEnemyCountText(int _count, TextMeshProUGUI _label)
    {
        if(_count <= 0)
        {
            _label.text = "None";
            _label.color = Color.gray;
        }else if(_count <= 10)
        {
            _label.text = "Tiny";
            _label.color = Color.green;
        }else if(_count <= 20)
        {
            _label.text = "Small";
            _label.color = Color.green;
        }else if(_count <= 50)
        {
            _label.text = "Moderate";
            _label.color = Color.yellow;
        }else if(_count <= 100)
        {
            _label.text = "Heavy";
            _label.color = Color.red;
        }else if(_count > 100)
        {
            _label.text = "Invasion Forces";
            _label.color = Color.red;
        }else
        {
            _label.text = "Unkown";
            _label.color = Color.gray;
        }
    }

    void SetEnemyAggressionText(BehaviorType _behavior, TextMeshProUGUI _label)
    {
        if(_behavior == BehaviorType.Hunt)
        {
            _label.text = "Hunting";
            _label.color = Color.red;
        }else
        {
            _label.text = "Unsuspecting";
            _label.color = Color.green;
        }
    }
}
