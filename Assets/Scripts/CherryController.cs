using UnityEngine;

public class CherryController : MonoBehaviour
{
    [SerializeField] private LevelGenerator level;
    [SerializeField] private Camera gameCamera;
    [SerializeField] private SpriteRenderer cherryPrefab;
    [SerializeField] private SpriteMask mapMask;
    [SerializeField] private float speed = 3f;
    private SpriteRenderer cherry;
    private Vector3 start;
    private Vector3 destination;
    private float distance;
    private float travelled;
    private float delay = 5f;

    private void Start()
    {
        mapMask.transform.position = new Vector3((level.Width - 1) / 2f, -(level.Height - 1) / 2f, 0f);
        Vector3 size = mapMask.sprite.bounds.size;
        mapMask.transform.localScale = new Vector3(level.Width / size.x, level.Height / size.y, 1f);
    }

    private void Update()
    {
        if (!cherry)
        {
            delay -= Time.deltaTime;
            if (delay <= 0f) Spawn();
            return;
        }
        travelled += speed * Time.deltaTime;
        cherry.transform.position = Vector3.Lerp(start, destination, travelled / distance);
        if (travelled >= distance)
        {
            Destroy(cherry.gameObject);
            cherry = null;
            delay = 5f;
        }
    }

    private void Spawn()
    {
        Vector2 direction = Random.insideUnitCircle.normalized;
        if (direction == Vector2.zero) direction = Vector2.right;
        Vector3 center = mapMask.transform.position;
        float xDistance = Mathf.Abs(direction.x) < .0001f ? float.PositiveInfinity : level.Width / (2f * Mathf.Abs(direction.x));
        float yDistance = Mathf.Abs(direction.y) < .0001f ? float.PositiveInfinity : level.Height / (2f * Mathf.Abs(direction.y));
        float radius = Mathf.Min(xDistance, yDistance) + 1f;
        start = center + (Vector3)direction * radius;
        float cameraWidth = gameCamera.orthographicSize * gameCamera.aspect;
        float endDistance = Vector2.Distance(center, gameCamera.transform.position) + Mathf.Sqrt(cameraWidth * cameraWidth + gameCamera.orthographicSize * gameCamera.orthographicSize) + 2f;
        destination = center - (Vector3)direction * endDistance;
        distance = Vector3.Distance(start, destination);
        travelled = 0f;
        cherry = Instantiate(cherryPrefab, start, Quaternion.identity, transform);
        delay = 5f;
    }
}
