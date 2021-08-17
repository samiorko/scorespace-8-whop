using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public float Score
    {
        get => m_leftScore + m_rightScore;
        set
        {
            var diff = value - Score;

            if (PlayerOnLeftSide)
            {
                m_leftScore += diff;
            }
            else
            {
                m_rightScore += diff;
            }
        }
    }

    public UnityEvent OnGameLost = new UnityEvent();

    public int FinalScore => (int) (Score - 20f);

    public bool PlayerOnLeftSide => m_player.localPosition.x < m_observedLevel * Mathf.Abs(m_edgeMarker.localPosition.x);
    public bool PlayerOnLeftSideAbsolute => m_player.localPosition.x < 0f;

    public static GameManager Instance { get; private set; }
    public List<PlatformControl> Platforms { get; } = new List<PlatformControl>();

    [Header("General")]
    [Tooltip("Game active")]
    public bool m_active = false;

    [Tooltip("Development mode. Instead of losing, spawn on the topmost generated platform.")]
    public bool m_developerMode;

    [Range(1f, 2f)]
    public float m_globalTimeScale;

    [Tooltip("How much under the camera level player must fall to be considered lost")]
    public float m_deathLimit;

    [Tooltip("Split position")]
    [Range(-1, 1)]
    public float m_level = 0;

    public float m_levelInterpolationTime;
    public AnimationCurve m_levelInterpolationCurve;

    [Tooltip("Speed multiplier. Use this to control speed during runtime")]
    [Range(-2, 4)]
    public float m_speedMultiplier;

    [Tooltip("Speed multiplier gets multiplied by this every second")]
    public float m_speedMultiplierOverTime;

    public float m_maxSpeed;

    public int m_movingPlatformStartLayer;

    [Header("Camera movement")]
    [Tooltip("Base speed for camera ascension. Gets multiplied by multiplier")]
    public float m_baseAscendSpeed;

    [Tooltip("How long in seconds the falling down when losing should take")]
    public float m_loseFallDownDuration;

    public AnimationCurve m_loseFallDownCurve;

    [Header("Platform generation")]
    [Tooltip("Seed for left side. Should be fairly small, ~1. No integers!")]
    public float m_leftSeed;

    [Tooltip("Seed for right side. Should be fairly small, ~1. No integers!")]
    public float m_rightSeed;

    [Tooltip("Vertical space between platforms")]
    public float m_platformLayerSpacing;

    public int m_minPlatformWidth;
    public int m_maxPlatformWidth;

    [Tooltip("Platform width distribution curve")]
    public AnimationCurve m_platformWidthCurve;

    [Tooltip("Multiplier used to pick sample point from noise for layers. Probably best kept under 50.")]
    public float m_sampleSpacing;

    [Tooltip("How far ahead to generate platforms")]
    public float m_platformBuffer;


    [Tooltip("How far from screen center can the furthest platforms be")]
    public float m_maxPlatformDistance;

    public int m_levelTextFrequency;

    [Header("Music")] 
    public float m_musicFadeInTime;
    public AnimationCurve m_musicFadeInCurve;

    [Header("References")]
    [Tooltip("Player root transform")]
    public Transform m_player;

    public Transform m_leftCamera;
    public Transform m_rightCamera;
    public CameraMaskController m_cameraMaskController;

    public Transform m_leftContainer;
    public Transform m_rightContainer;
    public Transform m_edgeMarker;

    public Transform m_startMenuLeft;
    public Transform m_startMenuRight;

    public AudioSource m_musicSource;

    [Header("Prefabs")]
    public GameObject m_platformPrefab;

    public GameObject m_levelTextPrefab;

    public GameObject m_scrollingTextPrefab;

    public bool ControlsEnabled { get; private set; }


    private Vector3 m_playerStartingPosition;
    private readonly HashSet<int> m_populatedLayers = new HashSet<int>();
    private float m_height;
    private Transform m_leftPlatformParent;
    private Transform m_rightPlatformParent;
    private readonly HashSet<GameObject> m_props = new HashSet<GameObject>();
    private float m_observedLevel;
    private float m_previousLevel;
    private bool m_onMenu = true;
    private bool m_firstMenu = true;
    public int HighestLevel { get; set; }
    public int PreviousAddedScore { get; set; }
    public int PreviousAddedLevel { get; set; }

    [Header("Scores")]
    [SerializeField]
    private float m_leftScore = 10f;
    [SerializeField]
    private float m_rightScore = 10f;

    [SerializeField]
    private float m_leftScoreMultiplier;

    [SerializeField]
    private float m_rightScoreMultiplier;

    private Vector3 m_leftCameraInitialPosition;
    private List<(int levels, int score)> m_highScores = new List<(int, int)>();

    public IEnumerable<(int levels, int score)> HighScores => m_highScores.OrderByDescending(x => x.score).ThenByDescending(x => x.levels).Take(10);
    
    private void Awake()
    {
        Instance = this;

        m_leftPlatformParent = new GameObject("Platforms").transform;
        m_leftPlatformParent.SetParent(m_leftContainer);
        m_leftPlatformParent.localPosition = Vector3.zero;

        m_rightPlatformParent = new GameObject("Platforms").transform;
        m_rightPlatformParent.SetParent(m_rightContainer);
        m_rightPlatformParent.localPosition = Vector3.zero;
    }

    private void Start()
    {
        m_observedLevel = m_level;
        m_playerStartingPosition = m_player.localPosition;
        m_leftCameraInitialPosition = m_leftCamera.localPosition;

        RestoreHighScores();
        ScoreboardManager.Instance.UpdateScores();
    }

    private void Update()
    {
        m_cameraMaskController.Level = m_observedLevel;

        if (!m_active)
        {
            if (!m_onMenu) return;

            if (Input.GetButtonUp("Jump") && !ScoreboardManager.Instance.Open && m_startMenuLeft.gameObject.activeSelf)
            {
                StartCoroutine(nameof(FadeMusicIn));

                m_onMenu = false;
                m_active = true;
                ControlsEnabled = true;
            }

            if (Input.GetKeyDown(KeyCode.H))
            {
                if (ScoreboardManager.Instance.Open)
                {
                    ScoreboardManager.Instance.SetVisible(false);
                    m_startMenuLeft.gameObject.SetActive(true);
                    m_startMenuRight.gameObject.SetActive(true);
                }
                else
                {
                    ScoreboardManager.Instance.UpdateScores();
                    ScoreboardManager.Instance.SetVisible(true);

                    m_startMenuLeft.gameObject.SetActive(false);
                    m_startMenuRight.gameObject.SetActive(false);
                }
            }

            return;
        };

        Time.timeScale = m_globalTimeScale;

        var levelChanged = Mathf.Approximately(m_previousLevel, m_level);
        m_previousLevel = m_level;

        if (levelChanged)
        {
            StopCoroutine(nameof(MoveLevelCoroutine));
            StartCoroutine(nameof(MoveLevelCoroutine));
        }

        m_leftScoreMultiplier = 1f - m_leftScore / Score;
        m_rightScoreMultiplier = 1f - m_rightScore / Score;

        m_speedMultiplier = Mathf.Clamp(m_speedMultiplier +  m_speedMultiplierOverTime * Time.deltaTime, 0f, m_maxSpeed);
        m_height += m_baseAscendSpeed * m_speedMultiplier * Time.deltaTime;
        m_leftCamera.transform.localPosition = m_leftCameraInitialPosition + (Vector3.up * m_height);

        SpawnPlatforms();

        if (m_player.localPosition.y < m_leftCamera.localPosition.y - m_deathLimit || Mathf.Abs(m_observedLevel) > .99f)
        {
            LoseGame();
        }
    }

    private void SaveHighScores()
    {
        var topScores = HighScores.ToList();
        for (var i = 0; i < 10; i++)
        {
            var scores = topScores.Count > i ? topScores[i] as (int levels, int score)? : null;

            var score = scores?.score ?? -1;
            var levels = scores?.levels ?? -1;

            PlayerPrefs.SetInt($"score-{i}", score);
            PlayerPrefs.SetInt($"levels-{i}", levels);
        }
    }

    private void RestoreHighScores()
    {
        var scores = new List<(int levels, int score)>();
        for (var i = 0; i < 10; i++)
        {
            var score = PlayerPrefs.HasKey($"score-{i}") ? PlayerPrefs.GetInt($"score-{i}") : -1;
            var levels = PlayerPrefs.HasKey($"levels-{i}") ? PlayerPrefs.GetInt($"levels-{i}") : -1;

            if (score != -1)
            {
                scores.Add((levels, score));
            }
        }

        m_highScores = scores.OrderByDescending(x => x.score).ThenByDescending(x => x.levels).ToList();
    }

    private IEnumerator FadeMusicIn()
    {
        float time = 0f, t;

        m_musicSource.Play();

        do
        {
            time += Time.deltaTime;
            t = time / m_musicFadeInTime;
            m_musicSource.volume = m_musicFadeInCurve.Evaluate(t);
            yield return null;
        } while (t < 1f);
    }

    private IEnumerator MoveLevelCoroutine()
    {
        var time = 0f;

        var startLevel = m_observedLevel;
        var targetLevel = m_level;

        float t;

        do
        {
            time += Time.deltaTime;
            t = time / m_levelInterpolationTime;
            t = m_levelInterpolationCurve.Evaluate(t);
            m_observedLevel = Mathf.Lerp(startLevel, targetLevel, t);
            yield return null;
        } while (t < 1f);
    }

    public void AddScore(float score)
    {
        score *= PlayerOnLeftSide ? m_leftScoreMultiplier * 2 : m_rightScoreMultiplier * 2;
        score *= m_speedMultiplier * (m_height > 10f ? 1f + m_height / 50 : 1f);
        Score += score;

        var obj = Instantiate(m_scrollingTextPrefab, m_leftPlatformParent);
        obj.transform.localPosition = m_player.transform.localPosition + (Vector3.up * .5f);
        obj.GetComponent<ScrollingScore>().SetScore((int) score);

        var obj2 = Instantiate(m_scrollingTextPrefab, m_rightPlatformParent);
        obj2.transform.localPosition = m_player.transform.localPosition + (Vector3.up * .5f);
        obj2.GetComponent<ScrollingScore>().SetScore((int)score);
    }

    public void NudgeLevel(float amount)
    {
        m_level = Mathf.Clamp(m_level + amount, -1f, 1f);
    }

    private void ResetGame()
    {
        foreach (Transform transform in m_leftPlatformParent)
        {
            Destroy(transform.gameObject);
        }

        foreach (Transform transform in m_rightPlatformParent)
        {
            Destroy(transform.gameObject);
        }

        Platforms.Clear();
        m_props.Clear();

        m_populatedLayers.Clear();
        m_height = 0f;
        m_level = 0f;
        m_observedLevel = 0f;
        m_leftScore = 10f;
        m_rightScore = 10f;
        m_player.transform.position = m_playerStartingPosition;
        m_speedMultiplier = 1f;
        m_onMenu = true;
        HighestLevel = 0;

        var rb = m_player.GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.velocity = Vector3.zero;

    }

    private void LoseGame()
    {
        if (m_developerMode)
        {
            m_level = 0f;
            m_observedLevel = 0f;

            m_player.transform.localPosition = Platforms.Any()
                ? Platforms.Last().transform.localPosition + Vector3.up
                : m_playerStartingPosition;
            m_player.GetComponent<Rigidbody>().velocity = Vector3.zero;

            return;
        }

        ControlsEnabled = false;
        m_active = false;

        PreviousAddedScore = FinalScore;
        PreviousAddedLevel = HighestLevel;

        if (
            FinalScore > 0 &&
            HighScores.Count() < 10
                || HighScores.Any() 
                && (HighScores.Min(x => x.score) < FinalScore)
        ) { 
            m_highScores.Add((HighestLevel, FinalScore));
            SaveHighScores();
        }

        OnGameLost.Invoke();

        StartCoroutine(nameof(LoseGameCoroutine));
    }

    private IEnumerator LoseGameCoroutine()
    {
        var rightFollow = m_rightCamera.gameObject.GetComponent<MirrorLocal>();
        rightFollow.enabled = false;
        var startLevel = m_observedLevel;


        var cameraStart = m_leftCamera.transform.localPosition;
        var cameraTarget = m_leftCameraInitialPosition;

        foreach (var platform in Platforms)
        {
            platform.ColliderEnabled = false;
        }

        float time = 0, t;

        do
        {
            time += Time.deltaTime;
            t = time / m_loseFallDownDuration;

            var curvedT = Mathf.Clamp01(m_loseFallDownCurve.Evaluate(t));
            var newPos = Vector3.Lerp(cameraStart, cameraTarget, curvedT);
            m_observedLevel = Mathf.Lerp(startLevel, 0f, curvedT);

            m_leftCamera.transform.localPosition = newPos;
            m_rightCamera.transform.localPosition = newPos;


            m_musicSource.volume = 1f - m_musicFadeInCurve.Evaluate(t);

            yield return null;
        } while (t < 1f);

        m_player.GetComponent<Rigidbody>().isKinematic = true;
        m_musicSource.Stop();

        rightFollow.enabled = true;
        ResetGame();
    }

    private void SpawnPlatforms()
    {
        var startLayer = (int) Mathf.Max(1, Mathf.Floor(m_height / m_platformLayerSpacing));

        var layerCount = m_platformBuffer / m_platformLayerSpacing;

        for (var i = 0; i < layerCount; i++)
        {
            var layer = startLayer + i;

            if (m_populatedLayers.Contains(layer)) continue;
            m_populatedLayers.Add(layer);

            var layerOrigin = new Vector3(0f, layer * m_platformLayerSpacing, 0);
            var layerVector = new Vector3(0f, 0, 0);
            var optimalVector = new Vector3(m_maxPlatformDistance, 0, 0);

            var leftVector = -optimalVector - layerVector;
            var rightVector = optimalVector - layerVector;

            SpawnPlatform(layer, layerOrigin, leftVector, m_leftSeed, "Left", true);
            SpawnPlatform(layer, layerOrigin, rightVector, m_rightSeed, "Right", false);  

            if (layer % m_levelTextFrequency == 0)
            {
                SpawnLevelTexts(layer, layerOrigin);
            }
        }
    }

    private void SpawnLevelTexts(int layer, Vector3 origin)
    {
        void Spawn(Vector3 pos, LevelText.TextSide side, string text, Transform parent)
        {
            var obj = Instantiate(m_levelTextPrefab, parent);
            var levelText = obj.GetComponent<LevelText>();
            levelText.Text = text;
            levelText.Side = side;
            obj.transform.localPosition = pos;
            m_props.Add(obj);
        }

        var leftPos = origin + Vector3.left * m_maxPlatformDistance;
        var rightPos = origin + Vector3.right * m_maxPlatformDistance;

        Spawn(leftPos, LevelText.TextSide.Left, $"Level {layer}", m_leftPlatformParent);
        Spawn(leftPos, LevelText.TextSide.Left, $"Level {layer}", m_rightPlatformParent);

        Spawn(rightPos, LevelText.TextSide.Right, $"Score {FinalScore}", m_leftPlatformParent);
        Spawn(rightPos, LevelText.TextSide.Right, $"Score {FinalScore}", m_rightPlatformParent);
    }

    private void SpawnPlatform(int layer, Vector3 origin, Vector3 directionVector, float seed, string prefix, bool left)
    {
        var sample = GetMapSample(layer, seed);
        var slotVector = directionVector * sample;

        var width = GetPlatformWidth(layer, seed);

        var plat = Instantiate(m_platformPrefab, m_leftPlatformParent);
        plat.name = $"{prefix} {layer}";
        plat.transform.localPosition = origin + slotVector;
        var obj = plat.GetComponent<PlatformControl>();
        obj.Width = width;
        obj.Left = true;
        obj.Level = layer;

        Platforms.Add(obj);

        var plat2 = Instantiate(m_platformPrefab, m_rightPlatformParent);
        plat2.name = $"{prefix} {layer}";
        plat2.transform.localPosition = origin + slotVector;

        var obj2 = plat2.GetComponent<PlatformControl>();
        obj2.Width = width;
        obj2.Left = false;
        obj2.Level = layer;
        var mirror = plat2.gameObject.AddComponent<MirrorLocal>();
        mirror.m_parent = plat.transform;

        if (layer >= m_movingPlatformStartLayer)
        {
            MovingPlatform.AddTo(plat, layer - (m_movingPlatformStartLayer - 1));
        }
    }

    private float GetMapSample(int layer, float seed)
    {
        var y = layer * m_sampleSpacing * seed;

        var perlinSample = Mathf.PerlinNoise(seed, y);
        var sample = Mathf.Clamp01(perlinSample);

        return sample;
    }

    private int GetPlatformWidth(int layer, float seed)
    {
        var y = layer * m_sampleSpacing * seed;

        var perlinSample = Mathf.PerlinNoise(.15f, seed + y);
        var sample = Mathf.Clamp01(perlinSample);

        var width = (sample * (m_maxPlatformWidth)) + m_minPlatformWidth;

        return (int) width;
    }

    private void SetPlatformMultipliers()
    {
        var distanceFromCenter = Mathf.Abs(m_level);

        var platformMultiplier = 1 - m_platformWidthCurve.Evaluate(distanceFromCenter);

        foreach (var platform in Platforms)
        {
            platform.Multiplier = platformMultiplier;
        }
    }
}
