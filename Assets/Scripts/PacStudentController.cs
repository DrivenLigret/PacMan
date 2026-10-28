using UnityEngine;

public class PacStudentController : MonoBehaviour
{
    [SerializeField] private LevelGenerator level;
    [SerializeField] private Animator animator;
    [SerializeField] private AudioSource movingAudio;
    [SerializeField] private AudioClip movingClip;
    [SerializeField] private AudioClip eatingClip;
    [SerializeField] private ParticleSystem dust;
    [SerializeField] private float speed = 5f;
    [SerializeField] private GameSession game;
    [SerializeField] private ParticleSystem wallHit;
    [SerializeField] private ParticleSystem deathBurst;
    [SerializeField] private AnimationClip deathAnimation;
    private KeyCode lastInput = KeyCode.None;
    private KeyCode currentInput = KeyCode.None;
    private Vector3 start;
    private Vector3 destination;
    private float progress;
    private bool isLerping;
    private bool moving;
    private Vector3 spawnPosition;
    private KeyCode blockedInput = KeyCode.None;
    public float Speed => speed;

    private void Awake()
    {
        spawnPosition = transform.position;
    }

    private void Start()
    {
        ResetPlayer();
    }

    public void ResetPlayer()
    {
        transform.position = spawnPosition;
        lastInput = KeyCode.None;
        currentInput = KeyCode.None;
        blockedInput = KeyCode.None;
        isLerping = false;
        animator.enabled = true;
        animator.speed = 1f;
        animator.SetBool("Preview", false);
        animator.Play("WalkRight", 0, 0f);
        animator.Update(0f);
        StopMoving();
        dust.Clear();
        deathBurst.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    private void Update()
    {
        if (!game.IsPlaying) return;
        if (Input.GetKeyDown(KeyCode.W)) lastInput = KeyCode.W;
        if (Input.GetKeyDown(KeyCode.A)) lastInput = KeyCode.A;
        if (Input.GetKeyDown(KeyCode.S)) lastInput = KeyCode.S;
        if (Input.GetKeyDown(KeyCode.D)) lastInput = KeyCode.D;
        Move(Time.deltaTime);
    }

    private void Move(float deltaTime)
    {
        float remaining = deltaTime * speed;
        while (true)
        {
            if (!isLerping && !BeginMove(lastInput) && !BeginMove(currentInput))
            {
                StopMoving();
                BumpWall();
                return;
            }
            if (remaining <= 0f) return;
            float step = Mathf.Min(remaining, 1f - progress);
            progress += step;
            remaining -= step;
            Vector3 previous = transform.position;
            transform.position = Vector3.Lerp(start, destination, progress);
            game.CheckPath(previous, transform.position);
            if (!game.IsPlaying) return;
            if (progress < 1f) return;
            transform.position = destination;
            isLerping = false;
            if (level.TryTeleport(Vector2Int.RoundToInt(transform.position), out Vector2Int exit))
            {
                transform.position = new Vector3(exit.x, exit.y, transform.position.z);
                dust.Clear();
            }
        }
    }

    private bool BeginMove(KeyCode input)
    {
        Vector2Int direction;
        string animation;
        switch (input)
        {
            case KeyCode.W: direction = Vector2Int.up; animation = "WalkUp"; break;
            case KeyCode.A: direction = Vector2Int.left; animation = "WalkLeft"; break;
            case KeyCode.S: direction = Vector2Int.down; animation = "WalkDown"; break;
            case KeyCode.D: direction = Vector2Int.right; animation = "WalkRight"; break;
            default: return false;
        }
        Vector2Int next = Vector2Int.RoundToInt(transform.position) + direction;
        int tile = level.GetTile(next);
        if (tile != 0 && tile != 5 && tile != 6) return false;
        start = transform.position;
        destination = new Vector3(next.x, next.y, start.z);
        progress = 0f;
        isLerping = true;
        blockedInput = KeyCode.None;
        animator.speed = 1f;
        if (!moving || currentInput != input) animator.Play(animation, 0, 0f);
        currentInput = input;
        AudioClip clip = tile == 5 || tile == 6 ? eatingClip : movingClip;
        if (!movingAudio.isPlaying || movingAudio.clip != clip)
        {
            movingAudio.clip = clip;
            movingAudio.loop = true;
            movingAudio.Play();
        }
        if (!moving) dust.Play();
        moving = true;
        return true;
    }

    private void StopMoving()
    {
        animator.speed = 0f;
        movingAudio.Stop();
        dust.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        moving = false;
    }

    public void StopForRound()
    {
        StopMoving();
        isLerping = false;
    }

    public void PlayDeath(float duration)
    {
        StopForRound();
        animator.speed = deathAnimation.length / Mathf.Max(.01f, duration);
        animator.Play("Dead", 0, 0f);
        deathBurst.Play();
    }

    private void BumpWall()
    {
        KeyCode input = currentInput != KeyCode.None ? currentInput : lastInput;
        if (input == KeyCode.None || input == blockedInput) return;
        blockedInput = input;
        Vector3 direction = input == KeyCode.W ? Vector3.up : input == KeyCode.S ? Vector3.down : input == KeyCode.A ? Vector3.left : Vector3.right;
        wallHit.transform.position = transform.position + direction * .45f;
        wallHit.Play();
        game.HitWall();
    }
}
