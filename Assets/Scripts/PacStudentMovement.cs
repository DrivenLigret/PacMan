using UnityEngine;

public class PacStudentMovement : MonoBehaviour
{
    [SerializeField] private float speed = 3f;
    [SerializeField] private Animator animator;
    [SerializeField] private AudioSource movingAudio;
    [SerializeField] private AudioClip movingClip;

    private Vector3[] corners =
    {
        new Vector3(1f, -1f, 0f),
        new Vector3(6f, -1f, 0f),
        new Vector3(6f, -5f, 0f),
        new Vector3(1f, -5f, 0f)
    };

    private int corner;
    private float distance;

    private void Start()
    {
        speed = Mathf.Max(0.1f, speed);
        transform.position = corners[0];
        if (animator != null)
        {
            animator.Play("WalkUp");
        }
        movingAudio.clip = movingClip;
        movingAudio.loop = true;
        movingAudio.Play();
    }

    private void Update()
    {
        distance += speed * Time.deltaTime;
        int next = (corner + 1) % corners.Length;
        float length = Vector3.Distance(corners[corner], corners[next]);

        while (distance >= length)
        {
            distance -= length;
            corner = next;
            next = (corner + 1) % corners.Length;
            length = Vector3.Distance(corners[corner], corners[next]);
            if (animator != null && corners[next].y != corners[corner].y)
            {
                animator.Play(corners[next].y > corners[corner].y ? "WalkUp" : "WalkDown");
            }
        }

        transform.position = Vector3.Lerp(corners[corner], corners[next], distance / length);
    }
}
