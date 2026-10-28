using UnityEngine;

public class GhostStatus : MonoBehaviour
{
    public enum State { Normal, Scared, Recovering, Dead }
    [SerializeField] private GameSession game;
    [SerializeField] private Animator animator;
    private Vector3 spawnPosition;
    private GhostController movement;
    private Vector2Int facing = Vector2Int.right;
    public State CurrentState { get; private set; }
    public Vector3 SpawnPosition => spawnPosition;

    private void Awake()
    {
        spawnPosition = transform.position;
        movement = GetComponent<GhostController>();
    }

    public void Revive()
    {
        SetState(game.ScaredTime > 3f ? State.Scared : game.ScaredTime > 0f ? State.Recovering : State.Normal);
        game.RefreshMusic();
    }

    public void ApplyScaredTime(float time)
    {
        if (CurrentState != State.Dead)
            SetState(time > 3f ? State.Scared : time > 0f ? State.Recovering : State.Normal);
    }

    public void Die()
    {
        SetState(State.Dead);
    }

    public void ResetGhost()
    {
        transform.position = spawnPosition;
        movement.ResetMovement();
        facing = Vector2Int.right;
        CurrentState = State.Dead;
        SetState(State.Normal);
    }

    public void Freeze(bool value)
    {
        animator.speed = value ? 0f : 1f;
    }

    public void Face(Vector2Int direction)
    {
        if (facing == direction) return;
        facing = direction;
        if (CurrentState == State.Normal || CurrentState == State.Scared) PlayAnimation();
    }

    private void SetState(State value)
    {
        if (CurrentState == value) return;
        CurrentState = value;
        animator.enabled = true;
        animator.speed = 1f;
        PlayAnimation();
    }

    private void PlayAnimation()
    {
        string direction = facing == Vector2Int.up ? "Up" : facing == Vector2Int.down ? "Down" : facing == Vector2Int.left ? "Left" : "Right";
        string state = CurrentState == State.Normal ? "Walk" + direction : CurrentState == State.Scared ? "Scared" + direction : CurrentState.ToString();
        animator.Play(state, 0, 0f);
    }
}
