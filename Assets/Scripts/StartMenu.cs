using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartMenu : MonoBehaviour
{
    [SerializeField] private Image[] border;
    [SerializeField] private Color dimColor;
    [SerializeField] private Color brightColor;
    [SerializeField] private Text[] highScores;
    [SerializeField] private Text[] bestTimes;
    private int previousStep = -1;

    private void Start()
    {
        string[] levels = { "Level01", "InnovationScene" };
        for (int i = 0; i < levels.Length; i++)
        {
            highScores[i].text = "HIGH SCORE  " + PlayerPrefs.GetInt(levels[i] + ".HighScore", 0);
            bestTimes[i].text = "BEST TIME  " + GameHUD.FormatTime(PlayerPrefs.GetFloat(levels[i] + ".BestTime", 0f));
        }
    }

    private void Update()
    {
        int step = Mathf.FloorToInt(Time.unscaledTime * 8f) % 8;
        if (step == previousStep) return;
        previousStep = step;
        for (int i = 0; i < border.Length; i++)
            border[i].color = (i + step) % 8 < 3 ? brightColor : dimColor;
    }

    public void OpenLevelOne()
    {
        SceneManager.LoadScene("Level01");
    }

    public void OpenLevelTwo()
    {
        SceneManager.LoadScene("InnovationScene");
    }
}
