using System.Collections.Generic;
using UnityEngine;

public class GhostController : MonoBehaviour
{
    [SerializeField] private GameSession game;
    [SerializeField] private LevelGenerator level;
    [SerializeField] private PacStudentController player;
    [SerializeField] private GhostStatus status;
    [SerializeField] private int number;
    [SerializeField] private Vector2Int exitCell;
    private static readonly Vector2Int[] directions = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
    private readonly List<Vector2Int> choices = new List<Vector2Int>(4);
    private readonly List<Vector2Int> preferred = new List<Vector2Int>(4);
    private Vector2Int previousCell;
    private Vector2Int facing = Vector2Int.right;
    private bool hasPrevious;
    private bool leaving = true;
    private bool followingOuterWall;
    private bool isLerping;
    private bool returning;
    private Vector3 start;
    private Vector3 destination;
    private float travelled;
    private GhostStatus.State previousState;

    public void ResetMovement()
    {
        hasPrevious = false;
        leaving = true;
        followingOuterWall = false;
        isLerping = false;
        returning = false;
        facing = Vector2Int.right;
        previousState = GhostStatus.State.Normal;
    }

    private void Update()
    {
        if (game.IsPlaying) Move(Time.deltaTime);
    }

    private void Move(float deltaTime)
    {
        float remaining = deltaTime;
        while (remaining > 0f && game.IsPlaying)
        {
            GhostStatus.State state = status.CurrentState;
            if (state == GhostStatus.State.Normal && previousState != state) followingOuterWall = false;
            previousState = state;
            float speed = player.Speed * .9f * (state == GhostStatus.State.Normal ? 1f : .5f);
            if (state == GhostStatus.State.Dead)
            {
                if (!returning)
                {
                    start = transform.position;
                    destination = status.SpawnPosition;
                    travelled = 0f;
                    returning = true;
                    isLerping = false;
                }
                float length = Vector3.Distance(start, destination);
                float step = Mathf.Min(remaining * speed, Mathf.Max(0f, length - travelled));
                travelled += step;
                remaining -= step / speed;
                transform.position = length > 0f ? Vector3.Lerp(start, destination, travelled / length) : destination;
                if (travelled < length) return;
                transform.position = destination;
                ResetMovement();
                status.Revive();
                continue;
            }
            if (!isLerping)
            {
                Vector2Int current = Vector2Int.RoundToInt(transform.position);
                if (leaving && current == exitCell) leaving = false;
                Vector2Int next = leaving ? ExitStep(current) : ChooseNext(current);
                if (next == current) return;
                start = transform.position;
                destination = new Vector3(next.x, next.y, start.z);
                travelled = 0f;
                isLerping = true;
                facing = next - current;
                status.Face(facing);
            }
            Vector3 before = transform.position;
            float distance = Mathf.Min(remaining * speed, 1f - travelled);
            travelled += distance;
            remaining -= distance / speed;
            transform.position = Vector3.Lerp(start, destination, travelled);
            game.CheckGhostPath(status, before, transform.position);
            if (!game.IsPlaying) return;
            if (status.CurrentState == GhostStatus.State.Dead) { isLerping = false; continue; }
            if (travelled < 1f) return;
            transform.position = destination;
            previousCell = Vector2Int.RoundToInt(start);
            hasPrevious = true;
            isLerping = false;
        }
    }

    private Vector2Int ExitStep(Vector2Int current)
    {
        if (current.x != exitCell.x) return current + new Vector2Int(current.x < exitCell.x ? 1 : -1, 0);
        return current + new Vector2Int(0, current.y < exitCell.y ? 1 : -1);
    }

    private bool Walkable(Vector2Int cell)
    {
        int tile = level.GetTile(cell);
        return (tile == 0 || tile == 5 || tile == 6) && !level.TryTeleport(cell, out _);
    }

    private Vector2Int ChooseNext(Vector2Int current)
    {
        choices.Clear();
        foreach (Vector2Int direction in directions)
        {
            Vector2Int next = current + direction;
            if (Walkable(next) && (!hasPrevious || next != previousCell)) choices.Add(next);
        }
        if (choices.Count == 0) return hasPrevious && Walkable(previousCell) ? previousCell : current;
        if (number == 4 && status.CurrentState == GhostStatus.State.Normal) return ClockwiseStep(current);
        bool flee = number == 1 || status.CurrentState != GhostStatus.State.Normal;
        bool chase = number == 2 && status.CurrentState == GhostStatus.State.Normal;
        if (!flee && !chase) return choices[Random.Range(0, choices.Count)];
        preferred.Clear();
        float currentDistance = ((Vector2)current - (Vector2)player.transform.position).sqrMagnitude;
        foreach (Vector2Int next in choices)
        {
            float distance = ((Vector2)next - (Vector2)player.transform.position).sqrMagnitude;
            if (flee ? distance >= currentDistance : distance <= currentDistance) preferred.Add(next);
        }
        var available = preferred.Count > 0 ? preferred : choices;
        return available[Random.Range(0, available.Count)];
    }

    private Vector2Int ClockwiseStep(Vector2Int current)
    {
        if (current == new Vector2Int(1, -1) && previousCell == new Vector2Int(1, -2)) followingOuterWall = true;
        if (!followingOuterWall) return FindOuterStep(current);
        int index = 0;
        while (directions[index] != facing) index++;
        for (int turn = -1; turn <= 1; turn++)
        {
            Vector2Int next = current + directions[(index + turn + 4) % 4];
            if (choices.Contains(next)) return next;
        }
        return choices[0];
    }

    private Vector2Int FindOuterStep(Vector2Int current)
    {
        Vector2Int target = new Vector2Int(1, -1);
        var queue = new Queue<Vector2Int>();
        var parents = new Dictionary<Vector2Int, Vector2Int>();
        queue.Enqueue(current);
        parents.Add(current, current);
        while (queue.Count > 0)
        {
            Vector2Int cell = queue.Dequeue();
            foreach (Vector2Int direction in directions)
            {
                Vector2Int next = cell + direction;
                if (!Walkable(next) || cell == current && !choices.Contains(next)) continue;
                if (next == target)
                {
                    if (cell != new Vector2Int(1, -2)) continue;
                    if (cell == current) return next;
                    while (parents[cell] != current) cell = parents[cell];
                    return cell;
                }
                if (parents.ContainsKey(next)) continue;
                parents.Add(next, cell);
                queue.Enqueue(next);
            }
        }
        return choices[0];
    }
}
