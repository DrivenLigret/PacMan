using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSession : MonoBehaviour
{
    private enum Phase { Countdown, Playing, Dying, GameOver }
    [SerializeField] private LevelGenerator level;
    [SerializeField] private PacStudentController player;
    [SerializeField] private CherryController cherry;
    [SerializeField] private GhostStatus[] ghosts;
    [SerializeField] private GameHUD hud;
    [SerializeField] private BackgroundMusic music;
    [SerializeField] private AudioSource effects;
    [SerializeField] private AudioClip wallClip;
    [SerializeField] private AudioClip cherryClip;
    [SerializeField] private AudioClip ghostClip;
    [SerializeField] private AudioClip deathClip;
    private Phase phase;
    private int score;
    private int lives = 3;
    private float elapsed;
    private float scaredTime;
    public bool IsPlaying => phase == Phase.Playing;
    public float ScaredTime => scaredTime;

    private IEnumerator Start()
    {
        hud.SetScore(0);
        hud.SetLives(lives);
        hud.SetTimers(0f, 0f);
        foreach (GhostStatus ghost in ghosts) { ghost.ResetGhost(); ghost.Freeze(true); }
        music.PlayIntro();
        foreach (string value in new[] { "3", "2", "1", "GO!" })
        {
            hud.ShowMessage(value);
            yield return new WaitForSeconds(1f);
        }
        hud.ShowMessage(null);
        phase = Phase.Playing;
        foreach (GhostStatus ghost in ghosts) ghost.Freeze(false);
        RefreshMusic();
    }

    private void Update()
    {
        if (phase == Phase.Playing || phase == Phase.Dying) elapsed += Time.deltaTime;
        if (IsPlaying && scaredTime > 0f)
        {
            scaredTime = Mathf.Max(0f, scaredTime - Time.deltaTime);
            foreach (GhostStatus ghost in ghosts) ghost.ApplyScaredTime(scaredTime);
            RefreshMusic();
        }
        hud.SetTimers(elapsed, scaredTime);
    }

    private void LateUpdate()
    {
        if (IsPlaying) CheckPath(player.transform.position, player.transform.position);
    }

    public void CheckPath(Vector3 from, Vector3 to)
    {
        if (!IsPlaying) return;
        Vector2Int cell = Vector2Int.RoundToInt(to);
        if (Vector2.Distance(to, cell) <= .25f)
        {
            int tile = level.ConsumePellet(cell);
            if (tile != 0)
            {
                AddScore(tile == 6 ? 50 : 10);
                if (tile == 6)
                {
                    scaredTime = 10f;
                    foreach (GhostStatus ghost in ghosts) ghost.ApplyScaredTime(scaredTime);
                    RefreshMusic();
                }
                if (level.RemainingPellets == 0) { EndGame(); return; }
            }
        }
        if (cherry.Current && DistanceToSegment(cherry.Current.position, from, to) < .55f)
            CollectCherry();
        foreach (GhostStatus ghost in ghosts)
        {
            if (ghost.CurrentState == GhostStatus.State.Dead || DistanceToSegment(ghost.transform.position, from, to) >= .55f) continue;
            if (ghost.CurrentState == GhostStatus.State.Normal)
            {
                StartCoroutine(LoseLife());
                return;
            }
            ghost.Die();
            effects.PlayOneShot(ghostClip);
            AddScore(300);
            RefreshMusic();
        }
    }

    public void CheckCherryPath(Vector3 from, Vector3 to)
    {
        if (IsPlaying && cherry.Current && DistanceToSegment(player.transform.position, from, to) < .55f)
            CollectCherry();
    }

    public void HitWall()
    {
        if (IsPlaying) effects.PlayOneShot(wallClip);
    }

    public void RefreshMusic()
    {
        if (!IsPlaying) return;
        bool anyDead = false;
        foreach (GhostStatus ghost in ghosts) anyDead |= ghost.CurrentState == GhostStatus.State.Dead;
        music.SetState(anyDead, scaredTime > 0f);
    }

    private void CollectCherry()
    {
        cherry.Collect();
        AddScore(100);
        effects.PlayOneShot(cherryClip);
    }

    private void AddScore(int amount)
    {
        score += amount;
        hud.SetScore(score);
    }

    private IEnumerator LoseLife()
    {
        phase = Phase.Dying;
        lives--;
        hud.SetLives(lives);
        player.PlayDeath(deathClip.length);
        foreach (GhostStatus ghost in ghosts) ghost.Freeze(true);
        music.StopMusic();
        effects.PlayOneShot(deathClip);
        yield return new WaitForSeconds(deathClip.length);
        if (lives == 0) { EndGame(); yield break; }
        scaredTime = 0f;
        player.ResetPlayer();
        foreach (GhostStatus ghost in ghosts) ghost.ResetGhost();
        phase = Phase.Playing;
        RefreshMusic();
    }

    private void EndGame()
    {
        if (phase == Phase.GameOver) return;
        phase = Phase.GameOver;
        player.StopForRound();
        foreach (GhostStatus ghost in ghosts) ghost.Freeze(true);
        music.StopMusic();
        hud.ShowMessage("Game Over");
        string key = SceneManager.GetActiveScene().name;
        int previous = PlayerPrefs.GetInt(key + ".HighScore", 0);
        float previousTime = PlayerPrefs.GetFloat(key + ".BestTime", float.MaxValue);
        if (!PlayerPrefs.HasKey(key + ".HighScore") || score > previous || score == previous && elapsed < previousTime)
        {
            PlayerPrefs.SetInt(key + ".HighScore", score);
            PlayerPrefs.SetFloat(key + ".BestTime", elapsed);
            PlayerPrefs.Save();
        }
        StartCoroutine(ReturnToMenu());
    }

    private IEnumerator ReturnToMenu()
    {
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene("StartScene");
    }

    private static float DistanceToSegment(Vector2 point, Vector2 from, Vector2 to)
    {
        Vector2 line = to - from;
        float t = line.sqrMagnitude > 0f ? Mathf.Clamp01(Vector2.Dot(point - from, line) / line.sqrMagnitude) : 0f;
        return Vector2.Distance(point, from + line * t);
    }
}
