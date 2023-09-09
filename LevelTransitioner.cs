using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelTransitioner : MonoBehaviour
{
    public LevelData level;
    [SerializeField] Image fadeOverlay;
    [SerializeField] GenerationDefinition genTest;

    Coroutine fadeRoutine;
    // Start is called before the first frame update
    void Start()
    {
        if(fadeOverlay == null)
        {
            LookForFadeOverlay();
        }
        //StartCoroutine(FadeToNewLevel(genTest));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetFadeOpacity(float _opacity)
    {
        if(fadeOverlay != null)
        {
            fadeOverlay.color = new Color(fadeOverlay.color.r, fadeOverlay.color.b, fadeOverlay.color.g, _opacity);
        }
    }

    public void CancelAllTransitions()
    {
        if(fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }
        StopAllCoroutines();
        fadeRoutine = null;
    }

    public void StartTransition()
    {
        if (fadeRoutine == null){
            Debug.LogWarning("Transition A Called");
        fadeRoutine = StartCoroutine(FadeToNewLevel(ProgressionManager.ProgressionInstance.GetGenerationDefinition(), ProgressionManager.ProgressionInstance.GetLevelDifficultyData()));
        }
    }

    public void StartTransition(GenerationDefinition _genData, DifficultyDefinition _difficultyData)
    {
        if (fadeRoutine == null){
            Debug.LogWarning("Transition B Called");
        fadeRoutine = StartCoroutine(FadeToNewLevel(_genData, _difficultyData.ToDifficultyData()));
        }
    }

    public void StartTransition(GenerationDefinition _genData, LevelDifficultyData _difficultyData)
    {
        if (fadeRoutine == null){
            Debug.LogWarning("Transition C Called");
        fadeRoutine = StartCoroutine(FadeToNewLevel(_genData, _difficultyData));
        }
    }

    public void StartTransition(Vector3 _playerPosition, int _clampIndex = 1)
    {
        if (fadeRoutine == null){
        fadeRoutine = StartCoroutine(FadeToTeleport(_playerPosition, _clampIndex));
        }
    }

    public void EndGameTransition()
    {
        if (fadeRoutine == null){
        fadeRoutine = StartCoroutine(FadeToEnd());
        }
    }

    void LookForFadeOverlay()
    {
        fadeOverlay = GameObject.FindGameObjectWithTag("FadeOverlay").GetComponent<Image>();
    }

    IEnumerator FadeToNewLevel(GenerationDefinition _genData, LevelDifficultyData _difficultyData)
    {//If there is a fade overlay, fade to black, create a new level, fade open again.
        if(fadeOverlay == null)
        {
            LookForFadeOverlay();
        }
        if(fadeOverlay != null)
        {
            //First we fade out to black,
            while (fadeOverlay.color.a < 1)
            {
                fadeOverlay.color = new Color(fadeOverlay.color.r, fadeOverlay.color.b, fadeOverlay.color.g, fadeOverlay.color.a + 0.05f);
                yield return new WaitForSeconds(0.1f);
            }

            //Then we delete the old one
            Destroy(GameObject.Find("Game Level"));

            //Reset Kill Objectives
            Objective mainObjective = ObjectiveManager.GetObjective(ObjectiveType.KillAll);
            mainObjective.SetObjectiveGoal(0);
            mainObjective.RemoveObjectivePoints(mainObjective.GetObjectiveScore());

            //Remove all the extra objectives
            ObjectiveManager.ClearExtraObjectives();

            //Increase the levels travel time.
            _difficultyData.AddTravelTime(_difficultyData.travelTime);

            //Then we create the new level
            LevelGenerator.Generator.CreateNewLevelData(_genData, _difficultyData);
            LevelInstantiator.Instantiator.generateLevel = true;

            //Then we wait until it has been generated
            while(!LevelInstantiator.Instantiator.BeginLevelInstatiation())
            {
                yield return null;
            }

            //Then we wait until it has been instantiated
            while(!LevelInstantiator.Instantiator.levelInstantiated)
            {
                yield return null;
            }
            //Start the next music track
            UIManager.UIInstance.PlayTrack(ProgressionManager.ProgressionInstance.WorldTimeline[ProgressionManager.ProgressionInstance.CurrentProgress.y].areaTimeline[ProgressionManager.ProgressionInstance.CurrentProgress.x].musicTrackIndex);

            //Increase player(s) progression through the world! - Might be better to do elsewhere?
            ProgressionManager.ProgressionInstance.Progress();

            //Teleport Players
            PlayerManager.Main.UpdateAllPlayerClamps(1);
            PlayerManager.Main.TransportPlayers(new Vector3(LevelGenerator.Generator.Level.playerSpawnpoint.x, 1, LevelGenerator.Generator.Level.playerSpawnpoint.y));

            //Then we fade back out!
            while (fadeOverlay.color.a > 0)
            {
                fadeOverlay.color = new Color(fadeOverlay.color.r, fadeOverlay.color.b, fadeOverlay.color.g, fadeOverlay.color.a - 0.05f);
                yield return new WaitForSeconds(0.1f);
            }
        }
        fadeRoutine = null;
    }

    IEnumerator FadeToTeleport(Vector3 _teleportPosition, int _clampIndex = 1)
    {//If there is a fade overlay, fade to black, create a new level, fade open again.
        if(fadeOverlay == null)
        {
            LookForFadeOverlay();
        }
        if(fadeOverlay != null)
        {
            //First we fade out to black,
            while (fadeOverlay.color.a < 1)
            {
                fadeOverlay.color = new Color(fadeOverlay.color.r, fadeOverlay.color.b, fadeOverlay.color.g, fadeOverlay.color.a + 0.05f);
                yield return new WaitForSeconds(0.1f);
            }

            //Wait until there are players
            while(PlayerManager.Main.PlayerCount <= 0)
            {
                yield return new WaitForSeconds(1f);
            }

            //Reset Kill Objectives
            Objective mainObjective = ObjectiveManager.GetObjective(ObjectiveType.KillAll);
            mainObjective.SetObjectiveGoal(0);
            mainObjective.RemoveObjectivePoints(mainObjective.GetObjectiveScore());

            //Remove all the extra objectives
            ObjectiveManager.ClearExtraObjectives();

            PlayerManager.Main.UpdateAllPlayerClamps(_clampIndex);
            //Move the player to the new position
            PlayerManager.Main.TransportPlayers(_teleportPosition);

            //Then we fade back out!
            while (fadeOverlay.color.a > 0)
            {
                fadeOverlay.color = new Color(fadeOverlay.color.r, fadeOverlay.color.b, fadeOverlay.color.g, fadeOverlay.color.a - 0.05f);
                yield return new WaitForSeconds(0.1f);
            }
        }
        fadeRoutine = null;
    }

    IEnumerator FadeToEnd()
    {
        //If there is a fade overlay, fade to black, roll credits and stuff
        if(fadeOverlay == null)
        {
            LookForFadeOverlay();
        }
        if(fadeOverlay != null)
        {
            //First we fade out to black,
            while (fadeOverlay.color.a < 1)
            {
                fadeOverlay.color = new Color(fadeOverlay.color.r, fadeOverlay.color.b, fadeOverlay.color.g, fadeOverlay.color.a + 0.05f);
                yield return new WaitForSeconds(0.1f);
            }

            //Reset Kill Objectives
            Objective mainObjective = ObjectiveManager.GetObjective(ObjectiveType.KillAll);
            mainObjective.SetObjectiveGoal(0);
            mainObjective.RemoveObjectivePoints(mainObjective.GetObjectiveScore());

            //Remove all the extra objectives
            ObjectiveManager.ClearExtraObjectives();

            //Activate end of game UI stuff.
            Debug.Log("Game has been won.");
            UIManager.UIInstance.EnableWinScreen();
            PlayerManager.Main.TransportPlayers(new Vector3(-173,0,25));
            //PlayerManager.Main.ActivateWinBanner(true);
        }
        fadeRoutine = null;
    }
}
