using UnityEngine;

public class PacManMovement : MonoBehaviour
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

    private string[] animations = { "WalkRight", "WalkDown", "WalkLeft", "WalkUp" };
    private int corner;
    private float distance;

    private void Start()
    {
        speed = Mathf.Max(0.1f, speed);
        transform.position = corners[0];
        animator.SetBool("Preview", false);
        animator.Play(animations[0]);
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
            animator.Play(animations[corner], 0, 0f);
        }

        transform.position = Vector3.Lerp(corners[corner], corners[next], distance / length);
    }
}
