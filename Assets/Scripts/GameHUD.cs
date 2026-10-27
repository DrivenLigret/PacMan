using UnityEngine;
using UnityEngine.SceneManagement;

public class GameHUD : MonoBehaviour
{
    public void ExitLevel()
    {
        SceneManager.LoadScene("StartScene");
    }
}
