using UnityEngine;

public class GhostStatus : MonoBehaviour
{
    public enum State { Normal, Scared, Recovering, Dead }
    [SerializeField] private GameSession game;
    [SerializeField] private Animator animator;
    private Vector3 spawnPosition;
    private float deadTime;
    public State CurrentState { get; private set; }

    private void Awake()
    {
        spawnPosition = transform.position;
    }

    private void Update()
    {
        if (!game.IsPlaying || CurrentState != State.Dead) return;
        deadTime -= Time.deltaTime;
        if (deadTime <= 0f)
        {
            SetState(game.ScaredTime > 3f ? State.Scared : game.ScaredTime > 0f ? State.Recovering : State.Normal);
            game.RefreshMusic();
        }
    }

    public void ApplyScaredTime(float time)
    {
        if (CurrentState != State.Dead)
            SetState(time > 3f ? State.Scared : time > 0f ? State.Recovering : State.Normal);
    }

    public void Die()
    {
        deadTime = 3f;
        SetState(State.Dead);
    }

    public void ResetGhost()
    {
        transform.position = spawnPosition;
        deadTime = 0f;
        CurrentState = State.Dead;
        SetState(State.Normal);
    }

    public void Freeze(bool value)
    {
        animator.speed = value ? 0f : 1f;
    }

    private void SetState(State value)
    {
        if (CurrentState == value) return;
        CurrentState = value;
        animator.enabled = true;
        animator.speed = 1f;
        animator.Play(value == State.Normal ? "WalkRight" : value.ToString(), 0, 0f);
    }
}
