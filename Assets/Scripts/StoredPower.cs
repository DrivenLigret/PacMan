using UnityEngine;
using UnityEngine.UI;

public class StoredPower : MonoBehaviour
{
    [SerializeField] private GameSession game;
    [SerializeField] private Text display;
    private int charges;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) Activate();
        display.text = "STORED POWER\n" + charges + " / 3\n\n" + (game.ScaredTime > 0f ? "POWER ACTIVE" : charges > 0 ? "SPACE TO ACTIVATE" : "COLLECT POWER") + "\n\nStore power pellets.\nSPACE: 10s power.\nKeep charges on death.";
    }

    public void Collect()
    {
        charges = Mathf.Min(3, charges + 1);
    }

    public void Activate()
    {
        if (!game.IsPlaying || charges == 0 || game.ScaredTime > 0f) return;
        charges--;
        game.FrightenGhosts();
    }
}
