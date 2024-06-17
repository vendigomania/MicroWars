using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MicroWarsGame : MonoBehaviour
{
    [SerializeField] private GameObject startScreen;

    [SerializeField] private GameObject playScreen;
    [SerializeField] private TMP_Text scoreLable;

    [SerializeField] private Animator endScreen;
    [SerializeField] private TMP_Text endTitleLable;
    [SerializeField] private ResultRow winner;
    [SerializeField] private ResultRow loser;
    [SerializeField] private TMP_Text nextActionLable;

    [Header("Ingame")]
    [SerializeField] private Transform levelParent;
    private GameObject[] levels;

    private GameObject curLevel;

    private PlanetVisualizator[] planets;

    private int Score
    {
        get => PlayerPrefs.GetInt("Score", 0);
        set => PlayerPrefs.SetInt("Score", value);
    }

    private int Level
    {
        get => PlayerPrefs.GetInt("Level", 0);
        set => PlayerPrefs.SetInt("Level", value);
    }

    bool settingsOpen = false;

    // Start is called before the first frame update
    void Start()
    {
        Screen.orientation = ScreenOrientation.Portrait;

        levels = Resources.LoadAll<GameObject>("/");
        PlanetVisualizator.OnPlanetTypeChanged.AddListener(CheckResult);
    }


    bool clickDown = false;
    void Update()
    {
        if (!playScreen.activeSelf || settingsOpen || endTitleLable.gameObject.activeInHierarchy) return;

        PlanetCycle();

        CheckAction();

        EnemyCycle();
    }

    public void StartGame()
    {
        startScreen.SetActive(false);
        playScreen.SetActive(true);

        if (curLevel != null) Destroy(curLevel);

        curLevel = Instantiate(levels[Level % levels.Length], levelParent);
        planets = curLevel.GetComponentsInChildren<PlanetVisualizator>();
        scoreLable.text = Score.ToString();

        BubbleVisualizator.timeScale = 1f;
    }

    public void SetSettings(bool value)
    {
        settingsOpen = value;
        BubbleVisualizator.timeScale = value ? 0f : 1f;
    }

    private void CheckResult()
    {
        bool playerAlive = false;
        bool enemyAlive = false;

        foreach (var planet in planets)
        {
            if (planet.Type == 1) playerAlive = true;
            else if(planet.Type > 1) enemyAlive = true;
        }

        if(playerAlive && !enemyAlive) EndGame(true);
        else if(enemyAlive && !playerAlive) EndGame(false);
    }

    private void EndGame(bool isWin)
    {
        BubbleVisualizator.timeScale = 0f;

        endScreen.Play("Show");

        endTitleLable.text = isWin ? "VICTORY!" : "DEFEAT!";

        int count = 0;
        foreach (var planet in planets) if (planet.Type != -1) count += planet.Value;

        winner.SetValue(isWin? 0 : 1, count);
        loser.SetValue(isWin ? 1 : 0, 0);

        nextActionLable.text = isWin ? "NEXT LEVEL" : "RETRY";

        Level += isWin? 1 : 0;
        Score = isWin? Score + count : 0;

        if (isWin) SoundManager.Instance.PlayWin();
        else SoundManager.Instance.PlayLose();
    }

    float cooldown = 1f;
    private void PlanetCycle()
    {
        if(cooldown > 0f)
        {
            cooldown -= Time.deltaTime;

            if(cooldown < 0f)
            {
                foreach (var planet in planets) planet.UpdateInvoke();
                cooldown = 1f;
            }
        }
    }

    [SerializeField] private BubbleVisualizator bubblePrefab;
    private void CheckAction()
    {
#if UNITY_EDITOR
        if (Input.GetMouseButton(0))
#else
        if (Input.touchCount > 0)
#endif
        {
            clickDown = true;
        }
        else if (clickDown)
        {
            if (PlanetVisualizator.selected.Count > 0)
            {
                foreach(var pln in PlanetVisualizator.selected)
                {
                    if(pln.Type != 1)
                        foreach (var pln2 in PlanetVisualizator.selected)
                        {
                            if(pln != pln2)
                            {
                                pln2.SendPoints(pln, bubblePrefab);
                            }
                        }
                }

                PlanetVisualizator.selected.Clear();
            }

            clickDown = false;
        }
    }

    float enemyCooldown = 1f;
    private void EnemyCycle()
    {
        if (enemyCooldown > 0f)
        {
            enemyCooldown -= Time.deltaTime;

            if(enemyCooldown < 0f)
            {
                foreach(var planet in planets)
                {
                    if (planet.Type > 1)
                    {
                        int random = Random.Range(0, planets.Length);
                        while (random < planets.Length * 2)
                        {
                            if (planets[random % planets.Length].Type != planet.Type)
                            {
                                planet.SendPoints(planets[random % planets.Length], bubblePrefab);
                                random = planets.Length * 2;
                            }
                            random++;
                        }
                    }
                }

                enemyCooldown = Random.Range(6f, 12f);
            }
        }
    }
}
