using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameHUD : MonoBehaviour
{
    [SerializeField] private Image[] lives;
    [SerializeField] private Text score;
    [SerializeField] private Text timer;
    [SerializeField] private Text scaredTimer;
    [SerializeField] private GameObject overlay;
    [SerializeField] private Text message;
    [SerializeField] private CanvasGroup controls;

    public void SetScore(int value)
    {
        score.text = value.ToString("D6");
    }

    public void SetLives(int value)
    {
        for (int i = 0; i < lives.Length; i++) lives[i].gameObject.SetActive(i < value);
    }

    public void SetTimers(float elapsed, float scared)
    {
        timer.text = FormatTime(elapsed);
        scaredTimer.gameObject.SetActive(scared > 0f);
        scaredTimer.text = Mathf.CeilToInt(scared).ToString();
    }

    public void ShowMessage(string value)
    {
        overlay.SetActive(value != null);
        message.text = value ?? "";
        controls.interactable = value == null;
    }

    public static string FormatTime(float value)
    {
        int hundredths = Mathf.FloorToInt(value * 100f);
        return (hundredths / 6000).ToString("D2") + ":" + (hundredths / 100 % 60).ToString("D2") + ":" + (hundredths % 100).ToString("D2");
    }

    public void ExitLevel()
    {
        if (!overlay.activeSelf) SceneManager.LoadScene("StartScene");
    }
}
